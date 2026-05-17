using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;
    private readonly IValidadorCliente _validador;

    public ClienteService(ApplicationDbContext context, IValidadorCliente validador)
    {
        _context = context;
        _validador = validador;
    }

    public async Task<(IEnumerable<ClienteDTO> Items, int Total)> ObtenerTodosAsync(int pagina, int tamanioPagina, string? nombre, bool? activo)
    {
        var query = _context.Clientes.AsQueryable();
        if (!string.IsNullOrEmpty(nombre)) query = query.Where(c => c.Nombre.ToLower().Contains(nombre.ToLower()) || c.Dni.Contains(nombre));
        if (activo.HasValue) query = query.Where(c => c.Activo == activo.Value);

        var total = await query.CountAsync();
        var items = await query.OrderBy(c => c.Nombre).Skip((pagina - 1) * tamanioPagina).Take(tamanioPagina).ToListAsync();
        var dtos = items.Select(c => new ClienteDTO { Id = c.Id, Dni = c.Dni, Nombre = c.Nombre, Telefono = c.Telefono, Email = c.Email,
            DireccionDefault = c.DireccionDefault, NumeroDefault = c.NumeroDefault, DepartamentoDefault = c.DepartamentoDefault,
            IdCiudad = c.IdCiudad, NombreCiudad = null, FechaCreacion = c.FechaCreacion, Activo = c.Activo, CantidadVentas = c.Ventas?.Count ?? 0 }).ToList();
        return (dtos, total);
    }

    public async Task<ClienteDTO?> ObtenerPorIdAsync(int id)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente == null) return null;
        return MapToDTO(cliente);
    }

    public async Task<ClienteDTO?> BuscarPorDniAsync(string dni)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Dni == dni);
        if (cliente == null) return null;
        return MapToDTO(cliente);
    }

    public async Task<ClienteDTO> AgregarAsyncCliente(ClienteCreateDTO dto)
    {
        // null en idCliente = validación completa (unicidad incluida) para una creación
        var errores = await _validador.ValidarDatosCliente(dto, null);
        if (errores.Any()) throw new InvalidOperationException(string.Join(", ", errores));

        var cliente = new Cliente { Dni = dto.Dni, Nombre = dto.Nombre, Telefono = dto.Telefono, Email = dto.Email?.ToLower(),
            DireccionDefault = dto.DireccionDefault, NumeroDefault = dto.NumeroDefault, DepartamentoDefault = dto.DepartamentoDefault,
            IdCiudad = dto.IdCiudad, FechaCreacion = DateTime.Now, Activo = true };
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return MapToDTO(cliente);
    }

    private ClienteDTO MapToDTO(Cliente cliente) => new ClienteDTO
    {
        Id = cliente.Id, Dni = cliente.Dni, Nombre = cliente.Nombre, Telefono = cliente.Telefono, Email = cliente.Email,
        DireccionDefault = cliente.DireccionDefault, NumeroDefault = cliente.NumeroDefault, DepartamentoDefault = cliente.DepartamentoDefault,
        IdCiudad = cliente.IdCiudad, NombreCiudad = cliente.Ciudad?.NombreCiudad, FechaCreacion = cliente.FechaCreacion,
        Activo = cliente.Activo, CantidadVentas = cliente.Ventas?.Count ?? 0
    };
}
