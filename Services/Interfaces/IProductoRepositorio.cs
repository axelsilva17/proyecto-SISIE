using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface IProductoRepositorio
{
    /// <summary>Obtiene productos paginados con filtros opcionales.</summary>
    Task<(List<Producto> Items, int Total)> ObtenerTodosAsync(
        int pagina, int tamanioPagina, int? idCategoria, bool? activo);

    /// <summary>Obtiene un producto por ID con la categoría incluida.</summary>
    Task<Producto?> ObtenerPorIdAsync(int id);

    /// <summary>Obtiene un producto por ID sin includes (para modificaciones).</summary>
    Task<Producto?> ObtenerPorIdCrudoAsync(int id);

    /// <summary>Crea un producto y guarda en BD. Devuelve el producto con categoría.</summary>
    Task<Producto> CrearAsync(Producto producto);

    /// <summary>Persiste cambios en un producto existente. Devuelve el producto con categoría.</summary>
    Task<Producto> ActualizarAsync(Producto producto);

    /// <summary>Eliminación lógica: marca Activo = false.</summary>
    Task<bool> EliminarLogicoAsync(int id);

    /// <summary>Alterna el estado Activo de un producto.</summary>
    Task<Producto?> ToggleActivoAsync(int id);

    /// <summary>Resta stock. cantidadNegativa permite revertir (valores negativos).</summary>
    Task<bool> ActualizarStockAsync(int idProducto, int cantidad);

    /// <summary>Verifica si ya existe un producto activo con ese nombre.</summary>
    Task<bool> ExisteNombreAsync(string nombre, int? idExcluir);

    /// <summary>Verifica si existe un producto por ID.</summary>
    Task<bool> ExisteAsync(int id);
}
