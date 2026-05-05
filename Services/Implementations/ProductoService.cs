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

    // Valida las reglas de negocio para crear/actualizar producto (método del Service)
    public async Task<List<string>> ValidaProducto(ProductoCreateDTO dto, int? idProducto = null)
    {
        // Usa el validador para reutilización
        return await _validador.ValidaProducto(dto, idProducto);
    }

    // Lista productos con paginación y filtros opcionales
    public async Task<(IEnumerable<ProductoListDTO> Items, int Total)> ObtenerTodosAsyncProducto(int pagina, int tamanioPagina, int? idCategoria, bool? activo)
    {
        // Inicia query incluyendo la categoría relacionada
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Where(p => true);

        // Filtra por categoría si se pasa
        if (idCategoria.HasValue)
            query = query.Where(p => p.IdCategoria == idCategoria.Value);
        
        // Filtra por estado (activo/inactivo)
        if (activo.HasValue)
            query = query.Where(p => p.Activo == activo.Value);

        // Cuenta el total antes de paginar
        var total = await query.CountAsync();

        // Aplica paginación y ordenamiento
        var items = await query
            .OrderByDescending(p => p.FechaCreacion) // Más nuevos primero
            .Skip((pagina - 1) * tamanioPagina)       // Salta páginas anteriores
            .Take(tamanioPagina)                        // Toma solo los de esta página
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

    // Obtiene un producto por su ID
    public async Task<ProductoDTO?> ObtenerPorIdAsyncProducto(int id)
    {
        // Busca el producto incluyendo su categoría
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        // Si no existe, retorna null
        if (producto == null) return null;

        // Convierte a DTO y retorna
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

    // Crea un nuevo producto en la base de datos
    public async Task<ProductoDTO> CrearAsyncProducto(ProductoCreateDTO dto)
    {
        // Valida reglas de negocio usando el validador
        var erroresNegocio = await _validador.ValidaProducto(dto);
        if (erroresNegocio.Any())
            throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        // Crea la entidad con los datos del DTO
        var producto = new Producto
        {
            NombreProducto = dto.NombreProducto,
            Descripcion = dto.Descripcion,
            PrecioUnitario = dto.PrecioUnitario,
            Stock = dto.Stock,
            IdCategoria = dto.IdCategoria,
            FechaCreacion = DateTime.Now,
            Activo = true // Los productos nuevos se crean activos
        };

        // Agrega a la DB y guarda
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        // Retorna el producto creado como DTO
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

    // Actualiza los datos de un producto existente
    public async Task<ProductoDTO?> ActualizarAsyncProducto(int id, ProductoUpdateDTO dto)
    {
        // Busca el producto por ID
        var producto = await _context.Productos.FindAsync(id);
        
        // Si no existe, retorna null
        if (producto == null) return null;

        // Valida reglas de negocio usando el validador
        var erroresNegocio = await _validador.ValidaProducto(new ProductoCreateDTO 
        { 
            NombreProducto = dto.NombreProducto,
            Descripcion = dto.Descripcion,
            PrecioUnitario = dto.PrecioUnitario,
            Stock = dto.Stock,
            IdCategoria = dto.IdCategoria
        }, id);
        
        if (erroresNegocio.Any())
            throw new InvalidOperationException(string.Join(", ", erroresNegocio));

        // Actualiza cada campo
        producto.NombreProducto = dto.NombreProducto;
        producto.Descripcion = dto.Descripcion;
        producto.PrecioUnitario = dto.PrecioUnitario;
        producto.Stock = dto.Stock;
        producto.IdCategoria = dto.IdCategoria;

        // Guarda los cambios en la DB
        await _context.SaveChangesAsync();

        // Retorna el producto actualizado
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

    // Elimina un producto (soft delete - solo lo marca como inactivo)
    public async Task<bool> EliminarAsyncProducto(int id)
    {
        // Busca el producto por ID
        var producto = await _context.Productos.FindAsync(id);
        
        // Si no existe, retorna false
        if (producto == null) return false;

        // Soft delete: marca como inactivo en vez de borrar
        producto.Activo = false;
        await _context.SaveChangesAsync();
        
        return true;
    }

    // Activa o desactiva un producto (toggle)
    public async Task<ProductoDTO?> ToggleActivoAsyncProducto(int id)
    {
        // Busca el producto por ID
        var producto = await _context.Productos.FindAsync(id);
        
        // Si no existe, retorna null
        if (producto == null) return null;

        // Invierte el estado actual (true → false, false → true)
        producto.Activo = !producto.Activo;
        await _context.SaveChangesAsync();

        // Retorna el producto con el nuevo estado
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