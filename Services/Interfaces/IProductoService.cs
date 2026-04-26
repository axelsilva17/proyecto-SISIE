using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IProductoService
{
    // Lista productos con paginación y filtros opcionales
    Task<(IEnumerable<ProductoListDTO> Items, int Total)> ObtenerTodosAsyncProducto(int pagina, int tamanioPagina, int? idCategoria, bool? activo);
    
    // Obtiene un producto por su ID
    Task<ProductoDTO?> ObtenerPorIdAsyncProducto(int id);
    
    // Crea un nuevo producto
    Task<ProductoDTO> CrearAsyncProducto(ProductoCreateDTO producto);
    
    // Actualiza un producto existente
    Task<ProductoDTO?> ActualizarAsyncProducto(int id, ProductoUpdateDTO producto);
    
    // Elimina un producto (soft delete)
    Task<bool> EliminarAsyncProducto(int id);
    
    // Activa o desactiva un producto
    Task<ProductoDTO?> ToggleActivoAsyncProducto(int id);
}