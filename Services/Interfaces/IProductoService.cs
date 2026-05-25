using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IProductoService
{
    Task<(IEnumerable<ProductoDTO> Items, int Total)> ObtenerTodosAsyncProducto(int pagina, int tamanioPagina, int? idCategoria, bool? activo);
    Task<ProductoDTO?> ObtenerPorIdAsyncProducto(int id);
    Task<ProductoDTO> CrearAsyncProducto(ProductoCreateDTO producto);
    Task<ProductoDTO?> ActualizarAsyncProducto(int id, ProductoUpdateDTO producto);
    Task<bool> EliminarAsyncProducto(int id);
    Task<ProductoDTO?> ToggleActivoAsyncProducto(int id);
    Task<StockVerificacionDTO> VerificarStockProductoAsync(int idProducto, int cantidad);
    Task<bool> ActualizarStockAsync(int idProducto, int cantidad);
}