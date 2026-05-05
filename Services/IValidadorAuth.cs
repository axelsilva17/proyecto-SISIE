using System.Collections.Generic;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

// Validador para Auth
public interface IValidadorAuth
{
    Task<List<string>> ValidarRegistroAsync(RegisterRequest dto);
    Task<List<string>> ValidarLoginAsync(LoginRequest dto);
}

public class ValidadorAuth : IValidadorAuth
{
    public Task<List<string>> ValidarRegistroAsync(RegisterRequest dto)
    {
        var errores = new List<string>();
        
        if (string.IsNullOrWhiteSpace(dto.Email))
            errores.Add("El email es obligatorio");
        
        if (string.IsNullOrWhiteSpace(dto.Password))
            errores.Add("La contraseña es obligatoria");
        
        if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
            errores.Add("El nombre de usuario es obligatorio");
        
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains("@"))
            errores.Add("El email debe contener @");
        
        if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password.Length < 6)
            errores.Add("La contraseña debe tener al menos 6 caracteres");
        
        return Task.FromResult(errores);
    }

    public Task<List<string>> ValidarLoginAsync(LoginRequest dto)
    {
        var errores = new List<string>();
        
        if (string.IsNullOrWhiteSpace(dto.Email))
            errores.Add("El email es obligatorio");
        
        if (string.IsNullOrWhiteSpace(dto.Password))
            errores.Add("La contraseña es obligatoria");
        
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains("@"))
            errores.Add("El email debe contener @");
        
        return Task.FromResult(errores);
    }
}