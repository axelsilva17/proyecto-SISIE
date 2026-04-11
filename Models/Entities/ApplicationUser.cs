using Microsoft.AspNetCore.Identity;

namespace proyecto_SISIE.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string? NombreCompleto { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}