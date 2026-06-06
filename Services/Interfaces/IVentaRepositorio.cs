using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface IVentaRepositorio
{
    /// <summary>Busca una venta completa con Usuario, Direccion, Detalles y Productos.</summary>
    Task<Venta?> BuscarVentaConTodoAsync(int id);

    /// <summary>Busca una venta con sus detalles (para cancelaciones).</summary>
    Task<Venta?> BuscarVentaConDetallesAsync(int id);

    /// <summary>Busca una venta sin includes (para cambios de estado).</summary>
    Task<Venta?> BuscarVentaCrudaAsync(int id);

    /// <summary>Inserta una venta en BD.</summary>
    Task<Venta> InsertarVentaAsync(Venta venta);

    /// <summary>Modifica una venta existente en BD.</summary>
    Task<Venta> ModificarVentaAsync(Venta venta);

    /// <summary>Inserta un detalle de venta en BD.</summary>
    Task<DetalleVenta> InsertarDetalleVentaAsync(DetalleVenta detalle);

    /// <summary>Inserta una dirección en BD (devuelve con ID asignado).</summary>
    Task<Direccion> InsertarDireccionAsync(Direccion direccion);

    /// <summary>Verifica si existe un usuario por ID.</summary>
    Task<bool> VerificarUsuarioExisteAsync(int idUsuario);

    /// <summary>Verifica si existe una dirección por ID.</summary>
    Task<bool> VerificarDireccionExisteAsync(int idDireccion);

    /// <summary>Consulta estadísticas de ventas con filtro de fechas.</summary>
    Task<VentasEstadisticas> ConsultarEstadisticasVentasAsync(DateTime? fechaDesde, DateTime? fechaHasta);

    // ===== MÉTODOS CON STORED PROCEDURES =====

    /// <summary>Cancela una venta y restaura stock usando sp_CancelarVenta.</summary>
    Task CancelarVentaConSPAsync(int idVenta);

    /// <summary>Consulta historial paginado usando sp_ObtenerHistorialVentas.</summary>
    Task<(List<VentaHistorialDTO> Items, int Total)> ConsultarHistorialPaginadoAsync(int pagina, int tamanioPagina,
        int? idUsuario, string? estado, DateTime? fechaDesde, DateTime? fechaHasta);
}
