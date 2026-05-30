using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Repositorios;

public class ProductoRepositorio : IProductoRepositorio
{
    private readonly ApplicationDbContext _context;

    public ProductoRepositorio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Producto> Items, int Total)> ObtenerTodosAsync(
        int pagina, int tamanioPagina, int? idCategoria, bool? activo)
    {
        var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

        if (idCategoria.HasValue)
            query = query.Where(p => p.IdCategoria == idCategoria.Value);

        if (activo.HasValue)
            query = query.Where(p => p.Activo == activo.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Producto?> ObtenerPorIdCrudoAsync(int id)
    {
        return await _context.Productos.FindAsync(id);
    }

    public async Task<Producto> CrearAsync(Producto producto)
    {
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();
        return producto;
    }

    public async Task<Producto> ActualizarAsync(Producto producto)
    {
        await _context.SaveChangesAsync();
        await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();
        return producto;
    }

    public async Task<bool> EliminarLogicoAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return false;

        producto.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Producto?> ToggleActivoAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return null;

        producto.Activo = !producto.Activo;
        await _context.SaveChangesAsync();
        await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();
        return producto;
    }

    public async Task<bool> ActualizarStockAsync(int idProducto, int cantidad)
    {
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto == null) return false;

        producto.Stock -= cantidad;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExisteNombreAsync(string nombre, int? idExcluir)
    {
        var nombreLower = nombre.ToLower();
        return await _context.Productos
            .AnyAsync(p => p.NombreProducto.ToLower() == nombreLower
                && p.Activo
                && (idExcluir == null || p.Id != idExcluir));
    }

    public async Task<bool> ExisteAsync(int id)
    {
        return await _context.Productos.AnyAsync(p => p.Id == id);
    }
}
