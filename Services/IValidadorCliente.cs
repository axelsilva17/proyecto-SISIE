using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;

namespace proyecto_SISIE.Services;

public interface IValidadorCliente
{
    Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto);
    Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto, int? idCliente);
}

public class ValidadorCliente : IValidadorCliente
{
    private readonly ApplicationDbContext _context;
    
    public ValidadorCliente(ApplicationDbContext context) => _context = context;
    
    public Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(dto.Dni)) errores.Add("El DNI es obligatorio");
        if (string.IsNullOrWhiteSpace(dto.Nombre)) errores.Add("El nombre es obligatorio");
        if (dto.Dni?.Length < 7 || dto.Dni?.Length > 10) errores.Add("El DNI debe tener entre 7 y 10 caracteres");
        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains("@")) errores.Add("El email debe contener @");
        return Task.FromResult(errores);
    }
    
    public async Task<List<string>> ValidarDatosCliente(ClienteCreateDTO dto, int? idCliente)
    {
        var errores = new List<string>();
        
        var query = _context.Clientes.Where(c => c.Dni == dto.Dni);
        if (idCliente.HasValue) query = query.Where(c => c.Id != idCliente.Value);
        if (await query.AnyAsync()) errores.Add("Ya existe un cliente con ese DNI");

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailQuery = _context.Clientes.Where(c => c.Email != null && c.Email.ToLower() == dto.Email.ToLower());
            if (idCliente.HasValue) emailQuery = emailQuery.Where(c => c.Id != idCliente.Value);
            if (await emailQuery.AnyAsync()) errores.Add("Ya existe un cliente con ese email");
        }

        if (dto.IdCiudad.HasValue)
        {
            var ciudadExiste = await _context.Ciudades.AnyAsync(c => c.Id == dto.IdCiudad.Value);
            if (!ciudadExiste) errores.Add("La ciudad no existe");
        }
        
        return errores;
    }
}