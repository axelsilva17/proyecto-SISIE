using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;
    private readonly IValidadorProducto _validador;

    public ProductoService(ApplicationDbContext context, IValidadorProducto validador)
    {
        _context = context;
        _validador = validador;
    }


    public async Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null)
        => await _validador.ValidaProducto(dto, idProducto);

    public async Task<(IEnumerable<ProductoListDTO> Items, int Total)> ObtenerTodosAsyncProducto(int pagina, int tamanioPagina, int? idCategoria, bool? activo)
    {
        var query = _context.Productos.Include(p => p.Categoria).AsQueryable();
        if (idCategoria.HasValue) query = query.Where(p => p.IdCategoria == idCategoria.Value);
        if (activo.HasValue) query = query.Where(p => p.Activo == activo.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * tamanioPagina).Take(tamanioPagina)
            .Select(p => new ProductoListDTO
            {
                Id = p.Id, NombreProducto = p.NombreProducto, Descripcion = p.Descripcion,
                PrecioUnitario = p.PrecioUnitario, Stock = p.Stock, IdCategoria = p.IdCategoria,
                NombreCategoria = p.Categoria!.NombreCategoria, FechaCreacion = p.FechaCreacion, Activo = p.Activo
            }).ToListAsync();
        return (items, total);
    }


    public async Task<ProductoDTO?> ObtenerPorIdAsyncProducto(int id)
    {
        var producto = await _context.Productos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
        if (producto == null) return null;
        return MapToDTO(producto);
    }


    public async Task<ProductoDTO> CrearAsyncProducto(ProductoCreateDTO dto)
    {
        var erroresNegocio = await _validador.ValidaProducto(dto);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        var producto = new Producto { NombreProducto = dto.NombreProducto, Descripcion = dto.Descripcion,
            PrecioUnitario = dto.PrecioUnitario, Stock = dto.Stock, IdCategoria = dto.IdCategoria,
            FechaCreacion = DateTime.Now, Activo = true };
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return MapToDTO(producto);
    }


    public async Task<ProductoDTO?> ActualizarAsyncProducto(int id, ProductoUpdateDTO dto)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return null;
        var erroresNegocio = await _validador.ValidaProducto(new ProductoCreateDTO 
            { NombreProducto = dto.NombreProducto, Descripcion = dto.Descripcion, PrecioUnitario = dto.PrecioUnitario, Stock = dto.Stock, IdCategoria = dto.IdCategoria }, id);
        if (erroresNegocio.Any()) throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        producto.NombreProducto = dto.NombreProducto;
        producto.Descripcion = dto.Descripcion;
        producto.PrecioUnitario = dto.PrecioUnitario;
        producto.Stock = dto.Stock;
        producto.IdCategoria = dto.IdCategoria;
        await _context.SaveChangesAsync();
        return MapToDTO(producto);
    }


    public async Task<bool> EliminarAsyncProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return false;
        producto.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<ProductoDTO?> ToggleActivoAsyncProducto(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return null;
        producto.Activo = !producto.Activo;
        await _context.SaveChangesAsync();
        return MapToDTO(producto);
    }

    private ProductoDTO MapToDTO(Producto producto) => new ProductoDTO
    {
        Id = producto.Id, NombreProducto = producto.NombreProducto, Descripcion = producto.Descripcion,
        PrecioUnitario = producto.PrecioUnitario, Stock = producto.Stock, IdCategoria = producto.IdCategoria,
        NombreCategoria = producto.Categoria?.NombreCategoria, FechaCreacion = producto.FechaCreacion, Activo = producto.Activo
    };

    public async Task<StockVerificacionDTO> VerificarStockProductoAsync(int idProducto, int cantidad)
    {
        var errores = await _validador.ValidarStock(idProducto, cantidad);
        var producto = await _context.Productos.FindAsync(idProducto);
        var hayStock = errores.Count == 0;
        return new StockVerificacionDTO { IdProducto = idProducto, NombreProducto = producto?.NombreProducto,
            StockDisponible = producto?.Stock ?? 0, HayStock = hayStock, Mensaje = hayStock ? "Stock disponible" : errores.FirstOrDefault() };
    }

    public async Task<bool> ActualizarStockAsync(int idProducto, int cantidad)
    {
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto == null) return false;
        producto.Stock -= cantidad;
        await _context.SaveChangesAsync();
        return true;
    }
}