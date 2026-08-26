using System.Security.Claims;

namespace PasswordManager.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("sub");

        if (Guid.TryParse(value, out var id))
            return id;

        throw new UnauthorizedAccessException("Token inválido — UserId no encontrado.");
    }
}