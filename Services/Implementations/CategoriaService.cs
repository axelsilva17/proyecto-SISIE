using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _context;

    public CategoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoriaDTO>> GetAllAsync()
    {
        return await _context.Categorias
            .OrderBy(c => c.NombreCategoria)
            .Select(c => new CategoriaDTO
            {
                Id = c.Id,
                NombreCategoria = c.NombreCategoria
            })
            .ToListAsync();
    }

    public async Task<CategoriaDTO?> GetByIdAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return null;

        return new CategoriaDTO
        {
            Id = categoria.Id,
            NombreCategoria = categoria.NombreCategoria
        };
    }

    public async Task<CategoriaDTO> CreateAsync(CategoriaCreateDTO dto)
    {
        // Verificar si ya existe una categoría con el mismo nombre
        var existe = await _context.Categorias
            .AnyAsync(c => c.NombreCategoria.ToLower() == dto.NombreCategoria.ToLower());
        
        if (existe)
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre");
        }

        var categoria = new Categoria
        {
            NombreCategoria = dto.NombreCategoria
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaDTO
        {
            Id = categoria.Id,
            NombreCategoria = categoria.NombreCategoria
        };
    }

    public async Task<CategoriaDTO?> UpdateAsync(int id, CategoriaCreateDTO dto)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return null;

        categoria.NombreCategoria = dto.NombreCategoria;
        await _context.SaveChangesAsync();

        return new CategoriaDTO
        {
            Id = categoria.Id,
            NombreCategoria = categoria.NombreCategoria
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return false;

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanDeleteAsync(int id)
    {
        var tieneProductos = await _context.Productos
            .AnyAsync(p => p.IdCategoria == id && p.Activo);
        return !tieneProductos;
    }
}