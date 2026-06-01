using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IValidadorCategoria
{
    Task<List<string>> ValidarDatosCategoria(CategoriaCreateDTO dto);
    Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null);
    Task<List<string>> ValidarCategoriaExiste(int idCategoria);
}
