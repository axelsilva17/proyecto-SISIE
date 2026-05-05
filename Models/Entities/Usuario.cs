using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.Entities;

// Tabla: Usuario (tabla propia, no usa Identity)
public class Usuario
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string NombreUsuario { get; set; } = string.Empty;
    
    [Required]
    [StringLength(30)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    [Required]
    public bool Activo { get; set; } = true;
    
    // FK a Contacto
    public int IdContacto { get; set; }
    public Contacto? Contacto { get; set; }
    
    // Navegación
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}