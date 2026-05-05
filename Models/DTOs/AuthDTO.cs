namespace proyecto_SISIE.Models.DTOs;

public class RegisterRequest
{
    public string NombreUsuario { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; }
}

public class UserDTO
{
    public string Id { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}