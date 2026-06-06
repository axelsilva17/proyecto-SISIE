using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Repositorios;

public class CategoriaRepositorio : ICategoriaRepositorio
{
    private readonly ApplicationDbContext _context;

    public CategoriaRepositorio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categoria>> BuscarCategoriasAsync()
    {
        return await _context.Categorias
            .OrderBy(c => c.NombreCategoria)
            .ToListAsync();
    }

    public async Task<Categoria?> BuscarCategoriaPorIdAsync(int id)
    {
        return await _context.Categorias.FindAsync(id);
    }

    public async Task<Categoria> InsertarCategoriaAsync(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<Categoria> ModificarCategoriaAsync(Categoria categoria)
    {
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<bool> EliminarCategoriaFisicoAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return false;

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerificarProductosActivosAsync(int idCategoria)
    {
        return await _context.Productos.AnyAsync(p => p.IdCategoria == idCategoria && p.Activo);
    }

    public async Task<bool> VerificarNombreCategoriaExisteAsync(string nombre, int? idExcluir)
    {
        var nombreLower = nombre.ToLower();
        return await _context.Categorias
            .AnyAsync(c => c.NombreCategoria.ToLower() == nombreLower
                && (idExcluir == null || c.Id != idExcluir));
    }

    public async Task<bool> VerificarCategoriaExisteAsync(int id)
    {
        return await _context.Categorias.AnyAsync(c => c.Id == id);
    }
}
