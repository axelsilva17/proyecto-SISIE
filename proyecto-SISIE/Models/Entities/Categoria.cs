using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.Entities;

public class Categoria
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string NombreCategoria { get; set; } = string.Empty;
    
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}