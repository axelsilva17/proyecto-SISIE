using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IValidadorCliente
{
    Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto);
    Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto, int? idCliente);
}
