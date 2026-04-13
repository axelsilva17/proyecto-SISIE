using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IProductoService
{
    Task<(IEnumerable<ProductoListDTO> Items, int Total)> GetAllAsync(int page, int pageSize, int? idCategoria, bool? activo);
    Task<ProductoDTO?> GetByIdAsync(int id);
    Task<ProductoDTO> CreateAsync(ProductoCreateDTO producto);
    Task<ProductoDTO?> UpdateAsync(int id, ProductoUpdateDTO producto);
    Task<bool> DeleteAsync(int id);
}