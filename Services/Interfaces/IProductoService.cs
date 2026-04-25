using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IProductoService
{
    // Lista productos con paginación y filtros opcionales
    Task<(IEnumerable<ProductoListDTO> Items, int Total)> ObtenerTodosAsync(int pagina, int tamanioPagina, int? idCategoria, bool? activo);
    
    // Obtiene un producto por su ID
    Task<ProductoDTO?> ObtenerPorIdAsync(int id);
    
    // Crea un nuevo producto
    Task<ProductoDTO> CrearAsync(ProductoCreateDTO producto);
    
    // Actualiza un producto existente
    Task<ProductoDTO?> ActualizarAsync(int id, ProductoUpdateDTO producto);
    
    // Elimina un producto (soft delete)
    Task<bool> EliminarAsync(int id);
    
    // Activa o desactiva un producto
    Task<ProductoDTO?> ToggleActivoAsync(int id);
}