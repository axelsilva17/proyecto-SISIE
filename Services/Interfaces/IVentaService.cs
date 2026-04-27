using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

// Interface para el servicio de ventas
public interface IVentaService
{
    // Registra una nueva venta con sus detalles, descuenta stock y calcula total
    Task<VentaDTO> RegistrarVentaAsync(int idUsuario, VentaCreateDTO ventaDto);

    // Obtiene una venta por su ID con todos los detalles
    Task<VentaDTO?> ObtenerVentaPorIdAsync(int id);

    // Obtiene el historial de ventas con paginación y filtros
    Task<(IEnumerable<VentaHistorialDTO> Items, int Total)> ObtenerHistorialVentasAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado, DateTime? fechaDesde, DateTime? fechaHasta);

    // Actualiza el estado de una venta (pendiente -> entregada, cancelada, etc.)
    Task<VentaDTO?> ActualizarEstadoVentaAsync(int id, VentaUpdateDTO updateDto);

    // Cancela una venta y reposiciona el stock (si aplica)
    Task<VentaDTO?> CancelarVentaAsync(int id);

    // Verifica el stock disponible de un producto específico
    Task<StockVerificacionDTO> VerificarStockProductoAsync(int idProducto, int cantidad);

    // Verifica el stock de varios productos (para el carrito)
    Task<CarritoVerificacionDTO> VerificarStockCarritoAsync(List<VentaDetalleDTO> detalles);

    // Obtiene las ventas de un usuario específico
    Task<VentaPagedResult> ObtenerVentasPorUsuarioAsync(int idUsuario, int pagina, int tamanioPagina);

    // Obtiene estadísticas de ventas (total, cantidad, etc.) para el dashboard
    Task<object> ObtenerEstadisticasVentasAsync(DateTime? fechaDesde, DateTime? fechaHasta);
}