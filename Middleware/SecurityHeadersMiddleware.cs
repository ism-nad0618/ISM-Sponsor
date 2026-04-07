namespace ISMSponsor.Middleware;

/// <summary>
/// Middleware that adds security headers to all HTTP responses.
/// OWASP A05: Security Misconfiguration
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _next = next;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Content-Type-Options: Prevent MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // X-Frame-Options: Clickjacking protection
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // X-XSS-Protection: XSS filter (legacy but still useful)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer-Policy: Control referrer information
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions-Policy: Restrict browser features
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // Content-Security-Policy (CSP)
        var csp = _configuration["Security:ContentSecurityPolicy"];
        if (!string.IsNullOrEmpty(csp))
        {
            context.Response.Headers["Content-Security-Policy"] = csp;
        }

        // Remove server header (information disclosure)
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");

        await _next(context);
    }
}

/// <summary>
/// Extension method for adding security headers middleware.
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
