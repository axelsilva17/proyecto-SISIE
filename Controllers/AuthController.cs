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

    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Registrar([FromBody] RegisterRequest request)
    {
        var errores = await _validador.ValidarDatosRegistro(request);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        var result = await _authService.RegisterAsync(request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> IniciarSesion([FromBody] LoginRequest request)
    {
        var errores = await _validador.ValidarDatosLogin(request);
        if (errores.Count > 0)
            return BadRequest(new { success = false, message = "Error de validación", errors = errores });

        var result = await _authService.LoginAsync(request);
        if (!result.Success)
            return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult CerrarSesion()
    {
        return Ok(new { message = "Sesión cerrada" });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> ObtenerUsuarioActual()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
            return NotFound();
        return Ok(user);
    }
}