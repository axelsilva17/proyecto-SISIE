using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services.Interfaces;

public interface IValidadorAuth
{
    Task<List<string>> ValidarDatosRegistro(RegisterRequest dto);
    Task<List<string>> ValidarDatosLogin(LoginRequest dto);
}
