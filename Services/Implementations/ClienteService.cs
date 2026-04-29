using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

// Servicio para gestionar clientes
public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;

    public ClienteService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lista clientes con paginación y filtros opcionales
    public async Task<(IEnumerable<ClienteDTO> Items, int Total)> ObtenerTodosAsync(
        int pagina, int tamanioPagina, string? nombre, bool? activo)
    {
        var query = _context.Clientes
            .AsQueryable();

        if (!string.IsNullOrEmpty(nombre))
            query = query.Where(c => 
                c.Nombre.ToLower().Contains(nombre.ToLower()) || 
                c.Dni.Contains(nombre));

        if (activo.HasValue)
            query = query.Where(c => c.Activo == activo.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Nombre)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

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
            NombreCiudad = null,
            FechaCreacion = c.FechaCreacion,
            Activo = c.Activo,
            CantidadVentas = c.Ventas?.Count ?? 0
        }).ToList();

        return (dtos, total);
    }

    // Obtiene un cliente por su ID
    public async Task<ClienteDTO?> ObtenerPorIdAsync(int id)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null) return null;

        return MapToDTO(cliente);
    }

    // Busca un cliente por su DNI
    public async Task<ClienteDTO?> BuscarPorDniAsync(string dni)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.Dni == dni);

        if (cliente == null) return null;

        return MapToDTO(cliente);
    }

    // Agrega un nuevo cliente
    public async Task<ClienteDTO> AgregarAsyncCliente(ClienteCreateDTO dto)
    {
        // Valida los datos del cliente
        await ValidarDatosClienteAsync(dto.Dni, dto.Nombre, dto.Telefono, dto.Email, dto.IdCiudad);

        // Crea la entidad
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

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return MapToDTO(cliente);
    }

    // Valida los datos de un cliente (público para usar desde otros servicios)
    public async Task ValidarDatosClienteAsync(string dni, string nombre, string telefono, string? email, int? idCiudad = null, int? idExcluir = null)
    {
        if (string.IsNullOrWhiteSpace(dni))
            throw new InvalidOperationException("El DNI es requerido");

        if (dni.Length < 7 || dni.Length > 15)
            throw new InvalidOperationException("El DNI debe tener entre 7 y 15 caracteres");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException("El nombre es requerido");

        if (nombre.Length < 3 || nombre.Length > 100)
            throw new InvalidOperationException("El nombre debe tener entre 3 y 100 caracteres");

        if (string.IsNullOrWhiteSpace(telefono))
            throw new InvalidOperationException("El teléfono es requerido");

        if (telefono.Length < 8 || telefono.Length > 20)
            throw new InvalidOperationException("El teléfono debe tener entre 8 y 20 caracteres");

        // Valida DNI único
        var query = _context.Clientes.Where(c => c.Dni == dni);
        if (idExcluir.HasValue)
            query = query.Where(c => c.Id != idExcluir.Value);
        
        var dniExiste = await query.AnyAsync();
        if (dniExiste)
            throw new InvalidOperationException("Ya existe un cliente con ese DNI");

        // Valida email único si se proporciona
        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailQuery = _context.Clientes.Where(c => c.Email != null && c.Email.ToLower() == email.ToLower());
            if (idExcluir.HasValue)
                emailQuery = emailQuery.Where(c => c.Id != idExcluir.Value);
            
            var emailExiste = await emailQuery.AnyAsync();
            if (emailExiste)
                throw new InvalidOperationException("Ya existe un cliente con ese email");
        }

        // Valida ciudad si se proporciona
        if (idCiudad.HasValue)
        {
            var ciudadExiste = await _context.Ciudades.AnyAsync(c => c.Id == idCiudad.Value);
            if (!ciudadExiste)
                throw new InvalidOperationException("La ciudad no existe");
        }
    }

    // Mapea entidad a DTO
    private ClienteDTO MapToDTO(Cliente cliente)
    {
        return new ClienteDTO
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
    }
}