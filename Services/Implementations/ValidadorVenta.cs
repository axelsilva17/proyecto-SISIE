using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ValidadorVenta : IValidadorVenta
{
    private readonly IVentaRepositorio _ventaRepositorio;
    private readonly IProductoRepositorio _productoRepositorio;

    public ValidadorVenta(IVentaRepositorio ventaRepositorio, IProductoRepositorio productoRepositorio)
    {
        _ventaRepositorio = ventaRepositorio;
        _productoRepositorio = productoRepositorio;
    }

    // Valida los datos de la venta antes de crearla o actualizarla
    public async Task<List<string>> ValidarDatosVenta(VentaCreateDTO dto, int idUsuario)
    {
        var errores = new List<string>();
        errores.AddRange(await ValidarUsuarioExiste(idUsuario));
        errores.AddRange(ValidarDetallesVacios(dto.Detalles));
        errores.AddRange(ValidarMetodoPago(dto.IdMetodoPago));
        errores.AddRange(ValidarEnvio(dto));
        errores.AddRange(await ValidarDireccion(dto));
        errores.AddRange(await ValidarProductosEnDetalles(dto.Detalles));
        return errores;
    }
    // Valida los datos de la venta antes de actualizar su estado
    public Task<List<string>> ValidarDatosVentaUpdate(VentaUpdateDTO dto)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarEstado(dto.Estado));
        return Task.FromResult(errores);
    }

    // Valida que el producto exista, esté activo y tenga stock suficiente para la cantidad solicitada
    public async Task<List<string>> ValidarStockProducto(int idProducto, int cantidad)
    {
        var errores = new List<string>();
        var producto = await _productoRepositorio.BuscarProductoCrudoAsync(idProducto);
        if (producto == null)
            errores.Add($"El producto con ID {idProducto} no existe");
        else if (!producto.Activo)
            errores.Add($"El producto '{producto.NombreProducto}' está inactivo");
        else if (producto.Stock < cantidad)
            errores.Add($"Stock insuficiente para '{producto.NombreProducto}'. Disponible: {producto.Stock}");
        return errores;
    }
    

    private async Task<List<string>> ValidarUsuarioExiste(int idUsuario)
    {
        var existe = await _ventaRepositorio.VerificarUsuarioExisteAsync(idUsuario);
        return existe ? [] : ["El usuario no existe"];
    }

    private List<string> ValidarDetallesVacios(List<VentaDetalleDTO>? detalles)
    {
        if (detalles == null || detalles.Count == 0)
            return ["Debe incluir al menos un producto"];

        var errores = new List<string>();
        foreach (var d in detalles)
        {
            if (d.IdProducto <= 0) errores.Add("Producto inválido");
            if (d.Cantidad <= 0) errores.Add("La cantidad debe ser mayor a 0");
        }
        return errores;
    }

    
    private List<string> ValidarMetodoPago(int idMetodoPago)
    {
        var errores = new List<string>();
        if (idMetodoPago <= 0)
            errores.Add("El método de pago es obligatorio");
        return errores;
    }

    private List<string> ValidarEnvio(VentaCreateDTO dto)
    {
        if (!dto.EsEnvio) return [];

        var errores = new List<string>();
        if (!dto.IdCiudad.HasValue || dto.IdCiudad.Value <= 0)
            errores.Add("Debe seleccionar una ciudad para envío a domicilio");
        if (!dto.IdDireccion.HasValue && string.IsNullOrWhiteSpace(dto.DireccionEnvio))
            errores.Add("Debe indicar la dirección para envío a domicilio");
        return errores;
    }

    private async Task<List<string>> ValidarDireccion(VentaCreateDTO dto)
    {
        if (!dto.IdDireccion.HasValue) return [];
        var direccionValida = await _ventaRepositorio.VerificarDireccionExisteAsync(dto.IdDireccion.Value);
        return direccionValida ? [] : ["La dirección no es válida"];
    }

    // Valida cada producto en los detalles de la venta para asegurarse de que existan, estén activos y tengan stock suficiente
    private async Task<List<string>> ValidarProductosEnDetalles(List<VentaDetalleDTO>? detalles)
    {
        var errores = new List<string>();
        foreach (var detalle in detalles ?? [])
            errores.AddRange(await ValidarStockProducto(detalle.IdProducto, detalle.Cantidad));
        return errores;
    }

    private List<string> ValidarEstado(string? estado)
    {
        if (string.IsNullOrWhiteSpace(estado)) return ["El estado es obligatorio"];

        var estadosValidos = new[] { "Pendiente", "Pagada", "Enviada", "Entregada", "Cancelada" };
        return !estadosValidos.Contains(estado)
            ? [$"Estado inválido. Estados válidos: {string.Join(", ", estadosValidos)}"]
            : [];
    }
}
