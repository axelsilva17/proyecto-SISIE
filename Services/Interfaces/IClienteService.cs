using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IClienteService
{
    Task<(IEnumerable<ClienteDTO> Items, int Total)> ObtenerTodosAsync(int pagina, int tamanioPagina, string? nombre, bool? activo);
    Task<ClienteDTO?> ObtenerPorIdAsync(int id);
    Task<ClienteDTO?> BuscarPorDniAsync(string dni);
    Task<ClienteDTO> AgregarAsyncCliente(ClienteCreateDTO clienteDto);
}