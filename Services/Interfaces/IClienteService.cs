using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

// Interface para el servicio de clientes
public interface IClienteService
{
    // Lista clientes con paginación y filtros
    Task<(IEnumerable<ClienteDTO> Items, int Total)> ObtenerTodosAsync(int pagina, int tamanioPagina, string? nombre, bool? activo);
    
    // Obtiene un cliente por su ID
    Task<ClienteDTO?> ObtenerPorIdAsync(int id);
    
    // Busca un cliente por su DNI
    Task<ClienteDTO?> BuscarPorDniAsync(string dni);
    
    // Agrega un nuevo cliente
    Task<ClienteDTO> AgregarAsyncCliente(ClienteCreateDTO clienteDto);

    // Valida los datos de un cliente (público para usar desde otros servicios)
    Task ValidarDatosClienteAsync(string dni, string nombre, string telefono, string? email, int? idCiudad = null, int? idExcluir = null);
}