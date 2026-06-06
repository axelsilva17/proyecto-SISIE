namespace proyecto_SISIE.Models.DTOs;

// DTO para crear un cliente
public class ClienteCreateDTO
{
    public string Dni { get; set; } = string.Empty;
    
    public string Nombre { get; set; } = string.Empty;
    
    public string Telefono { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    
    public string? DireccionDefault { get; set; }
    
    public int? NumeroDefault { get; set; }
    
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

