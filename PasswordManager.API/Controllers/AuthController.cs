using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PasswordManager.API.Extensions;
using PasswordManager.API.Models;
using PasswordManager.Application.Auth;
using PasswordManager.Application.Interfaces;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;

namespace PasswordManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    IGoogleTokenValidator googleValidator,
    IJwtService jwtService,
    IUserRepository userRepo,
    IRefreshTokenStore tokenStore) : ControllerBase
{
    // POST api/auth/login
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(
        [FromBody] GoogleLoginRequest request,
        CancellationToken ct)
    {
        // 1 — Validar el token de Google
        var googleUser = await googleValidator.ValidateAsync(request.IdToken, ct);

        if (googleUser is null)
            return Unauthorized(new { code = "INVALID_GOOGLE_TOKEN", message = "Token de Google inválido." });

        // 2 — Buscar o crear el usuario
        var user = await userRepo.GetByGoogleIdAsync(googleUser.GoogleId, ct);

        if (user is null)
        {
            // Crear un nuevo usuario con el entities no de claims eliminamos esa ambiguedad
            // y nos aseguramos de que el usuario se cree correctamente en la base de datos.
            user = Domain.Entities.User.Create(googleUser.GoogleId, googleUser.Email);
            await userRepo.AddAsync(user, ct);
        }

        // 3 — Emitir tokens
        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = jwtService.GenerateRefreshToken();

        await tokenStore.SaveAsync(refreshToken, user.Id, TimeSpan.FromHours(1), ct);

        return Ok(new { accessToken, refreshToken });
    }

    // POST api/auth/refresh
    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        // 1 — Validar que el refresh token existe en Redis
        var userId = await tokenStore.GetUserIdAsync(request.RefreshToken, ct);

        if (userId is null)
            return Unauthorized(new { code = "INVALID_REFRESH_TOKEN", message = "Refresh token inválido o expirado." });

        // 2 — Buscar el usuario
        var user = await userRepo.GetByIdAsync(userId.Value, CancellationToken.None);

        if (user is null)
            return Unauthorized(new { code = "USER_NOT_FOUND", message = "Usuario no encontrado." });

        // 3 — Revocar el token usado (rotación de tokens)
        await tokenStore.RevokeAsync(request.RefreshToken, CancellationToken.None);

        // 4 — Emitir nuevos tokens
        var newAccessToken = jwtService.GenerateAccessToken(user.Id, user.Email);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        await tokenStore.SaveAsync(newRefreshToken, user.Id, TimeSpan.FromHours(1), CancellationToken.None);

        return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        await tokenStore.RevokeAsync(request.RefreshToken, ct);

        return NoContent();
    }
}