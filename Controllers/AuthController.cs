using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Services;
using proyecto_SISIE.Services.Interfaces;
using System.Security.Claims;

namespace proyecto_SISIE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidadorAuth _validador;

    public AuthController(IAuthService authService, IValidadorAuth validador)
    {
        _authService = authService;
        _validador = validador;
    }

    // Registra un nuevo usuario en el sistema
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Registrar([FromBody] RegisterRequest request)
    {
        // Valida usando el validador desacoplado
        var errores = await _validador.ValidarRegistroAsync(request);
        if (errores.Count > 0)
        {
            return BadRequest(new { 
                success = false, 
                message = "Error de validación",
                errors = errores 
            });
        }

        // Registra el usuario
        var result = await _authService.RegisterAsync(request);
        
        // Si falla (email duplicado, etc), retorna error
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // Inicia sesión y retorna JWT token
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> IniciarSesion([FromBody] LoginRequest request)
    {
        // Valida usando el validador desacoplado
        var errores = await _validador.ValidarLoginAsync(request);
        if (errores.Count > 0)
        {
            return BadRequest(new { 
                success = false, 
                message = "Error de validación",
                errors = errores 
            });
        }

        // Verifica credenciales
        var result = await _authService.LoginAsync(request);

        // Si son incorrectas, retorna 401
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    // Cierra la sesión del usuario (JWT es stateless, solo responde OK)
    [HttpPost("logout")]
    [Authorize]
    public IActionResult CerrarSesion()
    {
        return Ok(new { message = "Sesión cerrada" });
    }

    // Obtiene los datos del usuario actualmente logueado
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> ObtenerUsuarioActual()
    {
        // Extrae el ID del usuario del token JWT
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Obtiene los datos del usuario
        var user = await _authService.GetCurrentUserAsync(userId);
        
        if (user == null)
            return NotFound();

        return Ok(user);
    }
}