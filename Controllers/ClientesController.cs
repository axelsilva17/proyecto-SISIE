using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Controllers;

// Controlador para gestionar clientes
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly IValidadorCliente _validador;

    public ClientesController(IClienteService clienteService, IValidadorCliente validador)
    {
        _clienteService = clienteService;
        _validador = validador;
    }

    // Busca clientes por nombre (para autocomplete)
    [HttpGet]
    public async Task<ActionResult> Buscar([FromQuery] string? nombre = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                var (items, total) = await _clienteService.ObtenerTodosAsync(1, 20, null, true);
                return Ok(items);
            }

            var busqueda = nombre.Trim();
            var (resultados, total2) = await _clienteService.ObtenerTodosAsync(1, 20, busqueda, true);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    // Busca un cliente por su DNI
    [HttpGet("dni/{dni}")]
    public async Task<ActionResult<ClienteDTO>> BuscarPorDni(string dni)
    {
        var cliente = await _clienteService.BuscarPorDniAsync(dni);

        if (cliente == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(cliente);
    }

// Obtiene un cliente por su ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ClienteDTO>> ObtenerPorId(int id)
    {
        var cliente = await _clienteService.ObtenerPorIdAsync(id);

        if (cliente == null)
            return NotFound(new { message = "Cliente no encontrado" });

        return Ok(cliente);
    }
// Agrega un nuevo cliente (si ya existe, retorna el existente)
    [HttpPost]
    public async Task<ActionResult<ClienteDTO>> Agregar([FromBody] ClienteCreateDTO clienteDto)
    {
        // Valida usando el validador desacoplado
        var errores = await _validador.ValidarAsync(clienteDto);
        if (errores.Count > 0)
        {
            return BadRequest(new { 
                success = false, 
                message = "Error de validación",
                errors = errores 
            });
        }

        try
        {
            // Verificar si ya existe
            var existente = await _clienteService.BuscarPorDniAsync(clienteDto.Dni);
            if (existente != null)
                return Ok(existente);

            var created = await _clienteService.AgregarAsyncCliente(clienteDto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}