namespace proyecto_SISIE.Models.DTOs;

// DTO para crear una venta completa con sus detalles
public class VentaCreateDTO
{
    public int Descuento { get; set; }

    public string MetodoPago { get; set; } = string.Empty;

    public string TipoEntrega { get; set; } = string.Empty;

    public string? Notas { get; set; }

    public int? IdDireccion { get; set; }

    // Dirección de envío (cuando no tiene dirección guardada)
    // Puede ser: "Barrio - Casa 5" o "Av. Principal 123"
    public string? DireccionEnvio { get; set; }

    // Departamento (opcional) para edificios o complejos
    public string? Departamento { get; set; }

    // ID de la ciudad para envío
    public int? IdCiudad { get; set; }

    public bool EsEnvio { get; set; }

    // Datos del cliente (opcional)
    public string? DniCliente { get; set; }
    public string? NombreCliente { get; set; }
    public string? TelefonoCliente { get; set; }
    public string? EmailCliente { get; set; }

    // Lista de productos a vender
    public List<VentaDetalleDTO> Detalles { get; set; } = new();
}

// DTO para cada item del detalle de venta
public class VentaDetalleDTO
{
    public int IdProducto { get; set; }

    public int Cantidad { get; set; }
}

// DTO para actualizar una venta (solo estado y notas)
public class VentaUpdateDTO
{
    public string Estado { get; set; } = string.Empty;

    public string? Notas { get; set; }
}

// DTO para respuesta de una venta
public class VentaDTO
{
    public int Id { get; set; }
    public int NumeroVenta { get; set; }
    public int Descuento { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string TipoEntrega { get; set; } = string.Empty;
    public string? Notas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int? IdDireccion { get; set; }
    public string? Direccion { get; set; }
    public int IdUsuario { get; set; }
    public string? NombreUsuario { get; set; }
    public List<DetalleVentaDTO> Detalles { get; set; } = new();
}

// DTO para detalle de venta en la respuesta
public class DetalleVentaDTO
{
    public int Id { get; set; }
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }
}

// DTO para historial de ventas con paginación
public class VentaHistorialDTO
{
    public int Id { get; set; }
    public int NumeroVenta { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int CantidadItems { get; set; }
}

// DTO para verificar disponibilidad de stock antes de agregar al carrito
public class StockVerificacionDTO
{
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public int StockDisponible { get; set; }
    public bool HayStock { get; set; }
    public string? Mensaje { get; set; }
}

// DTO para verificar stock de múltiples productos (carrito)
public class CarritoVerificacionDTO
{
    public List<StockVerificacionDTO> Productos { get; set; } = new();
    public bool TodoDisponible { get; set; }
}

// DTO para estadísticas de ventas
public class VentasEstadisticas
{
    public int TotalVentas { get; set; }
    public decimal TotalFacturado { get; set; }
    public int VentasCanceladas { get; set; }
    public int VentasPendientes { get; set; }
    public int VentasEntregadas { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}