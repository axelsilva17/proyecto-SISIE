using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface ICategoriaRepositorio
{
    /// <summary>Busca todas las categorías ordenadas por nombre.</summary>
    Task<List<Categoria>> BuscarCategoriasAsync();

    /// <summary>Busca una categoría por ID.</summary>
    Task<Categoria?> BuscarCategoriaPorIdAsync(int id);

    /// <summary>Inserta una categoría en BD.</summary>
    Task<Categoria> InsertarCategoriaAsync(Categoria categoria);

    /// <summary>Modifica una categoría en BD.</summary>
    Task<Categoria> ModificarCategoriaAsync(Categoria categoria);

    /// <summary>Elimina físicamente una categoría.</summary>
    Task<bool> EliminarCategoriaFisicoAsync(int id);

    /// <summary>Verifica si la categoría tiene productos activos vinculados.</summary>
    Task<bool> VerificarProductosActivosAsync(int idCategoria);

    /// <summary>Verifica si ya existe una categoría con ese nombre.</summary>
    Task<bool> VerificarNombreCategoriaExisteAsync(string nombre, int? idExcluir);

    /// <summary>Verifica si existe una categoría por ID.</summary>
    Task<bool> VerificarCategoriaExisteAsync(int id);
}
