using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Repositorios;

public class ClienteRepositorio : IClienteRepositorio
{
    private readonly ApplicationDbContext _context;

    public ClienteRepositorio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Cliente> Items, int Total)> BuscarClientesAsync(
        int pagina, int tamanioPagina, string? nombre, bool? activo)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrEmpty(nombre))
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(nombre.ToLower()) ||
                c.Dni.Contains(nombre));

        if (activo.HasValue)
            query = query.Where(c => c.Activo == activo.Value);

        var total = await query.CountAsync();
        var items = await query
            .Include(c => c.Ciudad)
            .OrderBy(c => c.Nombre)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Cliente?> BuscarClientePorIdAsync(int id)
    {
        return await _context.Clientes
            .Include(c => c.Ciudad)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> BuscarPorDniAsync(string dni)
    {
        return await _context.Clientes
            .Include(c => c.Ciudad)
            .FirstOrDefaultAsync(c => c.Dni == dni);
    }

    public async Task<Cliente> InsertarClienteAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return cliente;
    }

    public async Task<bool> VerificarDniClienteExisteAsync(string dni, int? idExcluir)
    {
        var query = _context.Clientes.Where(c => c.Dni == dni);
        if (idExcluir.HasValue)
            query = query.Where(c => c.Id != idExcluir.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> VerificarEmailClienteExisteAsync(string email, int? idExcluir)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var emailLower = email.ToLower();
        var query = _context.Clientes.Where(c =>
            c.Email != null && c.Email.ToLower() == emailLower);
        if (idExcluir.HasValue)
            query = query.Where(c => c.Id != idExcluir.Value);
        return await query.AnyAsync();
    }

    public async Task<bool> VerificarCiudadExisteAsync(int idCiudad)
    {
        return await _context.Ciudades.AnyAsync(c => c.Id == idCiudad);
    }
}
