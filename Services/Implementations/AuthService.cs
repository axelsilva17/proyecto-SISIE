using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using proyecto_SISIE.Data;
using proyecto_SISIE.Models.DTOs;
using proyecto_SISIE.Models.Entities;
using proyecto_SISIE.Services.Interfaces;

namespace proyecto_SISIE.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser { UserName = request.NombreUsuario, Email = request.Email, NombreCompleto = request.NombreUsuario, FechaCreacion = DateTime.Now, Activo = true };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return new AuthResult { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };

        // Crear registro en la tabla Usuario (negocio) vinculado al Identity user
        if (!await _context.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario))
        {
            var contacto = new Contacto
            {
                Email = request.Email,
                Telefono = 0
            };
            _context.Contactos.Add(contacto);
            await _context.SaveChangesAsync();

            _context.Usuarios.Add(new Usuario
            {
                NombreUsuario = request.NombreUsuario,
                PasswordHash = request.Password,
                FechaCreacion = DateTime.Now,
                Activo = true,
                IdContacto = contacto.Id
            });
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);
        return new AuthResult { Success = true, Token = token, Message = "Usuario registrado exitosamente", UserId = user.Id };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.Activo) return new AuthResult { Success = false, Message = "Credenciales inválidas" };

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded) return new AuthResult { Success = false, Message = "Credenciales inválidas" };

        // Asegurar que exista registro en Usuario (negocio) — útil para usuarios creados antes de esta migración
        if (!await _context.Usuarios.AnyAsync(u => u.NombreUsuario == user.UserName))
        {
            var contacto = new Contacto { Email = request.Email, Telefono = 0 };
            _context.Contactos.Add(contacto);
            await _context.SaveChangesAsync();

            _context.Usuarios.Add(new Usuario
            {
                NombreUsuario = user.UserName!,
                PasswordHash = request.Password,
                FechaCreacion = DateTime.Now,
                Activo = true,
                IdContacto = contacto.Id
            });
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);
        return new AuthResult { Success = true, Token = token, Message = "Login exitoso", UserId = user.Id };
    }

    public async Task<UserDTO?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        return new UserDTO { Id = user.Id, NombreUsuario = user.UserName ?? string.Empty, Email = user.Email ?? string.Empty };
    }

    public async Task<int?> ObtenerIdUsuarioPorNombreAsync(string nombreUsuario)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.NombreUsuario == nombreUsuario && u.Activo)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();

        if (usuario.HasValue) return usuario.Value;

        // Lazy creation: si el usuario existe en Identity pero no en la tabla Usuario, lo creamos
        var identityUser = await _userManager.FindByNameAsync(nombreUsuario);
        if (identityUser == null) return null;

        var contacto = new Contacto { Email = identityUser.Email ?? "", Telefono = 0 };
        _context.Contactos.Add(contacto);
        await _context.SaveChangesAsync();

        var nuevoUsuario = new Usuario
        {
            NombreUsuario = nombreUsuario,
            PasswordHash = "auth-migration",
            FechaCreacion = DateTime.Now,
            Activo = true,
            IdContacto = contacto.Id
        };
        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        return nuevoUsuario.Id;
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada en appsettings.json");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.UserName ?? string.Empty), new Claim(ClaimTypes.Email, user.Email ?? string.Empty) };
        var token = new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"] ?? "proyecto-SISIE", audience: _configuration["Jwt:Audience"] ?? "proyecto-SISIE",
            claims: claims, expires: DateTime.Now.AddHours(int.Parse(_configuration["Jwt:ExpiresInHours"] ?? "24")), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
