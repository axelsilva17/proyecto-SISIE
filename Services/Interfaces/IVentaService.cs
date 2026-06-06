using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IVentaService
{
    Task<VentaDTO> RegistrarVentaAsync(int idUsuario, VentaCreateDTO ventaDto);
    Task<VentaDTO?> ObtenerVentaPorIdAsync(int id);
    Task<(IEnumerable<VentaHistorialDTO> Items, int Total)> ObtenerHistorialVentasAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado, DateTime? fechaDesde, DateTime? fechaHasta);
    Task<VentaDTO?> ActualizarEstadoVentaAsync(int id, VentaUpdateDTO updateDto);
    Task<VentaDTO?> CancelarVentaAsync(int id);
    Task<CarritoVerificacionDTO> VerificarStockCarritoAsync(List<VentaDetalleDTO> detalles);
    Task<PagedResult<VentaHistorialDTO>> ObtenerVentasPorUsuarioAsync(int idUsuario, int pagina, int tamanioPagina);
    Task<object> ObtenerEstadisticasVentasAsync(DateTime? fechaDesde, DateTime? fechaHasta);
}