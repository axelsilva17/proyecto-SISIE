using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface ICategoriaService
{
    // Lista todas las categorías
    Task<IEnumerable<CategoriaDTO>> ObtenerTodosAsync();
    
    // Obtiene una categoría por su ID
    Task<CategoriaDTO?> ObtenerPorIdAsync(int id);
    
    // Crea una nueva categoría
    Task<CategoriaDTO> CrearAsync(CategoriaCreateDTO categoria);
    
    // Actualiza una categoría existente
    Task<CategoriaDTO?> ActualizarAsync(int id, CategoriaCreateDTO categoria);
    
    // Elimina una categoría
    Task<bool> EliminarAsync(int id);
    
    // Verifica si una categoría se puede eliminar
    Task<bool> PuedeEliminarAsync(int id);
}