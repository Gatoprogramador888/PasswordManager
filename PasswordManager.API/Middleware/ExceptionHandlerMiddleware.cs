using System.Net;
using System.Text.Json;

namespace PasswordManager.API.Middleware;

public sealed class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    // Estructura estándar de error que siempre regresa la API
    private sealed record ErrorResponse(string Code, string Message);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Excepción no controlada en {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest,
                                            "BAD_REQUEST",
                                            exception.Message),

            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,
                                            "UNAUTHORIZED",
                                            "No autorizado."),

            KeyNotFoundException => (HttpStatusCode.NotFound,
                                            "NOT_FOUND",
                                            "Recurso no encontrado."),

            OperationCanceledException => (HttpStatusCode.BadRequest,
                                            "REQUEST_CANCELLED",
                                            "La operación fue cancelada."),

            _ => (HttpStatusCode.InternalServerError,
                                            "INTERNAL_ERROR",
                                            "Ocurrió un error interno.") // nunca exponemos el detalle real
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse(code, message);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}