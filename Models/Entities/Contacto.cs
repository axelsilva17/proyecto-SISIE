using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.Entities;

// Tabla: Contacto
public class Contacto
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(40)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public int Telefono { get; set; }
}