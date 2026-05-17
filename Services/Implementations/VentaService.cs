using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IValidadorVenta _validador;

    public VentaService(ApplicationDbContext context, IProductoService productoService, IClienteService clienteService, IValidadorVenta validador)
    {
        _context = context;
        _productoService = productoService;
        _clienteService = clienteService;
        _validador = validador;
    }

    public async Task<VentaDTO> RegistrarVentaAsync(int idUsuario, VentaCreateDTO dto)
    {
        // Auto-registrar cliente si se proporcionan datos y no existe
        if (!string.IsNullOrWhiteSpace(dto.DniCliente))
        {
            var clienteExistente = await _clienteService.BuscarPorDniAsync(dto.DniCliente);
            if (clienteExistente == null && !string.IsNullOrWhiteSpace(dto.NombreCliente))
            {
                var nuevoCliente = new ClienteCreateDTO
                {
                    Dni = dto.DniCliente,
                    Nombre = dto.NombreCliente,
                    Telefono = dto.TelefonoCliente ?? string.Empty,
                    Email = dto.EmailCliente?.ToLower()
                };
                await _clienteService.AgregarAsyncCliente(nuevoCliente);
            }
        }
        await _validador.ValidarDatosVentaCreate(dto, idUsuario);
        int? idDireccionFinal = dto.IdDireccion;

        if (dto.EsEnvio && !dto.IdDireccion.HasValue)
        {
            var nuevaDireccion = new Direccion { Calle = dto.DireccionEnvio, Numero = 1, Departamento = dto.Departamento, IdCiudad = dto.IdCiudad!.Value, IdUsuario = idUsuario };
            _context.Direcciones.Add(nuevaDireccion);
            await _context.SaveChangesAsync();
            idDireccionFinal = nuevaDireccion.Id;
        }

        var numeroVenta = (int)(DateTime.Now.Ticks % 100000000) + new Random().Next(1000, 9999);
        var venta = new Venta { NumeroVenta = numeroVenta, Descuento = dto.Descuento, MetodoPago = dto.MetodoPago,
            TipoEntrega = dto.EsEnvio ? "Envío" : "Mostrador", Notas = dto.Notas, Estado = "Pendiente", Total = 0,
            FechaCreacion = DateTime.Now, IdDireccion = dto.EsEnvio ? idDireccionFinal : null, IdUsuario = idUsuario };
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        decimal totalVenta = 0;
        foreach (var detalleDto in dto.Detalles)
        {
            var stockVerificacion = await _productoService.VerificarStockProductoAsync(detalleDto.IdProducto, detalleDto.Cantidad);
            if (!stockVerificacion.HayStock) throw new InvalidOperationException(stockVerificacion.Mensaje);

            var producto = await _context.Productos.FindAsync(detalleDto.IdProducto);
            var subtotal = detalleDto.Cantidad * producto!.PrecioUnitario;
            var detalle = new DetalleVenta { IdVenta = venta.Id, IdProducto = detalleDto.IdProducto,
                Cantidad = detalleDto.Cantidad, PrecioUnitario = producto.PrecioUnitario, SubTotal = subtotal };
            _context.DetallesVenta.Add(detalle);
            await _productoService.ActualizarStockAsync(detalleDto.IdProducto, detalleDto.Cantidad);
            totalVenta += subtotal;
        }

        var descuentoDecimal = (decimal)dto.Descuento / 100;
        venta.Total = Math.Round(totalVenta * (1 - descuentoDecimal), 2);
        await _context.SaveChangesAsync();
        return await ObtenerVentaDTOCompleto(venta.Id);
    }

    public async Task<VentaDTO?> ObtenerVentaPorIdAsync(int id) => await ObtenerVentaDTOCompleto(id);

    public async Task<(IEnumerable<VentaHistorialDTO> Items, int Total)> ObtenerHistorialVentasAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var query = _context.Ventas.Include(v => v.Usuario).Include(v => v.Detalles).AsQueryable();
        if (idUsuario.HasValue) query = query.Where(v => v.IdUsuario == idUsuario.Value);
        if (!string.IsNullOrEmpty(estado)) query = query.Where(v => v.Estado.ToLower() == estado.ToLower());
        if (fechaDesde.HasValue) query = query.Where(v => v.FechaCreacion >= fechaDesde.Value);
        if (fechaHasta.HasValue) query = query.Where(v => v.FechaCreacion <= fechaHasta.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(v => v.FechaCreacion).Skip((pagina - 1) * tamanioPagina).Take(tamanioPagina)
            .Select(v => new VentaHistorialDTO { Id = v.Id, NumeroVenta = v.NumeroVenta, Estado = v.Estado, Total = v.Total,
                MetodoPago = v.MetodoPago, FechaCreacion = v.FechaCreacion, CantidadItems = v.Detalles.Count }).ToListAsync();
        return (items, total);
    }

    public async Task<VentaDTO?> ActualizarEstadoVentaAsync(int id, VentaUpdateDTO dto)
    {
        var venta = await _context.Ventas.FindAsync(id);
        if (venta == null) return null;
        if (venta.Estado == "Cancelada" || venta.Estado == "Entregada")
            throw new InvalidOperationException($"No se puede modificar una venta en estado '{venta.Estado}'");
        venta.Estado = dto.Estado;
        if (dto.Notas != null) venta.Notas = dto.Notas;
        await _context.SaveChangesAsync();
        return await ObtenerVentaDTOCompleto(id);
    }

    public async Task<VentaDTO?> CancelarVentaAsync(int id)
    {
        var venta = await _context.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return null;
        if (venta.Estado == "Cancelada") throw new InvalidOperationException("La venta ya está cancelada");
        if (venta.Estado == "Entregada") throw new InvalidOperationException("No se puede cancelar una venta entregada");

        foreach (var detalle in venta.Detalles)
            await _productoService.ActualizarStockAsync(detalle.IdProducto, -detalle.Cantidad);

        venta.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return await ObtenerVentaDTOCompleto(id);
    }

    public async Task<CarritoVerificacionDTO> VerificarStockCarritoAsync(List<VentaDetalleDTO> detalles)
    {
        var resultado = new CarritoVerificacionDTO { Productos = new List<StockVerificacionDTO>(), TodoDisponible = true };
        foreach (var detalle in detalles)
        {
            var verificacion = await _productoService.VerificarStockProductoAsync(detalle.IdProducto, detalle.Cantidad);
            resultado.Productos.Add(verificacion);
            if (!verificacion.HayStock) resultado.TodoDisponible = false;
        }
        return resultado;
    }

    public async Task<VentaPagedResult> ObtenerVentasPorUsuarioAsync(int idUsuario, int pagina, int tamanioPagina)
    {
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null) throw new InvalidOperationException("El usuario no existe");
        var (items, total) = await ObtenerHistorialVentasAsync(pagina, tamanioPagina, idUsuario, null, null, null);
        return new VentaPagedResult { Items = items, Total = total, Page = pagina, PageSize = tamanioPagina };
    }

    public async Task<object> ObtenerEstadisticasVentasAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var query = _context.Ventas.AsQueryable();
        if (fechaDesde.HasValue) query = query.Where(v => v.FechaCreacion >= fechaDesde.Value);
        if (fechaHasta.HasValue) query = query.Where(v => v.FechaCreacion <= fechaHasta.Value);

        var totalVentas = await query.CountAsync();
        var totalFacturado = await query.SumAsync(v => v.Total);
        var ventasCanceladas = await query.CountAsync(v => v.Estado == "Cancelada");
        var ventasPendientes = await query.CountAsync(v => v.Estado == "Pendiente");
        var ventasEntregadas = await query.CountAsync(v => v.Estado == "Entregada");

        return new { TotalVentas = totalVentas, TotalFacturado = totalFacturado, VentasCanceladas = ventasCanceladas,
            VentasPendientes = ventasPendientes, VentasEntregadas = ventasEntregadas, FechaDesde = fechaDesde, FechaHasta = fechaHasta };
    }

    private async Task<VentaDTO?> ObtenerVentaDTOCompleto(int idVenta)
    {
        var venta = await _context.Ventas.Include(v => v.Usuario).Include(v => v.Direccion).Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == idVenta);
        if (venta == null) return null;

        return new VentaDTO { Id = venta.Id, NumeroVenta = venta.NumeroVenta, Descuento = venta.Descuento, Total = venta.Total,
            MetodoPago = venta.MetodoPago, TipoEntrega = venta.TipoEntrega, Notas = venta.Notas, Estado = venta.Estado,
            FechaCreacion = venta.FechaCreacion, IdDireccion = venta.IdDireccion,
            Direccion = venta.Direccion != null ? $"{venta.Direccion.Calle} {venta.Direccion.Numero}" : null,
            IdUsuario = venta.IdUsuario, NombreUsuario = venta.Usuario?.NombreUsuario,
            Detalles = venta.Detalles.Select(d => new DetalleVentaDTO { Id = d.Id, IdProducto = d.IdProducto,
                NombreProducto = d.Producto?.NombreProducto, Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario, SubTotal = d.SubTotal }).ToList() };
    }
}