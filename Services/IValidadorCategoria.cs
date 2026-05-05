using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

// Validador para Categoría
public interface IValidadorCategoria
{
    Task<List<string>> ValidarAsync(CategoriaCreateDTO dto);
    Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null);
}

public class ValidadorCategoria : IValidadorCategoria
{
    private readonly ApplicationDbContext _context;
    
    public ValidadorCategoria(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Validación básica (campos obligatorios, formato)
    public Task<List<string>> ValidarAsync(CategoriaCreateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreCategoria))
            errores.Add("El nombre es obligatorio");
        if (dto.NombreCategoria?.Length > 50)
            errores.Add("El nombre no puede superar los 50 caracteres");
        return Task.FromResult(errores);
    }
    
    // Validación de negocio (duplicados, existencia en BD)
    public async Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null)
    {
        var errores = new List<string>();
        
        // Verifica si el nombre ya existe (duplicado)
        var nombreLower = dto.NombreCategoria.ToLower();
        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.NombreCategoria.ToLower() == nombreLower 
                && (idCategoria == null || c.Id != idCategoria));
        
        if (existeNombre)
            errores.Add("Ya existe una categoría con ese nombre");
        
        return errores;
    }
}
