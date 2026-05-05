using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.Entities;

// Tabla: Ciudad
public class Ciudad
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string NombreCiudad { get; set; } = string.Empty;
    
    [Required]
    public int Cp { get; set; }
    
    // FK a Provincia
    public int IdProvincia { get; set; }
    public Provincia? Provincia { get; set; }
    
    // Navegación
    public ICollection<Direccion> Direcciones { get; set; } = new List<Direccion>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}