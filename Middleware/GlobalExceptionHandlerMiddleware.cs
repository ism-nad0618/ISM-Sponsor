using System.Net;
using System.Text.Json;

namespace ISMSponsor.Middleware;

/// <summary>
/// Global exception handling middleware for standardized error responses and preventing information disclosure.
/// OWASP A05: Security Misconfiguration - Prevent error information leakage
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt: {Message}. User: {User}, IP: {IP}", 
                ex.Message, 
                context.User.Identity?.Name ?? "Anonymous",
                context.Connection.RemoteIpAddress);

            await HandleExceptionAsync(context, HttpStatusCode.Forbidden, "Access denied", ex);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}. User: {User}", 
                ex.Message, 
                context.User.Identity?.Name ?? "Anonymous");

            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "Invalid request", ex);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation: {Message}. User: {User}", 
                ex.Message, 
                context.User.Identity?.Name ?? "Anonymous");

            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, "Invalid operation", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}. User: {User}, Path: {Path}, IP: {IP}", 
                ex.Message,
                context.User.Identity?.Name ?? "Anonymous",
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "An error occurred", ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            TraceId = context.TraceIdentifier
        };

        // Only include exception details in development
        if (_environment.IsDevelopment())
        {
            response.DeveloperMessage = ex.Message;
            response.StackTrace = ex.StackTrace;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string? DeveloperMessage { get; set; }
    public string? StackTrace { get; set; }
}

/// <summary>
/// Extension method for adding global exception handler middleware.
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
