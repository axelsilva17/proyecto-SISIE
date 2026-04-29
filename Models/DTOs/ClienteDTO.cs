using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proyecto_SISIE.Models.DTOs;

// DTO para crear un cliente
public class ClienteCreateDTO
{
    [Required(ErrorMessage = "El DNI es requerido")]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "El DNI debe tener entre 7 y 15 caracteres")]
    public string Dni { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El teléfono es requerido")]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(40)]
    public string? Email { get; set; }
    
    [StringLength(100)]
    public string? DireccionDefault { get; set; }
    
    public int? NumeroDefault { get; set; }
    
    [StringLength(20)]
    public string? DepartamentoDefault { get; set; }
    
    public int? IdCiudad { get; set; }
}

// DTO para actualizar un cliente
public class ClienteUpdateDTO
{
    [Required(ErrorMessage = "El ID es requerido")]
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El DNI es requerido")]
    [StringLength(15, MinimumLength = 7, ErrorMessage = "El DNI debe tener entre 7 y 15 caracteres")]
    public string Dni { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El teléfono es requerido")]
    [StringLength(20)]
    public string Telefono { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(40)]
    public string? Email { get; set; }
    
    [StringLength(100)]
    public string? DireccionDefault { get; set; }
    
    public int? NumeroDefault { get; set; }
    
    [StringLength(20)]
    public string? DepartamentoDefault { get; set; }
    
    public int? IdCiudad { get; set; }
}

// DTO para respuesta de cliente
public class ClienteDTO
{
    public int Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DireccionDefault { get; set; }
    public int? NumeroDefault { get; set; }
    public string? DepartamentoDefault { get; set; }
    public int? IdCiudad { get; set; }
    public string? NombreCiudad { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
    public int CantidadVentas { get; set; }
}

// Lista paginada de clientes
public class ClientePagedResult
{
    public IEnumerable<ClienteDTO> Items { get; set; } = Enumerable.Empty<ClienteDTO>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}