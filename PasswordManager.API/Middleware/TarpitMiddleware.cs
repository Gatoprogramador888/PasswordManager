using System.Collections.Concurrent;

namespace PasswordManager.API.Middleware;

public sealed class TarpitMiddleware(RequestDelegate next, ILogger<TarpitMiddleware> logger)
{
    // ConcurrentDictionary porque múltiples requests llegan al mismo tiempo
    // string = IP, int = intentos fallidos
    private static readonly ConcurrentDictionary<string, int> _failedAttempts = new();

    // Rutas donde el tarpit aplica — solo endpoints sensibles
    private static readonly HashSet<string> _protectedPaths =
    [
        "/api/auth/login",
        "/api/auth/refresh"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        // Solo actúa en rutas protegidas
        if (!_protectedPaths.Contains(context.Request.Path.Value ?? string.Empty))
            return;

        // Solo actúa en respuestas fallidas de auth
        if (context.Response.StatusCode is not (401 or 403))
        {
            // Login exitoso — limpia el contador de esa IP
            var ip = GetIp(context);
            _failedAttempts.TryRemove(ip, out _);
            return;
        }

        await ApplyTarpitAsync(context);
    }

    private async Task ApplyTarpitAsync(HttpContext context)
    {
        var ip = GetIp(context);
        var attempts = _failedAttempts.AddOrUpdate(ip, 1, (_, count) => count + 1);

        var delayMs = attempts switch
        {
            >= 10 => 60_000, // 1 minuto — ya es un ataque claro
            >= 7 => 30_000,
            >= 5 => 10_000,
            >= 3 => 3_000,
            _ => 0  // primeros 2 intentos sin penalización
        };

        if (delayMs == 0) return;

        logger.LogWarning(
            "Tarpit activado para IP {Ip} — intento #{Attempts} — delay {DelayMs}ms",
            ip, attempts, delayMs);

        try
        {
            // Task.Delay async — el hilo se libera, el servidor sigue respondiendo a otros
            // context.RequestAborted — si el atacante cancela, nosotros tampoco gastamos más
            await Task.Delay(delayMs, context.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            // El cliente cerró la conexión durante el delay — está bien, no es un error
            logger.LogInformation("Tarpit cancelado por el cliente {Ip}", ip);
        }
    }

    private static string GetIp(HttpContext context)
    {
        // X-Forwarded-For cuando hay Nginx por delante
        // Si no hay header, usa la IP directa de la conexión
        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? context.Connection.RemoteIpAddress?.ToString()
               ?? "unknown";
    }
}