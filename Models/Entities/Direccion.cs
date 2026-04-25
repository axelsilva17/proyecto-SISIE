using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.Entities;

// Tabla: Direccion
public class Direccion
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(15)]
    public string Calle { get; set; } = string.Empty;
    
    [Required]
    public int Numero { get; set; }
    
    // FK a Ciudad
    public int IdCiudad { get; set; }
    public Ciudad? Ciudad { get; set; }
    
    // Navegación
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}