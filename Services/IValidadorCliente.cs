using System.Collections.Generic;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

// Validador para Cliente
public interface IValidadorCliente
{
    Task<List<string>> ValidarAsync(ClienteCreateDTO dto);
}

public class ValidadorCliente : IValidadorCliente
{
    public Task<List<string>> ValidarAsync(ClienteCreateDTO dto)
    {
        var errores = new List<string>();
        
        if (string.IsNullOrWhiteSpace(dto.Dni))
            errores.Add("El DNI es obligatorio");
        
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            errores.Add("El nombre es obligatorio");
        
        if (dto.Dni?.Length < 7 || dto.Dni?.Length > 10)
            errores.Add("El DNI debe tener entre 7 y 10 caracteres");
        
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains("@"))
            errores.Add("El email debe contener @");
        
        return Task.FromResult(errores);
    }
}