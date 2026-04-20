using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.DTOs;

public class ProductoDTO
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string NombreProducto { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    
    [Required(ErrorMessage = "El stock es requerido")]
    [Range(0, 99999, ErrorMessage = "El stock debe estar entre 0 y 99999")]
    public int Stock { get; set; }
    
    [Required(ErrorMessage = "La categoría es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int IdCategoria { get; set; }
    
    public string? NombreCategoria { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

public class ProductoListDTO
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string NombreProducto { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Descripcion { get; set; }
    
    [Required]
    [Range(0.01, 999999.99)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    
    [Required]
    [Range(0, 99999)]
    public int Stock { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int IdCategoria { get; set; }
    
    public string? NombreCategoria { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}

public class ProductoCreateDTO
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string NombreProducto { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    
    [Required(ErrorMessage = "El stock es requerido")]
    [Range(0, 99999, ErrorMessage = "El stock debe estar entre 0 y 99999")]
    public int Stock { get; set; }
    
    [Required(ErrorMessage = "La categoría es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int IdCategoria { get; set; }
}

public class ProductoUpdateDTO
{
    [Required(ErrorMessage = "El ID del producto es requerido")]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string NombreProducto { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    
    [Required(ErrorMessage = "El stock es requerido")]
    [Range(0, 99999, ErrorMessage = "El stock debe estar entre 0 y 99999")]
    public int Stock { get; set; }
    
    [Required(ErrorMessage = "La categoría es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida")]
    public int IdCategoria { get; set; }
}

public class ProductoPagedResult
{
    public IEnumerable<ProductoListDTO> Items { get; set; } = Enumerable.Empty<ProductoListDTO>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}