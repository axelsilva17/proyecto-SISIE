using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface IVentaRepositorio
{
    /// <summary>Obtiene una venta completa con Usuario, Direccion, Detalles y Productos.</summary>
    Task<Venta?> ObtenerPorIdConTodoAsync(int id);

    /// <summary>Obtiene una venta con sus detalles (para cancelaciones).</summary>
    Task<Venta?> ObtenerPorIdConDetallesAsync(int id);

    /// <summary>Obtiene una venta sin includes (para cambios de estado).</summary>
    Task<Venta?> ObtenerPorIdCrudoAsync(int id);

    /// <summary>Obtiene historial de ventas paginado con filtros.</summary>
    Task<(List<Venta> Items, int Total)> ObtenerHistorialAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado,
        DateTime? fechaDesde, DateTime? fechaHasta);

    /// <summary>Crea una venta y guarda en BD.</summary>
    Task<Venta> CrearAsync(Venta venta);

    /// <summary>Persiste cambios en una venta existente.</summary>
    Task<Venta> ActualizarAsync(Venta venta);

    /// <summary>Agrega un detalle a una venta y guarda en BD.</summary>
    Task<DetalleVenta> AgregarDetalleAsync(DetalleVenta detalle);

    /// <summary>Crea una dirección y guarda en BD (devuelve con ID asignado).</summary>
    Task<Direccion> CrearDireccionAsync(Direccion direccion);

    /// <summary>Verifica si existe un usuario por ID.</summary>
    Task<bool> ExisteUsuarioAsync(int idUsuario);

    /// <summary>Verifica si existe una dirección por ID.</summary>
    Task<bool> ExisteDireccionAsync(int idDireccion);

    /// <summary>Obtiene estadísticas de ventas con filtro de fechas.</summary>
    Task<VentasEstadisticas> ObtenerEstadisticasAsync(DateTime? fechaDesde, DateTime? fechaHasta);
}
