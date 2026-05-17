using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;

namespace proyecto_SISIE.Services;

public interface IValidadorAuth
{
    Task<List<string>> ValidarDatosRegistro(RegisterRequest dto);
    Task<List<string>> ValidarDatosLogin(LoginRequest dto);
}

public class ValidadorAuth : IValidadorAuth
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ValidadorAuth(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<List<string>> ValidarDatosRegistro(RegisterRequest dto)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarFormatoRegistro(dto));
        errores.AddRange(await ValidarEmailUnico(dto.Email));
        return errores;
    }

    public Task<List<string>> ValidarDatosLogin(LoginRequest dto)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarFormatoLogin(dto));
        return Task.FromResult(errores);
    }

    private List<string> ValidarFormatoRegistro(RegisterRequest dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Email)) errores.Add("El email es obligatorio");
        if (string.IsNullOrWhiteSpace(dto.Password)) errores.Add("La contraseña es obligatoria");
        if (string.IsNullOrWhiteSpace(dto.NombreUsuario)) errores.Add("El nombre de usuario es obligatorio");
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains("@"))
            errores.Add("El email debe contener @");
        if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password.Length < 6)
            errores.Add("La contraseña debe tener al menos 6 caracteres");
        return errores;
    }

    private List<string> ValidarFormatoLogin(LoginRequest dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Email)) errores.Add("El email es obligatorio");
        if (string.IsNullOrWhiteSpace(dto.Password)) errores.Add("La contraseña es obligatoria");
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains("@"))
            errores.Add("El email debe contener @");
        return errores;
    }

    private async Task<List<string>> ValidarEmailUnico(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return [];
        var existe = await _userManager.FindByEmailAsync(email);
        return existe != null ? ["El email ya está registrado"] : [];
    }
}
