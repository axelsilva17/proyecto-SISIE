using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.Entities;

// Tabla: DetalleVenta (FK Compuesta)
public class DetalleVenta
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    
    [Required]
    public int Cantidad { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    
    // FK Compuesta
    public int IdVenta { get; set; }
    public Venta? Venta { get; set; }
    
    public int IdProducto { get; set; }
    public Producto? Producto { get; set; }
}