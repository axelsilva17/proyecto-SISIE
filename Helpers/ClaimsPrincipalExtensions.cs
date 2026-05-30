using System.Security.Claims;

namespace proyecto_SISIE.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int ObtenerIdUsuario(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            throw new UnauthorizedAccessException("No se encontró el ID del usuario en el token");

        if (int.TryParse(claim.Value, out int id))
            return id;

        throw new UnauthorizedAccessException("El ID del usuario en el token no es un número válido");
    }
}
