using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ProductoListDTO> Items, int Total)> GetAllAsync(int page, int pageSize, int? idCategoria, bool? activo)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Where(p => true);

        if (idCategoria.HasValue)
            query = query.Where(p => p.IdCategoria == idCategoria.Value);
        
        if (activo.HasValue)
            query = query.Where(p => p.Activo == activo.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductoListDTO
            {
                Id = p.Id,
                NombreProducto = p.NombreProducto,
                Descripcion = p.Descripcion,
                PrecioUnitario = p.PrecioUnitario,
                Stock = p.Stock,
                IdCategoria = p.IdCategoria,
                NombreCategoria = p.Categoria!.NombreCategoria,
                FechaCreacion = p.FechaCreacion,
                Activo = p.Activo
            })
            .ToListAsync();

        return (items, total);
    }

    public async Task<ProductoDTO?> GetByIdAsync(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null) return null;

        return new ProductoDTO
        {
            Id = producto.Id,
            NombreProducto = producto.NombreProducto,
            Descripcion = producto.Descripcion,
            PrecioUnitario = producto.PrecioUnitario,
            Stock = producto.Stock,
            IdCategoria = producto.IdCategoria,
            NombreCategoria = producto.Categoria?.NombreCategoria,
            FechaCreacion = producto.FechaCreacion,
            Activo = producto.Activo
        };
    }

    public async Task<ProductoDTO> CreateAsync(ProductoCreateDTO dto)
    {
        // Verificar si ya existe un producto con el mismo nombre (case-insensitive)
        var existe = await _context.Productos
            .AnyAsync(p => p.NombreProducto.Equals(dto.NombreProducto, StringComparison.OrdinalIgnoreCase) && p.Activo);
        
        if (existe)
        {
            throw new InvalidOperationException("Ya existe un producto con ese nombre");
        }

        // Verificar que la categoría exista
        var categoriaExiste = await _context.Categorias
            .AnyAsync(c => c.Id == dto.IdCategoria);
        
        if (!categoriaExiste)
        {
            throw new InvalidOperationException("La categoría no existe");
        }

        var producto = new Producto
        {
            NombreProducto = dto.NombreProducto,
            Descripcion = dto.Descripcion,
            PrecioUnitario = dto.PrecioUnitario,
            Stock = dto.Stock,
            IdCategoria = dto.IdCategoria,
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return new ProductoDTO
        {
            Id = producto.Id,
            NombreProducto = producto.NombreProducto,
            Descripcion = producto.Descripcion,
            PrecioUnitario = producto.PrecioUnitario,
            Stock = producto.Stock,
            IdCategoria = producto.IdCategoria,
            FechaCreacion = producto.FechaCreacion,
            Activo = producto.Activo
        };
    }

    public async Task<ProductoDTO?> UpdateAsync(int id, ProductoUpdateDTO dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return null;

        producto.NombreProducto = dto.NombreProducto;
        producto.Descripcion = dto.Descripcion;
        producto.PrecioUnitario = dto.PrecioUnitario;
        producto.Stock = dto.Stock;
        producto.IdCategoria = dto.IdCategoria;

        await _context.SaveChangesAsync();

        return new ProductoDTO
        {
            Id = producto.Id,
            NombreProducto = producto.NombreProducto,
            Descripcion = producto.Descripcion,
            PrecioUnitario = producto.PrecioUnitario,
            Stock = producto.Stock,
            IdCategoria = producto.IdCategoria,
            FechaCreacion = producto.FechaCreacion,
            Activo = producto.Activo
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return false;

        // Soft delete: marcar como inactivo en vez de borrar
        producto.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductoDTO?> ToggleActivoAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return null;

        producto.Activo = !producto.Activo;
        await _context.SaveChangesAsync();

        return new ProductoDTO
        {
            Id = producto.Id,
            NombreProducto = producto.NombreProducto,
            Descripcion = producto.Descripcion,
            PrecioUnitario = producto.PrecioUnitario,
            Stock = producto.Stock,
            IdCategoria = producto.IdCategoria,
            FechaCreacion = producto.FechaCreacion,
            Activo = producto.Activo
        };
    }
}