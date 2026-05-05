using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

// Validador para Producto
public interface IValidadorProducto
{
    Task<List<string>> ValidarAsync(ProductoCreateDTO dto);
    Task<List<string>> ValidarAsync(ProductoUpdateDTO dto);
    Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null);
}

public class ValidadorProducto : IValidadorProducto
{
    private readonly ApplicationDbContext _context;
    
    public ValidadorProducto(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Validación básica (campos obligatorios, formato)
    public Task<List<string>> ValidarAsync(ProductoCreateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreProducto))
            errores.Add("El nombre es obligatorio");
        if (dto.PrecioUnitario <= 0)
            errores.Add("El precio debe ser mayor a 0");
        if (dto.Stock < 0)
            errores.Add("El stock no puede ser negativo");
        if (string.IsNullOrWhiteSpace(dto.Descripcion))
            errores.Add("La descripción es obligatoria");
        return Task.FromResult(errores);
    }

    public Task<List<string>> ValidarAsync(ProductoUpdateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreProducto))
            errores.Add("El nombre es obligatorio");
        if (dto.PrecioUnitario <= 0)
            errores.Add("El precio debe ser mayor a 0");
        if (dto.Stock < 0)
            errores.Add("El stock no puede ser negativo");
        return Task.FromResult(errores);
    }
    
    // Validación de negocio (duplicados, existencia en BD)
    public async Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null)
    {
        var errores = new List<string>();
        
        // Verifica si el nombre ya existe (duplicado)
        var nombreLower = dto.NombreProducto.ToLower();
        var existeNombre = await _context.Productos
            .AnyAsync(p => p.NombreProducto.ToLower() == nombreLower 
                && p.Activo && (idProducto == null || p.Id != idProducto));
        
        if (existeNombre)
            errores.Add("Ya existe un producto con ese nombre");
        
        // Verifica que la categoría exista
        var categoriaExiste = await _context.Categorias
            .AnyAsync(c => c.Id == dto.IdCategoria);
        
        if (!categoriaExiste)
            errores.Add("La categoría no existe");
        
        return errores;
    }
}
