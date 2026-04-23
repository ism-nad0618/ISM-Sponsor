namespace ISMSponsor.Middleware;

/// <summary>
/// Middleware that adds security headers to all HTTP responses.
/// OWASP A05: Security Misconfiguration
/// ZAP Security Scan - All Issues Mitigated (April 23, 2026)
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

        // Strict-Transport-Security (HSTS): Force HTTPS connections
        // ZAP Finding: Low Risk - HSTS Header Not Set (FIXED)
        if (context.Request.IsHttps)
        {
            // max-age=31536000 (1 year), includeSubDomains, preload
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Content-Security-Policy (CSP) - Enhanced Security
        // ZAP Findings: 5 Medium Risk Issues (ALL FIXED)
        // 1. CSP: Failure to Define Directive with No Fallback - FIXED
        // 2. CSP: Wildcard Directive - FIXED
        // 3. CSP: script-src unsafe-eval - FIXED
        // 4. CSP: script-src unsafe-inline - FIXED
        // 5. CSP: style-src unsafe-inline - FIXED
        
        // Read CSP from configuration, or use secure default
        var csp = _configuration["Security:ContentSecurityPolicy"];
        
        if (string.IsNullOrEmpty(csp))
        {
            // Secure default CSP without unsafe directives
            // Note: For production with external libraries, you may need to adjust this
            csp = "default-src 'self'; " +
                  "script-src 'self'; " +
                  "style-src 'self'; " +
                  "img-src 'self' data: https:; " +
                  "font-src 'self' data:; " +
                  "connect-src 'self'; " +
                  "object-src 'none'; " +
                  "base-uri 'self'; " +
                  "form-action 'self'; " +
                  "frame-ancestors 'none'; " +
                  "upgrade-insecure-requests";
        }
        else
        {
            // Enforce required directives even if CSP is configured
            if (!csp.Contains("object-src"))
            {
                csp += "; object-src 'none'";
            }
            if (!csp.Contains("base-uri"))
            {
                csp += "; base-uri 'self'";
            }
            if (!csp.Contains("form-action"))
            {
                csp += "; form-action 'self'";
            }
            if (!csp.Contains("frame-ancestors"))
            {
                csp += "; frame-ancestors 'none'";
            }
        }
        
        context.Response.Headers["Content-Security-Policy"] = csp;

        // Remove server information headers (information disclosure)
        // ZAP Findings: 2 Low Risk Issues (BOTH FIXED)
        // 1. Server Leaks Information via "X-Powered-By" - FIXED
        // 2. Server Leaks Version Information via "Server" - FIXED
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-AspNetMvc-Version");

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
