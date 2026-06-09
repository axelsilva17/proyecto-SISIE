using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface IVentaRepositorio
{
    /// Busca una venta completa con Usuario, Direccion, Detalles y Productos.
    Task<Venta?> BuscarVentaConTodoAsync(int id);

    /// Busca una venta con sus detalles (para cancelaciones)
    Task<Venta?> BuscarVentaConDetallesAsync(int id);

    /// Busca una venta sin includes (para cambios de estado)
    Task<Venta?> BuscarVentaCrudaAsync(int id);


    Task<Venta> InsertarVentaAsync(Venta venta);

    /// Modifica una venta existente en BD.
    Task<Venta> ModificarVentaAsync(Venta venta);

    /// Inserta un detalle de venta en BD.
    Task<DetalleVenta> InsertarDetalleVentaAsync(DetalleVenta detalle);

    /// Inserta una dirección en BD (devuelve con ID asignado). 
    Task<Direccion> InsertarDireccionAsync(Direccion direccion);

    /// Verifica si existe un usuario por ID.
    Task<bool> VerificarUsuarioExisteAsync(int idUsuario);

    /// Verifica si existe una dirección por ID.
    Task<bool> VerificarDireccionExisteAsync(int idDireccion);

    /// Consulta estadísticas de ventas con filtro de fechas.   
    Task<VentasEstadisticas> ConsultarEstadisticasVentasAsync(DateTime? fechaDesde, DateTime? fechaHasta);


    Task CancelarVentaConSPAsync(int idVenta);

    Task<(List<VentaHistorialDTO> Items, int Total)> ConsultarHistorialPaginadoAsync(int pagina, int tamanioPagina,
        int? idUsuario, string? estado, DateTime? fechaDesde, DateTime? fechaHasta);
}
