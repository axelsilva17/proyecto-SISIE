using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface ICategoriaRepositorio
{
    /// <summary>Obtiene todas las categorías ordenadas por nombre.</summary>
    Task<List<Categoria>> ObtenerTodasAsync();

    /// <summary>Obtiene una categoría por ID.</summary>
    Task<Categoria?> ObtenerPorIdAsync(int id);

    /// <summary>Crea una categoría y guarda en BD.</summary>
    Task<Categoria> CrearAsync(Categoria categoria);

    /// <summary>Actualiza una categoría y guarda en BD.</summary>
    Task<Categoria> ActualizarAsync(Categoria categoria);

    /// <summary>Elimina físicamente una categoría.</summary>
    Task<bool> EliminarFisicoAsync(int id);

    /// <summary>Verifica si la categoría tiene productos activos vinculados.</summary>
    Task<bool> TieneProductosActivosAsync(int idCategoria);

    /// <summary>Verifica si ya existe una categoría con ese nombre.</summary>
    Task<bool> ExisteNombreAsync(string nombre, int? idExcluir);

    /// <summary>Verifica si existe una categoría por ID.</summary>
    Task<bool> ExisteAsync(int id);
}
