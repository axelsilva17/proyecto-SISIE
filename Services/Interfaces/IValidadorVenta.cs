using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Interfaces;

public interface IValidadorVenta
{
    Task<List<string>> ValidarDatosVenta(VentaCreateDTO dto);
    Task<List<string>> ValidarDatosVentaCreate(VentaCreateDTO dto, int idUsuario);
    Task<List<string>> ValidarDatosVentaUpdate(VentaUpdateDTO dto);
    Task<List<string>> ValidarStockProducto(int idProducto, int cantidad);
}

public class ValidadorVenta : IValidadorVenta
{
    private readonly IVentaRepositorio _ventaRepositorio;
    private readonly IProductoRepositorio _productoRepositorio;

    public ValidadorVenta(IVentaRepositorio ventaRepositorio, IProductoRepositorio productoRepositorio)
    {
        _ventaRepositorio = ventaRepositorio;
        _productoRepositorio = productoRepositorio;
    }

    public Task<List<string>> ValidarDatosVenta(VentaCreateDTO dto)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarDetallesVacios(dto.Detalles));
        errores.AddRange(ValidarMetodoPago(dto.MetodoPago));
        errores.AddRange(ValidarDatosCliente(dto));
        return Task.FromResult(errores);
    }

    public async Task<List<string>> ValidarDatosVentaCreate(VentaCreateDTO dto, int idUsuario)
    {
        var errores = new List<string>();
        errores.AddRange(await ValidarUsuarioExiste(idUsuario));
        errores.AddRange(ValidarDetallesVacios(dto.Detalles));
        errores.AddRange(ValidarEnvio(dto));
        errores.AddRange(await ValidarDireccion(dto));
        errores.AddRange(await ValidarProductosEnDetalles(dto.Detalles));
        return errores;
    }

    public Task<List<string>> ValidarDatosVentaUpdate(VentaUpdateDTO dto)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarEstado(dto.Estado));
        return Task.FromResult(errores);
    }

    public async Task<List<string>> ValidarStockProducto(int idProducto, int cantidad)
    {
        var errores = new List<string>();
        var producto = await _productoRepositorio.ObtenerPorIdCrudoAsync(idProducto);
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
        var existe = await _ventaRepositorio.ExisteUsuarioAsync(idUsuario);
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

    private List<string> ValidarMetodoPago(string? metodoPago)
    {
        return string.IsNullOrWhiteSpace(metodoPago) ? ["El método de pago es obligatorio"] : [];
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
        var direccionValida = await _ventaRepositorio.ExisteDireccionAsync(dto.IdDireccion.Value);
        return direccionValida ? [] : ["La dirección no es válida"];
    }

    private async Task<List<string>> ValidarProductosEnDetalles(List<VentaDetalleDTO>? detalles)
    {
        var errores = new List<string>();
        foreach (var detalle in detalles ?? [])
            errores.AddRange(await ValidarStockProducto(detalle.IdProducto, detalle.Cantidad));
        return errores;
    }

    private List<string> ValidarDatosCliente(VentaCreateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.DniCliente)) return errores;

        if (dto.DniCliente.Length < 7 || dto.DniCliente.Length > 15)
            errores.Add("El DNI del cliente debe tener entre 7 y 15 caracteres");
        if (string.IsNullOrWhiteSpace(dto.NombreCliente))
            errores.Add("El nombre del cliente es obligatorio");
        if (string.IsNullOrWhiteSpace(dto.TelefonoCliente))
            errores.Add("El teléfono del cliente es obligatorio");
        if (!string.IsNullOrWhiteSpace(dto.EmailCliente) && !dto.EmailCliente.Contains("@"))
            errores.Add("El email del cliente debe contener @");
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
