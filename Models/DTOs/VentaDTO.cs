using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.DTOs;

// DTO para crear una venta completa con sus detalles
public class VentaCreateDTO
{
    [Required(ErrorMessage = "El descuento es requerido")]
    [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100")]
    public int Descuento { get; set; }

    [Required(ErrorMessage = "El método de pago es requerido")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "El método de pago debe tener entre 3 y 20 caracteres")]
    public string MetodoPago { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de entrega es requerido")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "El tipo de entrega debe tener entre 3 y 20 caracteres")]
    public string TipoEntrega { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Las notas no pueden exceder 200 caracteres")]
    public string? Notas { get; set; }

    public int? IdDireccion { get; set; }

    // Dirección de envío (cuando no tiene dirección guardada)
    // Puede ser: "Barrio - Casa 5" o "Av. Principal 123"
    [StringLength(100, ErrorMessage = "La dirección no puede exceder 100 caracteres")]
    public string? DireccionEnvio { get; set; }

    // Departamento (opcional) para edificios o complejos
    [StringLength(20, ErrorMessage = "El departamento no puede exceder 20 caracteres")]
    public string? Departamento { get; set; }

    // ID de la ciudad para envío
    public int? IdCiudad { get; set; }

    [Required(ErrorMessage = "La dirección o tipo de entrega debe ser indicado")]
    public bool EsEnvio { get; set; }

    // Lista de productos a vender
    [Required(ErrorMessage = "Debe incluir al menos un producto")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
    public List<VentaDetalleDTO> Detalles { get; set; } = new();
}

// DTO para cada item del detalle de venta
public class VentaDetalleDTO
{
    [Required(ErrorMessage = "El producto es requerido")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "La cantidad es requerida")]
    [Range(1, 9999, ErrorMessage = "La cantidad debe estar entre 1 y 9999")]
    public int Cantidad { get; set; }
}

// DTO para actualizar una venta (solo estado y notas)
public class VentaUpdateDTO
{
    [Required(ErrorMessage = "El estado es requerido")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "El estado debe tener entre 3 y 20 caracteres")]
    public string Estado { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Las notas no pueden exceder 200 caracteres")]
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

// Resultado paginado para historial
public class VentaPagedResult
{
    public IEnumerable<VentaHistorialDTO> Items { get; set; } = Enumerable.Empty<VentaHistorialDTO>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
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