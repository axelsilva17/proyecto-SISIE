using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    
    // Departamento (opcional) para edificios o complejos
    [StringLength(20)]
    public string? Departamento { get; set; }
    
    // FK a Usuario (para saber qué usuario creó esta dirección)
    [ForeignKey(nameof(Usuario))]
    public int IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }
    
    // FK a Ciudad
    public int IdCiudad { get; set; }
    public Ciudad? Ciudad { get; set; }
    
    // Navegación
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}