using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

// Servicio para gestionar ventas, detalles y stock
public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;

    public VentaService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Registra una nueva venta con sus detalles, descuenta stock y calcula el total
    public async Task<VentaDTO> RegistrarVentaAsync(int idUsuario, VentaCreateDTO dto)
    {
// Valida que el usuario exista
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null)
            throw new InvalidOperationException("El usuario no existe");

        // Variable para guardar el ID de dirección
        int? idDireccionFinal = dto.IdDireccion;

        // Si es envío a domicilio sin dirección previa, crea una nueva
        if (dto.EsEnvio && !dto.IdDireccion.HasValue)
        {
            // Valida que tenga ciudad
            if (!dto.IdCiudad.HasValue || dto.IdCiudad.Value <= 0)
                throw new InvalidOperationException("Debe seleccionar una ciudad para envío a domicilio");

            // Valida que tenga dirección
            if (string.IsNullOrWhiteSpace(dto.DireccionEnvio))
                throw new InvalidOperationException("Debe indicar la dirección para envío a domicilio");

            // Crea la nueva dirección
            var nuevaDireccion = new Direccion
            {
                Calle = dto.DireccionEnvio,
                Numero = 1,
                Departamento = dto.Departamento,
                IdCiudad = dto.IdCiudad.Value,
                IdUsuario = idUsuario
            };

            _context.Direcciones.Add(nuevaDireccion);
            await _context.SaveChangesAsync();
            idDireccionFinal = nuevaDireccion.Id;
        }

        // Valida que si es envío con ID de dirección, la dirección sea válida
        if (dto.EsEnvio && dto.IdDireccion.HasValue)
        {
            var direccionValida = await _context.Direcciones
                .AnyAsync(d => d.Id == dto.IdDireccion.Value);
            if (!direccionValida)
                throw new InvalidOperationException("La dirección no es válida");
        }

        // Verifica stock disponible de todos los productos antes de procesar
        foreach (var detalle in dto.Detalles)
        {
            var producto = await _context.Productos.FindAsync(detalle.IdProducto);
            if (producto == null)
                throw new InvalidOperationException($"El producto con ID {detalle.IdProducto} no existe");
            if (!producto.Activo)
                throw new InvalidOperationException($"El producto '{producto.NombreProducto}' está inactivo");
            if (producto.Stock < detalle.Cantidad)
                throw new InvalidOperationException($"Stock insuficiente para '{producto.NombreProducto}'. Disponible: {producto.Stock}");
        }

        // Genera el número de venta (timestamp + random para evitar duplicados)
        var numeroVenta = (int)(DateTime.Now.Ticks % 100000000) + new Random().Next(1000, 9999);

        // Crea la entidad Venta
        var venta = new Venta
        {
            NumeroVenta = numeroVenta,
            Descuento = dto.Descuento,
            MetodoPago = dto.MetodoPago,
            TipoEntrega = dto.EsEnvio ? "Envío" : "Mostrador",
            Notas = dto.Notas,
            Estado = "Pendiente",
            Total = 0, // Se calculará con los triggers o manualmente
            FechaCreacion = DateTime.Now,
            IdDireccion = dto.EsEnvio ? idDireccionFinal : null,
            IdUsuario = idUsuario
        };

        // Agrega la venta al contexto
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync(); // Guardar para obtener el ID

        // Crea los detalles de venta
        decimal totalVenta = 0;
        foreach (var detalleDto in dto.Detalles)
        {
            var producto = await _context.Productos.FindAsync(detalleDto.IdProducto);
            
            // Calcula el subtotal (cantidad * precio unitario)
            var subtotal = detalleDto.Cantidad * producto!.PrecioUnitario;
            
            var detalle = new DetalleVenta
            {
                IdVenta = venta.Id,
                IdProducto = detalleDto.IdProducto,
                Cantidad = detalleDto.Cantidad,
                PrecioUnitario = producto.PrecioUnitario,
                SubTotal = subtotal
            };

            _context.DetallesVenta.Add(detalle);
            
            // Descuenta el stock del producto
            producto.Stock -= detalleDto.Cantidad;
            
            totalVenta += subtotal;
        }

        // Aplica el descuento porcentual
        var descuentoDecimal = (decimal)dto.Descuento / 100;
        venta.Total = Math.Round(totalVenta * (1 - descuentoDecimal), 2);

        await _context.SaveChangesAsync();

        // Retorna la venta creada con sus detalles
        return await ObtenerVentaDTOCompleto(venta.Id);
    }

    // Obtiene una venta por su ID con todos los detalles
    public async Task<VentaDTO?> ObtenerVentaPorIdAsync(int id)
    {
        return await ObtenerVentaDTOCompleto(id);
    }

    // Obtiene el historial de ventas con paginación y filtros opcionales
    public async Task<(IEnumerable<VentaHistorialDTO> Items, int Total)> ObtenerHistorialVentasAsync(
        int pagina, int tamanioPagina, int? idUsuario, string? estado, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        // Inicia la query base
        var query = _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
            .AsQueryable();

        // Aplica filtros si se proporcionan
        if (idUsuario.HasValue)
            query = query.Where(v => v.IdUsuario == idUsuario.Value);

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(v => v.Estado.ToLower() == estado.ToLower());

        if (fechaDesde.HasValue)
            query = query.Where(v => v.FechaCreacion >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(v => v.FechaCreacion <= fechaHasta.Value);

        // Cuenta el total antes de paginar
        var total = await query.CountAsync();

        // Obtiene los resultados paginados y ordenados por fecha descendente
        var items = await query
            .OrderByDescending(v => v.FechaCreacion)
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .Select(v => new VentaHistorialDTO
            {
                Id = v.Id,
                NumeroVenta = v.NumeroVenta,
                Estado = v.Estado,
                Total = v.Total,
                MetodoPago = v.MetodoPago,
                FechaCreacion = v.FechaCreacion,
                CantidadItems = v.Detalles.Count
            })
            .ToListAsync();

        return (items, total);
    }

    // Actualiza el estado de una venta (Pendiente -> Entregada, Cancelada, etc.)
    public async Task<VentaDTO?> ActualizarEstadoVentaAsync(int id, VentaUpdateDTO dto)
    {
        // Busca la venta por ID
        var venta = await _context.Ventas.FindAsync(id);
        
        // Si no existe, retorna null
        if (venta == null) return null;

        // Verifica que la venta no esté ya cerrada o cancelada
        if (venta.Estado == "Cancelada" || venta.Estado == "Entregada")
            throw new InvalidOperationException($"No se puede modificar una venta en estado '{venta.Estado}'");

        // Actualiza los campos permitidos
        venta.Estado = dto.Estado;
        if (dto.Notas != null)
            venta.Notas = dto.Notas;

        // Guarda los cambios en la base de datos
        await _context.SaveChangesAsync();

        // Retorna la venta actualizada
        return await ObtenerVentaDTOCompleto(id);
    }

    // Cancela una venta y reposiciona el stock de los productos
    public async Task<VentaDTO?> CancelarVentaAsync(int id)
    {
        // Busca la venta con sus detalles
        var venta = await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id);

        // Si no existe, retorna null
        if (venta == null) return null;

        // Verifica que la venta se pueda cancelar
        if (venta.Estado == "Cancelada")
            throw new InvalidOperationException("La venta ya está cancelada");

        if (venta.Estado == "Entregada")
            throw new InvalidOperationException("No se puede cancelar una venta entregada");

        // Repone el stock de cada producto
        foreach (var detalle in venta.Detalles)
        {
            var producto = await _context.Productos.FindAsync(detalle.IdProducto);
            if (producto != null)
            {
                producto.Stock += detalle.Cantidad;
            }
        }

        // Cambia el estado a cancelada
        venta.Estado = "Cancelada";

        // Guarda los cambios
        await _context.SaveChangesAsync();

        // Retorna la venta cancelada
        return await ObtenerVentaDTOCompleto(id);
    }

    // Verifica el stock disponible de un producto específico
    public async Task<StockVerificacionDTO> VerificarStockProductoAsync(int idProducto, int cantidad)
    {
        // Busca el producto
        var producto = await _context.Productos.FindAsync(idProducto);

        // Si no existe, retorna error
        if (producto == null)
        {
            return new StockVerificacionDTO
            {
                IdProducto = idProducto,
                HayStock = false,
                Mensaje = "El producto no existe"
            };
        }

        // Verifica si está activo
        if (!producto.Activo)
        {
            return new StockVerificacionDTO
            {
                IdProducto = idProducto,
                NombreProducto = producto.NombreProducto,
                StockDisponible = producto.Stock,
                HayStock = false,
                Mensaje = "El producto está inactivo"
            };
        }

        // Verifica el stock
        var hayStock = producto.Stock >= cantidad;
        var mensaje = hayStock 
            ? "Stock disponible" 
            : $"Stock insuficiente. Disponible: {producto.Stock}, solicitado: {cantidad}";

        return new StockVerificacionDTO
        {
            IdProducto = idProducto,
            NombreProducto = producto.NombreProducto,
            StockDisponible = producto.Stock,
            HayStock = hayStock,
            Mensaje = mensaje
        };
    }

    // Verifica el stock de varios productos (para el carrito de compras)
    public async Task<CarritoVerificacionDTO> VerificarStockCarritoAsync(List<VentaDetalleDTO> detalles)
    {
        var resultado = new CarritoVerificacionDTO
        {
            Productos = new List<StockVerificacionDTO>(),
            TodoDisponible = true
        };

        // Verifica cada producto del carrito
        foreach (var detalle in detalles)
        {
            var verificacion = await VerificarStockProductoAsync(detalle.IdProducto, detalle.Cantidad);
            resultado.Productos.Add(verificacion);

            if (!verificacion.HayStock)
                resultado.TodoDisponible = false;
        }

        return resultado;
    }

    // Obtiene las ventas de un usuario específico con paginación
    public async Task<VentaPagedResult> ObtenerVentasPorUsuarioAsync(int idUsuario, int pagina, int tamanioPagina)
    {
        // Primero verifica que el usuario exista
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null)
            throw new InvalidOperationException("El usuario no existe");

        // Obtiene las ventas del usuario
        var (items, total) = await ObtenerHistorialVentasAsync(pagina, tamanioPagina, idUsuario, null, null, null);

        return new VentaPagedResult
        {
            Items = items,
            Total = total,
            Page = pagina,
            PageSize = tamanioPagina
        };
    }

    // Obtiene estadísticas de ventas para el dashboard
    public async Task<object> ObtenerEstadisticasVentasAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        // Query base
        var query = _context.Ventas.AsQueryable();

        // Aplica filtros de fecha si se proporcionan
        if (fechaDesde.HasValue)
            query = query.Where(v => v.FechaCreacion >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(v => v.FechaCreacion <= fechaHasta.Value);

        // Calcula estadísticas
        var totalVentas = await query.CountAsync();
        var totalFacturado = await query.SumAsync(v => v.Total);
        var ventasCanceladas = await query.CountAsync(v => v.Estado == "Cancelada");
        var ventasPendientes = await query.CountAsync(v => v.Estado == "Pendiente");
        var ventasEntregadas = await query.CountAsync(v => v.Estado == "Entregada");

        return new
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

    // Método privado para construir el DTO completo de una venta
    private async Task<VentaDTO?> ObtenerVentaDTOCompleto(int idVenta)
    {
        // Busca la venta con todas las relaciones
        var venta = await _context.Ventas
            .Include(v => v.Usuario)
            .Include(v => v.Direccion)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == idVenta);

        if (venta == null) return null;

        // Construye el DTO de respuesta
        return new VentaDTO
        {
            Id = venta.Id,
            NumeroVenta = venta.NumeroVenta,
            Descuento = venta.Descuento,
            Total = venta.Total,
            MetodoPago = venta.MetodoPago,
            TipoEntrega = venta.TipoEntrega,
            Notas = venta.Notas,
            Estado = venta.Estado,
            FechaCreacion = venta.FechaCreacion,
            IdDireccion = venta.IdDireccion,
            Direccion = venta.Direccion != null 
                ? $"{venta.Direccion.Calle} {venta.Direccion.Numero}" 
                : null,
            IdUsuario = venta.IdUsuario,
            NombreUsuario = venta.Usuario?.NombreUsuario,
            Detalles = venta.Detalles.Select(d => new DetalleVentaDTO
            {
                Id = d.Id,
                IdProducto = d.IdProducto,
                NombreProducto = d.Producto?.NombreProducto,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                SubTotal = d.SubTotal
            }).ToList()
        };
    }
}