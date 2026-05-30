using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Repositorios;

public class VentaRepositorio : IVentaRepositorio
{
    private readonly ApplicationDbContext _context;

    public VentaRepositorio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Venta?> ObtenerPorIdConTodoAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Direccion)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venta?> ObtenerPorIdConDetallesAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venta?> ObtenerPorIdCrudoAsync(int id)
    {
        return await _context.Ventas.FindAsync(id);
    }

    public async Task<(List<Venta> Items, int Total)> ObtenerHistorialAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado,
        DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var query = _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
            .AsQueryable();

        if (idUsuario.HasValue)
            query = query.Where(v => v.IdUsuario == idUsuario.Value);

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(v => v.Estado.ToLower() == estado.ToLower());

        if (fechaDesde.HasValue)
            query = query.Where(v => v.FechaCreacion >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(v => v.FechaCreacion <= fechaHasta.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.FechaCreacion)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Venta> CrearAsync(Venta venta)
    {
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return venta;
    }

    public async Task<Venta> ActualizarAsync(Venta venta)
    {
        await _context.SaveChangesAsync();
        return venta;
    }

    public async Task<DetalleVenta> AgregarDetalleAsync(DetalleVenta detalle)
    {
        _context.DetallesVenta.Add(detalle);
        await _context.SaveChangesAsync();
        return detalle;
    }

    public async Task<Direccion> CrearDireccionAsync(Direccion direccion)
    {
        _context.Direcciones.Add(direccion);
        await _context.SaveChangesAsync();
        return direccion;
    }

    public async Task<bool> ExisteUsuarioAsync(int idUsuario)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == idUsuario);
    }

    public async Task<bool> ExisteDireccionAsync(int idDireccion)
    {
        return await _context.Direcciones.AnyAsync(d => d.Id == idDireccion);
    }

    public async Task<VentasEstadisticas> ObtenerEstadisticasAsync(
        DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var query = _context.Ventas.AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(v => v.FechaCreacion >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(v => v.FechaCreacion <= fechaHasta.Value);

        var totalVentas = await query.CountAsync();
        var totalFacturado = await query.SumAsync(v => v.Total);
        var ventasCanceladas = await query.CountAsync(v => v.Estado == "Cancelada");
        var ventasPendientes = await query.CountAsync(v => v.Estado == "Pendiente");
        var ventasEntregadas = await query.CountAsync(v => v.Estado == "Entregada");

        return new VentasEstadisticas
        {
            TotalVentas = totalVentas,
            TotalFacturado = totalFacturado,
            VentasCanceladas = ventasCanceladas,
            VentasPendientes = ventasPendientes,
            VentasEntregadas = ventasEntregadas,
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };
    }
}
