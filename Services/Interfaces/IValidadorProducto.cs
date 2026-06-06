using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IValidadorProducto
{
    Task<List<string>> ValidarDatosProductoCreate(ProductoCreateDTO dto);
    Task<List<string>> ValidarDatosProductoUpdate(ProductoUpdateDTO dto);
    Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null);
    Task<List<string>> ValidaProductoUpdate(ProductoUpdateDTO dto, int idProducto);
}
