using Microsoft.Data.SqlClient;
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

    public async Task<Venta?> BuscarVentaConTodoAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Direccion)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venta?> BuscarVentaConDetallesAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venta?> BuscarVentaCrudaAsync(int id)
    {
        return await _context.Ventas.FindAsync(id);
    }

   

    public async Task<Venta> ModificarVentaAsync(Venta venta)
    {
        await _context.SaveChangesAsync();
        return venta;
    }
public async Task<Direccion> InsertarDireccionAsync(Direccion direccion)
    {
        _context.Direcciones.Add(direccion);
        await _context.SaveChangesAsync();
        return direccion;
    }

    public async Task<bool> VerificarUsuarioExisteAsync(int idUsuario)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == idUsuario);
    }

    public async Task<bool> VerificarDireccionExisteAsync(int idDireccion)
    {
        return await _context.Direcciones.AnyAsync(d => d.Id == idDireccion);
    }
   

    // ============================================
    // MÉTODOS CON STORED PROCEDURES
    // ============================================

    //SP de Actualizacion inserta la venta
     public async Task<Venta> InsertarVentaAsync(Venta venta)
    {
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "sp_RegistrarVenta";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddRange(new[]
        {
            new SqlParameter("@NumeroVenta", venta.NumeroVenta),
            new SqlParameter("@Descuento", venta.Descuento),
            new SqlParameter("@MetodoPago", venta.MetodoPago),
            new SqlParameter("@TipoEntrega", venta.TipoEntrega),
            new SqlParameter("@Estado", venta.Estado ?? "Pendiente"),
            new SqlParameter("@Notas", (object?)venta.Notas ?? DBNull.Value),
            new SqlParameter("@IdDireccion", (object?)venta.IdDireccion ?? DBNull.Value),
            new SqlParameter("@IdUsuario", venta.IdUsuario),
            new SqlParameter("@Total", venta.Total)
        });

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();
        venta.Id = Convert.ToInt32(result);

        // Attach para que EF trackee futuros cambios 
        _context.Ventas.Attach(venta);

        return venta;
    }

    //SP de Actualizacion inserta el detalle venta
    public async Task<DetalleVenta> InsertarDetalleVentaAsync(DetalleVenta detalle)
    {
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "sp_RegistrarDetalleVenta";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddRange(new[]
        {
            new SqlParameter("@IdVenta", detalle.IdVenta),
            new SqlParameter("@IdProducto", detalle.IdProducto),
            new SqlParameter("@Cantidad", detalle.Cantidad),
            new SqlParameter("@PrecioUnitario", detalle.PrecioUnitario)
        });

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        var result = await command.ExecuteScalarAsync();
        if (result != null)
            detalle.Id = Convert.ToInt32(result);

        // Attach para que EF trackee la entidad
        _context.DetallesVenta.Attach(detalle);

        return detalle;
    }

    

    public async Task CancelarVentaConSPAsync(int idVenta)
    {
        var parameter = new SqlParameter("@IdVenta", idVenta);
        await _context.Database
            .ExecuteSqlRawAsync("EXEC sp_CancelarVenta @IdVenta", parameter);
    }

    public async Task<(List<VentaHistorialDTO> Items, int Total)> ConsultarHistorialPaginadoAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado,
        DateTime? fechaDesde, DateTime? fechaHasta)
    {
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "sp_ObtenerHistorialVentas";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddRange(new[]
        {
            new SqlParameter("@Pagina", pagina),
            new SqlParameter("@TamanoPagina", tamanioPagina),
            new SqlParameter("@IdUsuario", (object?)idUsuario ?? DBNull.Value),
            new SqlParameter("@Estado", (object?)estado ?? DBNull.Value),
            new SqlParameter("@FechaDesde", (object?)fechaDesde ?? DBNull.Value),
            new SqlParameter("@FechaHasta", (object?)fechaHasta ?? DBNull.Value)
        });

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        // Primer result set: Total
        int total = 0;
        if (await reader.ReadAsync())
            total = reader.GetInt32(0);

        // Segundo result set: Items
        var resultados = new List<VentaHistorialDTO>();
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            resultados.Add(new VentaHistorialDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                NumeroVenta = reader.GetInt32(reader.GetOrdinal("NumeroVenta")),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                MetodoPago = reader.GetString(reader.GetOrdinal("MetodoPago")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                CantidadItems = reader.GetInt32(reader.GetOrdinal("CantidadItems"))
            });
        }

        return (resultados, total);
    }


   //SP de Consulta,estadisticas Ventas
     public async Task<VentasEstadisticas> ConsultarEstadisticasVentasAsync(
        DateTime? fechaDesde, DateTime? fechaHasta)
    {
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "sp_ObtenerEstadisticasVentas";
        command.CommandType = System.Data.CommandType.StoredProcedure;
        command.Parameters.AddRange(new[]
        {
            new SqlParameter("@FechaDesde", (object?)fechaDesde ?? DBNull.Value),
            new SqlParameter("@FechaHasta", (object?)fechaHasta ?? DBNull.Value)
        });

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new VentasEstadisticas
        {
            TotalVentas = reader.GetInt32(reader.GetOrdinal("TotalVentas")),
            TotalFacturado = reader.GetDecimal(reader.GetOrdinal("TotalFacturado")),
            VentasCanceladas = reader.GetInt32(reader.GetOrdinal("VentasCanceladas")),
            VentasPendientes = reader.GetInt32(reader.GetOrdinal("VentasPendientes")),
            VentasEntregadas = reader.GetInt32(reader.GetOrdinal("VentasEntregadas")),
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta
        };
    }
}
