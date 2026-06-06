using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class ValidadorCliente : IValidadorCliente
{
    private readonly IClienteRepositorio _clienteRepositorio;

    public ValidadorCliente(IClienteRepositorio clienteRepositorio) => _clienteRepositorio = clienteRepositorio;

    public async Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarFormato(dto));
        errores.AddRange(await ValidarCiudadExiste(dto.IdCiudad));
        return errores;
    }

    public async Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto, int? idCliente)
    {
        var errores = new List<string>();
        errores.AddRange(ValidarFormato(dto));
        errores.AddRange(await ValidarUnicos(dto, idCliente));
        errores.AddRange(await ValidarCiudadExiste(dto.IdCiudad));
        return errores;
    }

    private List<string> ValidarFormato(ClienteCreateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Dni)) errores.Add("El DNI es obligatorio");
        else if (dto.Dni.Length < 7 || dto.Dni.Length > 15)
            errores.Add("El DNI debe tener entre 7 y 15 caracteres");

        if (string.IsNullOrWhiteSpace(dto.Nombre)) errores.Add("El nombre es obligatorio");
        else if (dto.Nombre.Length < 3 || dto.Nombre.Length > 100)
            errores.Add("El nombre debe tener entre 3 y 100 caracteres");

        if (string.IsNullOrWhiteSpace(dto.Telefono)) errores.Add("El teléfono es obligatorio");
        else if (dto.Telefono.Length < 8 || dto.Telefono.Length > 20)
            errores.Add("El teléfono debe tener entre 8 y 20 caracteres");

        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains("@"))
            errores.Add("El email debe contener @");

        return errores;
    }

    private async Task<List<string>> ValidarUnicos(ClienteCreateDTO dto, int? idCliente)
    {
        var errores = new List<string>();

        if (await _clienteRepositorio.ExisteDniAsync(dto.Dni, idCliente))
            errores.Add("Ya existe un cliente con ese DNI");

        if (!string.IsNullOrWhiteSpace(dto.Email)
            && await _clienteRepositorio.ExisteEmailAsync(dto.Email, idCliente))
            errores.Add("Ya existe un cliente con ese email");

        return errores;
    }

    private async Task<List<string>> ValidarCiudadExiste(int? idCiudad)
    {
        if (!idCiudad.HasValue) return [];
        var existe = await _clienteRepositorio.ExisteCiudadAsync(idCiudad.Value);
        return existe ? [] : ["La ciudad no existe"];
    }
}
