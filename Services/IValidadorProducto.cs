using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

public interface IValidadorProducto
{
    Task<List<string>> ValidarDatosProductoCreate(ProductoCreateDTO dto);
    Task<List<string>> ValidarDatosProductoUpdate(ProductoUpdateDTO dto);
    Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null);
    Task<List<string>> ValidaProductoUpdate(ProductoUpdateDTO dto, int idProducto);
    Task<List<string>> ValidarStock(int idProducto, int cantidad);
}

public class ValidadorProducto : IValidadorProducto
{
    private readonly ApplicationDbContext _context;
    
    public ValidadorProducto(ApplicationDbContext context) => _context = context;
    
    public Task<List<string>> ValidarDatosProductoCreate(ProductoCreateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreProducto)) errores.Add("El nombre es obligatorio");
        if (dto.PrecioUnitario <= 0) errores.Add("El precio debe ser mayor a 0");
        if (dto.Stock < 0) errores.Add("El stock no puede ser negativo");
        if (string.IsNullOrWhiteSpace(dto.Descripcion)) errores.Add("La descripción es obligatoria");
        return Task.FromResult(errores);
    }

    public Task<List<string>> ValidarDatosProductoUpdate(ProductoUpdateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.NombreProducto)) errores.Add("El nombre es obligatorio");
        if (dto.PrecioUnitario <= 0) errores.Add("El precio debe ser mayor a 0");
        if (dto.Stock < 0) errores.Add("El stock no puede ser negativo");
        return Task.FromResult(errores);
    }
    
    public async Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null)
    {
        var errores = new List<string>();
        
        var nombreLower = dto.NombreProducto.ToLower();
        var existeNombre = await _context.Productos
            .AnyAsync(p => p.NombreProducto.ToLower() == nombreLower 
                && p.Activo && (idProducto == null || p.Id != idProducto));
        
        if (existeNombre) errores.Add("Ya existe un producto con ese nombre");
        
        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.IdCategoria);
        if (!categoriaExiste) errores.Add("La categoría no existe");
        
        return errores;
    }

    public async Task<List<string>> ValidaProductoUpdate(ProductoUpdateDTO dto, int idProducto)
    {
        var errores = new List<string>();
        
        // Para update, usamos el ID del producto actual
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto == null)
        {
            errores.Add("El producto no existe");
            return errores;
        }

        // Verificar si el nombre ya existe en otro producto
        var nombreLower = dto.NombreProducto.ToLower();
        var existeNombre = await _context.Productos
            .AnyAsync(p => p.NombreProducto.ToLower() == nombreLower 
                && p.Activo && p.Id != idProducto);
        
        if (existeNombre) errores.Add("Ya existe un producto con ese nombre");
        
        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.IdCategoria);
        if (!categoriaExiste) errores.Add("La categoría no existe");
        
        return errores;
    }

    public async Task<List<string>> ValidarStock(int idProducto, int cantidad)
    {
        var errores = new List<string>();
        
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto == null)
            errores.Add($"El producto con ID {idProducto} no existe");
        else if (!producto.Activo)
            errores.Add($"El producto '{producto.NombreProducto}' está inactivo");
        else if (producto.Stock < cantidad)
            errores.Add($"Stock insuficiente para '{producto.NombreProducto}'. Disponible: {producto.Stock}");
        
        return errores;
    }
}