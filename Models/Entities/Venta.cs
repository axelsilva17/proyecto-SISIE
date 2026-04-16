using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.Entities;

// Tabla: Venta
public class Venta
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int NumeroVenta { get; set; }
    
    public int Descuento { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    
    [Required]
    [StringLength(20)]
    public string MetodoPago { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string TipoEntrega { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Notas { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = "pendiente";
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // FK a Direccion (nullable para ventas por mostrador)
    public int? IdDireccion { get; set; }
    public Direccion? Direccion { get; set; }
    
    // FK a Usuario
    public int IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }
    
    // Navegación
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}