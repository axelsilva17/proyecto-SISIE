using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface ICategoriaService
{
    Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null);
    Task<IEnumerable<CategoriaDTO>> ObtenerTodosAsyncCategoria();
    Task<CategoriaDTO?> ObtenerPorIdAsyncCategoria(int id);
    Task<CategoriaDTO> CrearAsyncCategoria(CategoriaCreateDTO categoria);
    Task<CategoriaDTO?> ActualizarAsyncCategoria(int id, CategoriaCreateDTO categoria);
    Task<bool> EliminarAsyncCategoria(int id);
    Task<bool> PuedeEliminarAsync(int id);
}