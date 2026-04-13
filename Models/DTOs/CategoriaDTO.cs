using System.ComponentModel.DataAnnotations;

namespace proyecto_SISIE.Models.DTOs;

public class CategoriaDTO
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre de categoría es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    public string NombreCategoria { get; set; } = string.Empty;
}

public class CategoriaCreateDTO
{
    [Required(ErrorMessage = "El nombre de categoría es requerido")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9\sáéíóúÁÉÍÓÚñÑ]+$", ErrorMessage = "Solo letras, números y espacios")]
    public string NombreCategoria { get; set; } = string.Empty;
}