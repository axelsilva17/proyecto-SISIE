using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ProductoService : IProductoService
{
    private readonly IProductoRepositorio _productoRepositorio;
    private readonly IValidadorProducto _validador;
    private readonly IValidadorVenta _validadorVenta;

    public ProductoService(IProductoRepositorio productoRepositorio, IValidadorProducto validador, IValidadorVenta validadorVenta)
    {
        _productoRepositorio = productoRepositorio;
        _validador = validador;
        _validadorVenta = validadorVenta;
    }


    public async Task<(IEnumerable<ProductoDTO> Items, int Total)> ObtenerTodosAsyncProducto(
        int pagina, int tamanioPagina, int? idCategoria, bool? activo)
    {
        var (items, total) = await _productoRepositorio.ObtenerTodosAsync(
            pagina, tamanioPagina, idCategoria, activo);

        var dtoItems = items.Select(p => new ProductoDTO
        {
            Id = p.Id,
            NombreProducto = p.NombreProducto,
            Descripcion = p.Descripcion,
            PrecioUnitario = p.PrecioUnitario,
            Stock = p.Stock,
            IdCategoria = p.IdCategoria,
            NombreCategoria = p.Categoria?.NombreCategoria,
            FechaCreacion = p.FechaCreacion,
            Activo = p.Activo
        }).ToList();

        return (dtoItems, total);
    }


    public async Task<ProductoDTO?> ObtenerPorIdAsyncProducto(int id)
    {
        var producto = await _productoRepositorio.ObtenerPorIdAsync(id);
        if (producto == null) return null;
        return MapToDTO(producto);
    }


    public async Task<ProductoDTO> CrearAsyncProducto(ProductoCreateDTO dto)
    {
        var erroresNegocio = await _validador.ValidaProducto(dto);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        var producto = new Producto
        {
            NombreProducto = dto.NombreProducto,
            Descripcion = dto.Descripcion,
            PrecioUnitario = dto.PrecioUnitario,
            Stock = dto.Stock,
            IdCategoria = dto.IdCategoria,
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        producto = await _productoRepositorio.CrearAsync(producto);
        return MapToDTO(producto);
    }


    public async Task<ProductoDTO?> ActualizarAsyncProducto(int id, ProductoUpdateDTO dto)
    {
        var producto = await _productoRepositorio.ObtenerPorIdCrudoAsync(id);
        if (producto == null) return null;

        var erroresNegocio = await _validador.ValidaProducto(new ProductoCreateDTO
        {
            NombreProducto = dto.NombreProducto,
            Descripcion = dto.Descripcion,
            PrecioUnitario = dto.PrecioUnitario,
            Stock = dto.Stock,
            IdCategoria = dto.IdCategoria
        }, id);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        producto.NombreProducto = dto.NombreProducto;
        producto.Descripcion = dto.Descripcion;
        producto.PrecioUnitario = dto.PrecioUnitario;
        producto.Stock = dto.Stock;
        producto.IdCategoria = dto.IdCategoria;

        producto = await _productoRepositorio.ActualizarAsync(producto);
        return MapToDTO(producto);
    }


    public async Task<bool> EliminarAsyncProducto(int id)
    {
        return await _productoRepositorio.EliminarLogicoAsync(id);
    }


    public async Task<ProductoDTO?> ToggleActivoAsyncProducto(int id)
    {
        var producto = await _productoRepositorio.ToggleActivoAsync(id);
        return producto == null ? null : MapToDTO(producto);
    }


    private ProductoDTO MapToDTO(Producto producto) => new ProductoDTO
    {
        Id = producto.Id,
        NombreProducto = producto.NombreProducto,
        Descripcion = producto.Descripcion,
        PrecioUnitario = producto.PrecioUnitario,
        Stock = producto.Stock,
        IdCategoria = producto.IdCategoria,
        NombreCategoria = producto.Categoria?.NombreCategoria,
        FechaCreacion = producto.FechaCreacion,
        Activo = producto.Activo
    };


    public async Task<StockVerificacionDTO> VerificarStockProductoAsync(int idProducto, int cantidad)
    {
        var producto = await _productoRepositorio.ObtenerPorIdCrudoAsync(idProducto);
        var errores = await _validadorVenta.ValidarStockProducto(idProducto, cantidad);

        return new StockVerificacionDTO
        {
            IdProducto = idProducto,
            NombreProducto = producto?.NombreProducto,
            StockDisponible = producto?.Stock ?? 0,
            HayStock = errores.Count == 0,
            Mensaje = errores.Count == 0 ? "Stock disponible" : errores.FirstOrDefault()
        };
    }


    public async Task<bool> ActualizarStockAsync(int idProducto, int cantidad)
    {
        return await _productoRepositorio.ActualizarStockAsync(idProducto, cantidad);
    }
}