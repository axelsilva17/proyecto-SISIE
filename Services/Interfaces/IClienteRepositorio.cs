using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface IClienteRepositorio
{
    /// <summary>Busca clientes paginados con filtro opcional por nombre y activo.</summary>
    Task<(List<Cliente> Items, int Total)> BuscarClientesAsync(
        int pagina, int tamanioPagina, string? nombre, bool? activo);

    /// <summary>Busca un cliente por ID.</summary>
    Task<Cliente?> BuscarClientePorIdAsync(int id);

    /// <summary>Busca un cliente por DNI.</summary>
    Task<Cliente?> BuscarPorDniAsync(string dni);

    /// <summary>Inserta un cliente en BD.</summary>
    Task<Cliente> InsertarClienteAsync(Cliente cliente);

    /// <summary>Verifica si ya existe un cliente con ese DNI.</summary>
    Task<bool> VerificarDniClienteExisteAsync(string dni, int? idExcluir);

    /// <summary>Verifica si ya existe un cliente con ese email.</summary>
    Task<bool> VerificarEmailClienteExisteAsync(string email, int? idExcluir);

    /// <summary>Verifica si existe una ciudad por ID.</summary>
    Task<bool> VerificarCiudadExisteAsync(int idCiudad);
}
