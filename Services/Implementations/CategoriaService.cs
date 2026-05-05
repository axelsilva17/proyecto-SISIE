using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _context;
    private readonly IValidadorCategoria _validador;

    public CategoriaService(ApplicationDbContext context, IValidadorCategoria validador)
    {
        _context = context;
        _validador = validador;
    }

    public async Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null)
        => await _validador.ValidaCategoria(dto, idCategoria);

    public async Task<IEnumerable<CategoriaDTO>> ObtenerTodosAsyncCategoria()
        => await _context.Categorias.OrderBy(c => c.NombreCategoria)
            .Select(c => new CategoriaDTO { Id = c.Id, NombreCategoria = c.NombreCategoria }).ToListAsync();

    public async Task<CategoriaDTO?> ObtenerPorIdAsyncCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return null;
        return new CategoriaDTO { Id = categoria.Id, NombreCategoria = categoria.NombreCategoria };
    }

    public async Task<CategoriaDTO> CrearAsyncCategoria(CategoriaCreateDTO dto)
    {
        var erroresNegocio = await _validador.ValidaCategoria(dto);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        var categoria = new Categoria { NombreCategoria = dto.NombreCategoria };
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return new CategoriaDTO { Id = categoria.Id, NombreCategoria = categoria.NombreCategoria };
    }

    public async Task<CategoriaDTO?> ActualizarAsyncCategoria(int id, CategoriaCreateDTO dto)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return null;
        var erroresNegocio = await _validador.ValidaCategoria(dto, id);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        categoria.NombreCategoria = dto.NombreCategoria;
        await _context.SaveChangesAsync();
        return new CategoriaDTO { Id = categoria.Id, NombreCategoria = categoria.NombreCategoria };
    }

    public async Task<bool> EliminarAsyncCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return false;
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PuedeEliminarAsync(int id)
    {
        var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id && p.Activo);
        return !tieneProductos;
    }
}