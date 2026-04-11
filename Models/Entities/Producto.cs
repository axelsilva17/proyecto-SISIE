using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.Entities;

public class Producto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Descripcion { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }
    
    public int Stock { get; set; }
    
    public int IdCategoria { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    public bool Activo { get; set; } = true;
    
    // Navegación
    public Categoria? Categoria { get; set; }
}