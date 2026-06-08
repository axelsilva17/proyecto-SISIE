using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IValidadorVenta
{
    Task<List<string>> ValidarDatosVenta(VentaCreateDTO dto, int idUsuario);
    Task<List<string>> ValidarDatosVentaUpdate(VentaUpdateDTO dto);
    Task<List<string>> ValidarStockProducto(int idProducto, int cantidad);
}
