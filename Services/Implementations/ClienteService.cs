using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ClienteService : IClienteService
{
    private readonly IClienteRepositorio _clienteRepositorio;
    private readonly IValidadorCliente _validador;

    public ClienteService(IClienteRepositorio clienteRepositorio, IValidadorCliente validador)
    {
        _clienteRepositorio = clienteRepositorio;
        _validador = validador;
    }

    public async Task<(IEnumerable<ClienteDTO> Items, int Total)> ObtenerTodosAsync(
        int pagina, int tamanioPagina, string? nombre, bool? activo)
    {
        var (items, total) = await _clienteRepositorio.BuscarClientesAsync(
            pagina, tamanioPagina, nombre, activo);

        var dtos = items.Select(c => new ClienteDTO
        {
            Id = c.Id,
            Dni = c.Dni,
            Nombre = c.Nombre,
            Telefono = c.Telefono,
            Email = c.Email,
            DireccionDefault = c.DireccionDefault,
            NumeroDefault = c.NumeroDefault,
            DepartamentoDefault = c.DepartamentoDefault,
            IdCiudad = c.IdCiudad,
            NombreCiudad = c.Ciudad?.NombreCiudad,
            FechaCreacion = c.FechaCreacion,
            Activo = c.Activo,
            CantidadVentas = c.Ventas?.Count ?? 0
        }).ToList();

        return (dtos, total);
    }

    public async Task<ClienteDTO?> ObtenerPorIdAsync(int id)
    {
        var cliente = await _clienteRepositorio.BuscarClientePorIdAsync(id);
        if (cliente == null) return null;
        return MapToDTO(cliente);
    }

    public async Task<ClienteDTO?> BuscarPorDniAsync(string dni)
    {
        var cliente = await _clienteRepositorio.BuscarPorDniAsync(dni);
        if (cliente == null) return null;
        return MapToDTO(cliente);
    }

    public async Task<ClienteDTO> AgregarAsyncCliente(ClienteCreateDTO dto)
    {
        var errores = await _validador.ValidarDatosCliente(dto, null);
        if (errores.Any()) throw new InvalidOperationException(string.Join(", ", errores));

        var cliente = new Cliente
        {
            Dni = dto.Dni,
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Email = dto.Email?.ToLower(),
            DireccionDefault = dto.DireccionDefault,
            NumeroDefault = dto.NumeroDefault,
            DepartamentoDefault = dto.DepartamentoDefault,
            IdCiudad = dto.IdCiudad,
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        cliente = await _clienteRepositorio.InsertarClienteAsync(cliente);
        return MapToDTO(cliente);
    }

    private ClienteDTO MapToDTO(Cliente cliente) => new ClienteDTO
    {
        Id = cliente.Id,
        Dni = cliente.Dni,
        Nombre = cliente.Nombre,
        Telefono = cliente.Telefono,
        Email = cliente.Email,
        DireccionDefault = cliente.DireccionDefault,
        NumeroDefault = cliente.NumeroDefault,
        DepartamentoDefault = cliente.DepartamentoDefault,
        IdCiudad = cliente.IdCiudad,
        NombreCiudad = cliente.Ciudad?.NombreCiudad,
        FechaCreacion = cliente.FechaCreacion,
        Activo = cliente.Activo,
        CantidadVentas = cliente.Ventas?.Count ?? 0
    };

    public async Task AutoRegistrarClienteSiCorresponde(VentaCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DniCliente)) return;

        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreCliente))
            errores.Add("El nombre del cliente es obligatorio");
        if (string.IsNullOrWhiteSpace(dto.TelefonoCliente))
            errores.Add("El teléfono del cliente es obligatorio");
        if (!string.IsNullOrWhiteSpace(dto.EmailCliente) && !dto.EmailCliente.Contains("@"))
            errores.Add("El email del cliente debe contener @");

        if (errores.Count > 0)
            throw new InvalidOperationException(string.Join(", ", errores));

        var clienteExistente = await _clienteRepositorio.BuscarPorDniAsync(dto.DniCliente);
        if (clienteExistente != null) return;

        var nuevoCliente = new ClienteCreateDTO
        {
            Dni = dto.DniCliente,
            Nombre = dto.NombreCliente!,
            Telefono = dto.TelefonoCliente ?? string.Empty,
            Email = dto.EmailCliente?.ToLower()
        };
        await AgregarAsyncCliente(nuevoCliente);
    }
}
