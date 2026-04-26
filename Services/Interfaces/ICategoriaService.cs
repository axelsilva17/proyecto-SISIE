using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface ICategoriaService
{
    // Lista todas las categorías
    Task<IEnumerable<CategoriaDTO>> ObtenerTodosAsyncCategoria();
    
    // Obtiene una categoría por su ID
    Task<CategoriaDTO?> ObtenerPorIdAsyncCategoria(int id);
    
    // Crea una nueva categoría
    Task<CategoriaDTO> CrearAsyncCategoria(CategoriaCreateDTO categoria);
    
    // Actualiza una categoría existente
    Task<CategoriaDTO?> ActualizarAsyncCategoria(int id, CategoriaCreateDTO categoria);
    
    // Elimina una categoría
    Task<bool> EliminarAsyncCategoria(int id);
    
    // Verifica si una categoría se puede eliminar
    Task<bool> PuedeEliminarAsync(int id);
}