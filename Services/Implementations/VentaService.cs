using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;
using proyecto_SISIE.Services.Strategy;

namespace proyecto_SISIE.Services.Implementations;

public class VentaService : IVentaService
{
    private readonly IVentaRepositorio _ventaRepositorio;
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IValidadorVenta _validador;
    private readonly ProcesadorPago _procesadorPago;

    public VentaService(
        IVentaRepositorio ventaRepositorio,
        IProductoService productoService,
        IClienteService clienteService,
        IValidadorVenta validador,
        ProcesadorPago procesadorPago)
    {
        _ventaRepositorio = ventaRepositorio;
        _productoService = productoService;
        _clienteService = clienteService;
        _validador = validador;
        _procesadorPago = procesadorPago;
    }

    public async Task<VentaDTO> RegistrarVentaAsync(int idUsuario, VentaCreateDTO dto)
    {
        await AutoRegistrarClienteSiCorresponde(dto);

        var erroresNegocio = await _validador.ValidarDatosVentaCreate(dto, idUsuario);
        if (erroresNegocio.Any())
            throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        var idDireccion = await ObtenerOCrearDireccion(dto, idUsuario);
        var venta = await CrearVenta(idUsuario, dto, idDireccion);
        var subtotal = await ProcesarDetallesVenta(venta.Id, dto.Detalles);

        // Actualizar stock de cada producto (según diagrama de secuencia)
        foreach (var detalleDto in dto.Detalles)
            await _productoService.ActualizarStockAsync(detalleDto.IdProducto, detalleDto.Cantidad);

        var totalConMetodoPago = _procesadorPago.CalcularTotal(venta.MetodoPago, subtotal, dto.Descuento);
        venta.Total = totalConMetodoPago;
        await _ventaRepositorio.ModificarVentaAsync(venta);

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
        nuevaDireccion = await _ventaRepositorio.InsertarDireccionAsync(nuevaDireccion);
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
        return await _ventaRepositorio.InsertarVentaAsync(venta);
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

            var precio = stockVerificacion.PrecioUnitario;
            var subtotal = detalleDto.Cantidad * precio;

            var detalle = new DetalleVenta
            {
                IdVenta = idVenta,
                IdProducto = detalleDto.IdProducto,
                Cantidad = detalleDto.Cantidad,
                PrecioUnitario = precio,
                SubTotal = subtotal
            };
            await _ventaRepositorio.InsertarDetalleVentaAsync(detalle);
            total += subtotal;
        }
        return total;
    }

    public async Task<VentaDTO?> ObtenerVentaPorIdAsync(int id) => await ObtenerVentaDTOCompleto(id);

    public async Task<(IEnumerable<VentaHistorialDTO> Items, int Total)> ObtenerHistorialVentasAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado,
        DateTime? fechaDesde, DateTime? fechaHasta)
    {
        return await _ventaRepositorio.ConsultarHistorialPaginadoAsync(
            pagina, tamanioPagina, idUsuario, estado, fechaDesde, fechaHasta);
    }

    public async Task<VentaDTO?> ActualizarEstadoVentaAsync(int id, VentaUpdateDTO dto)
    {
        var venta = await _ventaRepositorio.BuscarVentaCrudaAsync(id);
        if (venta == null) return null;

        if (venta.Estado == "Cancelada" || venta.Estado == "Entregada")
            throw new InvalidOperationException($"No se puede modificar una venta en estado '{venta.Estado}'");

        venta.Estado = dto.Estado;
        if (dto.Notas != null) venta.Notas = dto.Notas;

        await _ventaRepositorio.ModificarVentaAsync(venta);
        return await ObtenerVentaDTOCompleto(id);
    }

    public async Task<VentaDTO?> CancelarVentaAsync(int id)
    {
        // Verificar que la venta existe
        var venta = await _ventaRepositorio.BuscarVentaConDetallesAsync(id);
        if (venta == null) return null;

        // El SP sp_CancelarVenta valida estados y restaura stock en una transacción
        await _ventaRepositorio.CancelarVentaConSPAsync(id);

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
        var existeUsuario = await _ventaRepositorio.VerificarUsuarioExisteAsync(idUsuario);
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
        return await _ventaRepositorio.ConsultarEstadisticasVentasAsync(fechaDesde, fechaHasta);
    }

    private async Task<VentaDTO?> ObtenerVentaDTOCompleto(int idVenta)
    {
        var venta = await _ventaRepositorio.BuscarVentaConTodoAsync(idVenta);
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