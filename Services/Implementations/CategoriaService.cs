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

    // Valida las reglas de negocio para crear/actualizar categoría (método del Service)
    public async Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null)
    {
        // Usa el validador para reutilización
        return await _validador.ValidaCategoria(dto, idCategoria);
    }

    // Lista todas las categorías ordenadas alfabéticamente
    public async Task<IEnumerable<CategoriaDTO>> ObtenerTodosAsyncCategoria()
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

    // Obtiene una categoría por su ID
    public async Task<CategoriaDTO?> ObtenerPorIdAsyncCategoria(int id)
    {
        // Busca la categoría por ID
        var categoria = await _context.Categorias.FindAsync(id);
        
        // Si no existe, retorna null
        if (categoria == null) return null;

        // Convierte a DTO y retorna
        return new CategoriaDTO
        {
            Id = categoria.Id,
            NombreCategoria = categoria.NombreCategoria
        };
    }

    // Crea una nueva categoría
    public async Task<CategoriaDTO> CrearAsyncCategoria(CategoriaCreateDTO dto)
    {
        // Valida reglas de negocio usando el validador
        var erroresNegocio = await _validador.ValidaCategoria(dto);
        if (erroresNegocio.Any())
            throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        // Crea la entidad
        var categoria = new Categoria
        {
            NombreCategoria = dto.NombreCategoria
        };

        // Agrega a la DB y guarda
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        // Retorna la categoría creada como DTO
        return new CategoriaDTO
        {
            Id = categoria.Id,
            NombreCategoria = categoria.NombreCategoria
        };
    }

    // Actualiza una categoría existente
    public async Task<CategoriaDTO?> ActualizarAsyncCategoria(int id, CategoriaCreateDTO dto)
    {
        // Busca la categoría por ID
        var categoria = await _context.Categorias.FindAsync(id);
        
        // Si no existe, retorna null
        if (categoria == null) return null;

        // Valida reglas de negocio usando el validador
        var erroresNegocio = await _validador.ValidaCategoria(dto, id);
        if (erroresNegocio.Any())
            throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        // Actualiza el nombre
        categoria.NombreCategoria = dto.NombreCategoria;
        
        // Guarda los cambios en la DB
        await _context.SaveChangesAsync();

        // Retorna la categoría actualizada
        return new CategoriaDTO
        {
            Id = categoria.Id,
            NombreCategoria = categoria.NombreCategoria
        };
    }

    // Elimina una categoría de la base de datos
    public async Task<bool> EliminarAsyncCategoria(int id)
    {
        // Busca la categoría por ID
        var categoria = await _context.Categorias.FindAsync(id);
        
        // Si no existe, retorna false
        if (categoria == null) return false;

        // Elimina físicamente de la DB
        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        
        return true;
    }

    // Verifica si una categoría se puede eliminar (sin productos activos)
    public async Task<bool> PuedeEliminarAsync(int id)
    {
        // Busca si hay productos activos en esta categoría
        var tieneProductos = await _context.Productos
            .AnyAsync(p => p.IdCategoria == id && p.Activo);
        
        // Retorna true si NO tiene productos (puede eliminar)
        return !tieneProductos;
    }
}