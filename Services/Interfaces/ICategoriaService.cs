using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaDTO>> GetAllAsync();
    Task<CategoriaDTO?> GetByIdAsync(int id);
    Task<CategoriaDTO> CreateAsync(CategoriaCreateDTO categoria);
    Task<CategoriaDTO?> UpdateAsync(int id, CategoriaCreateDTO categoria);
    Task<bool> DeleteAsync(int id);
    Task<bool> CanDeleteAsync(int id);
}