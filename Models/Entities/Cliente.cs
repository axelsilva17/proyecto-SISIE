using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.Entities;

// Tabla: Cliente (para ventas, separado de usuarios del sistema)
public class Cliente
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(15)]
    public string Dni { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;
    
    [StringLength(40)]
    public string? Email { get; set; }
    
    // Dirección por defecto para envíos
    [StringLength(100)]
    public string? DireccionDefault { get; set; }
    
    public int? NumeroDefault { get; set; }
    
    [StringLength(20)]
    public string? DepartamentoDefault { get; set; }
    
    // FK a Ciudad (opcional - si tiene ciudad asignada)
    public int? IdCiudad { get; set; }
    public Ciudad? Ciudad { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    public bool Activo { get; set; } = true;
    
    // Navegación - ventas realizadas por este cliente
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}