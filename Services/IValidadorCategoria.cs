using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

public interface IValidadorCategoria
{
    Task<List<string>> ValidarDatosCategoria(CategoriaCreateDTO dto);
    Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null);
}

public class ValidadorCategoria : IValidadorCategoria
{
    private readonly ApplicationDbContext _context;
    
    public ValidadorCategoria(ApplicationDbContext context) => _context = context;
    
    public Task<List<string>> ValidarDatosCategoria(CategoriaCreateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreCategoria)) errores.Add("El nombre es obligatorio");
        if (dto.NombreCategoria?.Length > 50) errores.Add("El nombre no puede superar los 50 caracteres");
        return Task.FromResult(errores);
    }
    
    public async Task<List<string>> ValidaCategoria(CategoriaCreateDTO dto, int? idCategoria = null)
    {
        var errores = new List<string>();
        var nombreLower = dto.NombreCategoria.ToLower();
        var existeNombre = await _context.Categorias
            .AnyAsync(c => c.NombreCategoria.ToLower() == nombreLower 
                && (idCategoria == null || c.Id != idCategoria));
        
        if (existeNombre) errores.Add("Ya existe una categoría con ese nombre");
        
        return errores;
    }
}