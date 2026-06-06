using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface IProductoRepositorio
{
    /// <summary>Busca productos paginados con filtros opcionales.</summary>
    Task<(List<Producto> Items, int Total)> BuscarProductosAsync(
        int pagina, int tamanioPagina, int? idCategoria, bool? activo);

    /// <summary>Busca un producto por ID con la categoría incluida.</summary>
    Task<Producto?> BuscarProductoPorIdAsync(int id);

    /// <summary>Busca un producto por ID sin includes (para modificaciones).</summary>
    Task<Producto?> BuscarProductoCrudoAsync(int id);

    /// <summary>Inserta un producto en BD. Devuelve el producto con categoría.</summary>
    Task<Producto> InsertarProductoAsync(Producto producto);

    /// <summary>Modifica un producto existente en BD. Devuelve el producto con categoría.</summary>
    Task<Producto> ModificarProductoAsync(Producto producto);

    /// <summary>Eliminación lógica: marca Activo = false.</summary>
    Task<bool> EliminarProductoLogicoAsync(int id);

    /// <summary>Alterna el estado Activo de un producto.</summary>
    Task<Producto?> AlternarActivoProductoAsync(int id);

    /// <summary>Modifica el stock de un producto. cantidadNegativa permite revertir (valores negativos).</summary>
    Task<bool> ModificarStockProductoAsync(int idProducto, int cantidad);

    /// <summary>Verifica si ya existe un producto activo con ese nombre.</summary>
    Task<bool> VerificarNombreProductoExisteAsync(string nombre, int? idExcluir);

    /// <summary>Verifica si existe un producto por ID.</summary>
    Task<bool> VerificarProductoExisteAsync(int id);
}
