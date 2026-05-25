using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class VentaService : IVentaService
{
    private readonly IVentaRepositorio _ventaRepositorio;
    private readonly IProductoRepositorio _productoRepositorio;
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IValidadorVenta _validador;

    public VentaService(
        IVentaRepositorio ventaRepositorio,
        IProductoRepositorio productoRepositorio,
        IProductoService productoService,
        IClienteService clienteService,
        IValidadorVenta validador)
    {
        _ventaRepositorio = ventaRepositorio;
        _productoRepositorio = productoRepositorio;
        _productoService = productoService;
        _clienteService = clienteService;
        _validador = validador;
    }

    public async Task<VentaDTO> RegistrarVentaAsync(int idUsuario, VentaCreateDTO dto)
    {
        await AutoRegistrarClienteSiCorresponde(dto);

        var erroresNegocio = await _validador.ValidarDatosVentaCreate(dto, idUsuario);
        if (erroresNegocio.Any())
            throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        var idDireccion = await ObtenerOCrearDireccion(dto, idUsuario);
        var venta = await CrearVenta(idUsuario, dto, idDireccion);
        var totalVenta = await ProcesarDetallesVenta(venta.Id, dto.Detalles);
        await AplicarDescuentoYActualizarTotal(venta, dto.Descuento, totalVenta);

        return (await ObtenerVentaDTOCompleto(venta.Id))!;
    }

    private async Task AutoRegistrarClienteSiCorresponde(VentaCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DniCliente) || string.IsNullOrWhiteSpace(dto.NombreCliente))
            return;

        var clienteExistente = await _clienteService.BuscarPorDniAsync(dto.DniCliente);
        if (clienteExistente != null) return;

        var nuevoCliente = new ClienteCreateDTO
        {
            Dni = dto.DniCliente,
            Nombre = dto.NombreCliente,
            Telefono = dto.TelefonoCliente ?? string.Empty,
            Email = dto.EmailCliente?.ToLower()
        };
        await _clienteService.AgregarAsyncCliente(nuevoCliente);
    }

    private async Task<int?> ObtenerOCrearDireccion(VentaCreateDTO dto, int idUsuario)
    {
        if (!dto.EsEnvio || dto.IdDireccion.HasValue)
            return dto.IdDireccion;

        var nuevaDireccion = new Direccion
        {
            Calle = dto.DireccionEnvio!,
            Numero = 1,
            Departamento = dto.Departamento,
            IdCiudad = dto.IdCiudad!.Value,
            IdUsuario = idUsuario
        };
        nuevaDireccion = await _ventaRepositorio.CrearDireccionAsync(nuevaDireccion);
        return nuevaDireccion.Id;
    }

    private async Task<Venta> CrearVenta(int idUsuario, VentaCreateDTO dto, int? idDireccion)
    {
        var numeroVenta = (int)(DateTime.Now.Ticks % 100000000) + new Random().Next(1000, 9999);
        var venta = new Venta
        {
            NumeroVenta = numeroVenta,
            Descuento = dto.Descuento,
            MetodoPago = dto.MetodoPago,
            TipoEntrega = dto.EsEnvio ? "Envío" : "Mostrador",
            Notas = dto.Notas,
            Estado = "Pendiente",
            Total = 0,
            FechaCreacion = DateTime.Now,
            IdDireccion = dto.EsEnvio ? idDireccion : null,
            IdUsuario = idUsuario
        };
        return await _ventaRepositorio.CrearAsync(venta);
    }

    private async Task<decimal> ProcesarDetallesVenta(int idVenta, List<VentaDetalleDTO> detalles)
    {
        decimal total = 0;
        foreach (var detalleDto in detalles)
        {
            var stockVerificacion = await _productoService.VerificarStockProductoAsync(
                detalleDto.IdProducto, detalleDto.Cantidad);
            if (!stockVerificacion.HayStock)
                throw new InvalidOperationException(stockVerificacion.Mensaje);

            var producto = await _productoRepositorio.ObtenerPorIdCrudoAsync(detalleDto.IdProducto);
            var precio = producto?.PrecioUnitario ?? 0;
            var subtotal = detalleDto.Cantidad * precio;

            var detalle = new DetalleVenta
            {
                IdVenta = idVenta,
                IdProducto = detalleDto.IdProducto,
                Cantidad = detalleDto.Cantidad,
                PrecioUnitario = precio,
                SubTotal = subtotal
            };
            await _ventaRepositorio.AgregarDetalleAsync(detalle);
            await _productoService.ActualizarStockAsync(detalleDto.IdProducto, detalleDto.Cantidad);
            total += subtotal;
        }
        return total;
    }

    private async Task AplicarDescuentoYActualizarTotal(Venta venta, int descuento, decimal totalVenta)
    {
        var descuentoDecimal = (decimal)descuento / 100;
        venta.Total = Math.Round(totalVenta * (1 - descuentoDecimal), 2);
        await _ventaRepositorio.ActualizarAsync(venta);
    }

    public async Task<VentaDTO?> ObtenerVentaPorIdAsync(int id) => await ObtenerVentaDTOCompleto(id);

    public async Task<(IEnumerable<VentaHistorialDTO> Items, int Total)> ObtenerHistorialVentasAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado,
        DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var (items, total) = await _ventaRepositorio.ObtenerHistorialAsync(
            pagina, tamanioPagina, idUsuario, estado, fechaDesde, fechaHasta);

        var dtoItems = items.Select(v => new VentaHistorialDTO
        {
            Id = v.Id,
            NumeroVenta = v.NumeroVenta,
            Estado = v.Estado,
            Total = v.Total,
            MetodoPago = v.MetodoPago,
            FechaCreacion = v.FechaCreacion,
            CantidadItems = v.Detalles?.Count ?? 0
        }).ToList();

        return (dtoItems, total);
    }

    public async Task<VentaDTO?> ActualizarEstadoVentaAsync(int id, VentaUpdateDTO dto)
    {
        var venta = await _ventaRepositorio.ObtenerPorIdCrudoAsync(id);
        if (venta == null) return null;

        if (venta.Estado == "Cancelada" || venta.Estado == "Entregada")
            throw new InvalidOperationException($"No se puede modificar una venta en estado '{venta.Estado}'");

        venta.Estado = dto.Estado;
        if (dto.Notas != null) venta.Notas = dto.Notas;

        await _ventaRepositorio.ActualizarAsync(venta);
        return await ObtenerVentaDTOCompleto(id);
    }

    public async Task<VentaDTO?> CancelarVentaAsync(int id)
    {
        var venta = await _ventaRepositorio.ObtenerPorIdConDetallesAsync(id);
        if (venta == null) return null;
        if (venta.Estado == "Cancelada")
            throw new InvalidOperationException("La venta ya está cancelada");
        if (venta.Estado == "Entregada")
            throw new InvalidOperationException("No se puede cancelar una venta entregada");

        foreach (var detalle in venta.Detalles)
            await _productoService.ActualizarStockAsync(detalle.IdProducto, -detalle.Cantidad);

        venta.Estado = "Cancelada";
        await _ventaRepositorio.ActualizarAsync(venta);
        return await ObtenerVentaDTOCompleto(id);
    }

    public async Task<CarritoVerificacionDTO> VerificarStockCarritoAsync(List<VentaDetalleDTO> detalles)
    {
        var resultado = new CarritoVerificacionDTO
        {
            Productos = new List<StockVerificacionDTO>(),
            TodoDisponible = true
        };

        foreach (var detalle in detalles)
        {
            var verificacion = await _productoService.VerificarStockProductoAsync(
                detalle.IdProducto, detalle.Cantidad);
            resultado.Productos.Add(verificacion);
            if (!verificacion.HayStock)
                resultado.TodoDisponible = false;
        }

        return resultado;
    }

    public async Task<PagedResult<VentaHistorialDTO>> ObtenerVentasPorUsuarioAsync(
        int idUsuario, int pagina, int tamanioPagina)
    {
        var existeUsuario = await _ventaRepositorio.ExisteUsuarioAsync(idUsuario);
        if (!existeUsuario)
            throw new InvalidOperationException("El usuario no existe");

        var (items, total) = await ObtenerHistorialVentasAsync(
            pagina, tamanioPagina, idUsuario, null, null, null);
        return new PagedResult<VentaHistorialDTO>
        {
            Items = items,
            Total = total,
            Page = pagina,
            PageSize = tamanioPagina
        };
    }

    public async Task<object> ObtenerEstadisticasVentasAsync(
        DateTime? fechaDesde, DateTime? fechaHasta)
    {
        return await _ventaRepositorio.ObtenerEstadisticasAsync(fechaDesde, fechaHasta);
    }

    private async Task<VentaDTO?> ObtenerVentaDTOCompleto(int idVenta)
    {
        var venta = await _ventaRepositorio.ObtenerPorIdConTodoAsync(idVenta);
        if (venta == null) return null;

        return new VentaDTO
        {
            Id = venta.Id,
            NumeroVenta = venta.NumeroVenta,
            Descuento = venta.Descuento,
            Total = venta.Total,
            MetodoPago = venta.MetodoPago,
            TipoEntrega = venta.TipoEntrega,
            Notas = venta.Notas,
            Estado = venta.Estado,
            FechaCreacion = venta.FechaCreacion,
            IdDireccion = venta.IdDireccion,
            Direccion = venta.Direccion != null
                ? $"{venta.Direccion.Calle} {venta.Direccion.Numero}"
                : null,
            IdUsuario = venta.IdUsuario,
            NombreUsuario = venta.Usuario?.NombreUsuario,
            Detalles = venta.Detalles.Select(d => new DetalleVentaDTO
            {
                Id = d.Id,
                IdProducto = d.IdProducto,
                NombreProducto = d.Producto?.NombreProducto,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                SubTotal = d.SubTotal
            }).ToList()
        };
    }
}