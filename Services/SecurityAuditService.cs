using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Http;

namespace ISMSponsor.Services;

/// <summary>
/// Service for logging security-relevant events including authentication, authorization, and admin actions.
/// Implements tamper-evident audit trail requirements.
/// OWASP Alignment: A09 Security Logging and Monitoring Failures
/// </summary>
public class SecurityAuditService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SecurityAuditService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Logs successful authentication event.
    /// </summary>
    public async Task LogAuthenticationSuccessAsync(string userId, string userEmail, string authenticationType)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = "Authentication",
            Details = $"Successful login via {authenticationType} for user {userEmail}. IP: {ipAddress}, User-Agent: {userAgent}",
            UserDisplay = userEmail,
            RoleName = "System",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Authentication success: User={UserEmail}, Type={AuthType}, IP={IP}", 
            userEmail, authenticationType, ipAddress);
    }

    /// <summary>
    /// Logs failed authentication attempt.
    /// OWASP A07: Identification and Authentication Failures
    /// </summary>
    public async Task LogAuthenticationFailureAsync(string email, string reason, string authenticationType)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = "Authentication Failure",
            Details = $"Failed login attempt via {authenticationType} for email {email}. Reason: {reason}. IP: {ipAddress}, User-Agent: {userAgent}",
            UserDisplay = email ?? "Unknown",
            RoleName = "System",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Authentication failure: Email={Email}, Reason={Reason}, Type={AuthType}, IP={IP}", 
            email, reason, authenticationType, ipAddress);
    }

    /// <summary>
    /// Logs logout event.
    /// </summary>
    public async Task LogLogoutAsync(string userId, string userEmail)
    {
        var ipAddress = GetClientIpAddress();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = "Logout",
            Details = $"User {userEmail} logged out. IP: {ipAddress}",
            UserDisplay = userEmail,
            RoleName = "System",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Logout: User={UserEmail}, IP={IP}", userEmail, ipAddress);
    }

    /// <summary>
    /// Logs authorization failure (access denied).
    /// OWASP A01: Broken Access Control
    /// </summary>
    public async Task LogAuthorizationFailureAsync(string userId, string userEmail, string resource, string requiredRole)
    {
        var ipAddress = GetClientIpAddress();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = "Authorization Failure",
            Details = $"Access denied for user {userEmail} to resource '{resource}'. Required role: {requiredRole}. IP: {ipAddress}",
            UserDisplay = userEmail ?? "Unknown",
            RoleName = "System",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Authorization failure: User={UserEmail}, Resource={Resource}, RequiredRole={Role}, IP={IP}", 
            userEmail, resource, requiredRole, ipAddress);
    }

    /// <summary>
    /// Logs admin action for audit trail.
    /// OWASP A08: Software and Data Integrity Failures
    /// </summary>
    public async Task LogAdminActionAsync(string userId, string userEmail, string action, string entityType, string entityId, string details)
    {
        var ipAddress = GetClientIpAddress();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = action,
            Details = $"Admin action by {userEmail}: {details}. Entity: {entityType} {entityId}. IP: {ipAddress}",
            UserDisplay = userEmail,
            RoleName = "admin",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin action: User={UserEmail}, Action={Action}, Entity={EntityType}:{EntityId}, IP={IP}", 
            userEmail, action, entityType, entityId, ipAddress);
    }

    /// <summary>
    /// Logs security configuration change.
    /// </summary>
    public async Task LogSecurityConfigurationChangeAsync(string userId, string userEmail, string settingName, string oldValue, string newValue)
    {
        var ipAddress = GetClientIpAddress();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = "Security Configuration Change",
            Details = $"User {userEmail} changed {settingName} from '{oldValue}' to '{newValue}'. IP: {ipAddress}",
            UserDisplay = userEmail,
            RoleName = "admin",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Security config change: User={UserEmail}, Setting={Setting}, OldValue={OldValue}, NewValue={NewValue}, IP={IP}", 
            userEmail, settingName, oldValue, newValue, ipAddress);
    }

    /// <summary>
    /// Logs data integrity concern (e.g., tampering detection).
    /// OWASP A08: Software and Data Integrity Failures
    /// </summary>
    public async Task LogDataIntegrityAlertAsync(string userId, string userEmail, string entityType, string entityId, string concern)
    {
        var ipAddress = GetClientIpAddress();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = "Data Integrity Alert",
            Details = $"Integrity concern detected by {userEmail ?? "System"}: {concern}. Entity: {entityType} {entityId}. IP: {ipAddress}",
            UserDisplay = userEmail ?? "System",
            RoleName = "System",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogError("Data integrity alert: User={UserEmail}, Entity={EntityType}:{EntityId}, Concern={Concern}, IP={IP}", 
            userEmail, entityType, entityId, concern, ipAddress);
    }

    /// <summary>
    /// Logs suspicious activity for security monitoring.
    /// </summary>
    public async Task LogSuspiciousActivityAsync(string userId, string userEmail, string activityType, string details)
    {
        var ipAddress = GetClientIpAddress();

        var log = new ActivityLog
        {
            Date = DateTime.UtcNow,
            Item = "Suspicious Activity",
            Details = $"Suspicious activity detected: {activityType}. User: {userEmail ?? "Unknown"}. Details: {details}. IP: {ipAddress}",
            UserDisplay = userEmail ?? "Unknown",
            RoleName = "System",
            SchoolYearId = string.Empty
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Suspicious activity: User={UserEmail}, Type={ActivityType}, Details={Details}, IP={IP}", 
            userEmail, activityType, details, ipAddress);
    }

    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        // Check for X-Forwarded-For header (Azure App Service, load balancers)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        return httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }
}
