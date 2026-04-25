using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.Entities;

// Tabla: Provincia
public class Provincia
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string NombreProvincia { get; set; } = string.Empty;
    
    // Navegación
    public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
}