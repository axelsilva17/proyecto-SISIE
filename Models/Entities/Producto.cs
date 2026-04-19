using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.Entities;

public class Producto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string NombreProducto { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Descripcion { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    
    [Required]
    public int Stock { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    [Required]
    public bool Activo { get; set; } = true;
    
    // FK a Categoria
    public int IdCategoria { get; set; }
    public Categoria? Categoria { get; set; }
    
    // Navegación
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}