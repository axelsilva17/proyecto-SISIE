using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services.Interfaces;

public interface IClienteRepositorio
{
    /// <summary>Obtiene clientes paginados con filtro opcional por nombre y activo.</summary>
    Task<(List<Cliente> Items, int Total)> ObtenerTodosAsync(
        int pagina, int tamanioPagina, string? nombre, bool? activo);

    /// <summary>Obtiene un cliente por ID.</summary>
    Task<Cliente?> ObtenerPorIdAsync(int id);

    /// <summary>Busca un cliente por DNI.</summary>
    Task<Cliente?> BuscarPorDniAsync(string dni);

    /// <summary>Crea un cliente y guarda en BD.</summary>
    Task<Cliente> CrearAsync(Cliente cliente);

    /// <summary>Verifica si ya existe un cliente con ese DNI.</summary>
    Task<bool> ExisteDniAsync(string dni, int? idExcluir);

    /// <summary>Verifica si ya existe un cliente con ese email.</summary>
    Task<bool> ExisteEmailAsync(string email, int? idExcluir);

    /// <summary>Verifica si existe una ciudad por ID.</summary>
    Task<bool> ExisteCiudadAsync(int idCiudad);
}
