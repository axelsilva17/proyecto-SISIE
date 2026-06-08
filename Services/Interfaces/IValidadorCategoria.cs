using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IValidadorCategoria
{
    Task<List<string>> ValidarDatosCategoria(CategoriaCreateDTO dto, int? idCategoria);
    Task<List<string>> ValidarCategoriaExiste(int idCategoria);
}
