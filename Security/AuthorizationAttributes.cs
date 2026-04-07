using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ISMSponsor.Security;

/// <summary>
/// Authorization attribute that restricts access to Admin role only.
/// OWASP A01: Broken Access Control - Enforces role-based authorization.
/// </summary>
public class AdminOnlyAttribute : TypeFilterAttribute
{
    public AdminOnlyAttribute() : base(typeof(AdminOnlyFilter))
    {
    }
}

public class AdminOnlyFilter : IAuthorizationFilter
{
    private readonly ILogger<AdminOnlyFilter> _logger;

    public AdminOnlyFilter(ILogger<AdminOnlyFilter> logger)
    {
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthorized access attempt to admin-only resource from unauthenticated user. IP: {IP}", 
                context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            return;
        }

        if (!user.IsInRole("admin"))
        {
            var userEmail = user.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Authorization failure: User {User} attempted to access admin-only resource without admin role. IP: {IP}", 
                userEmail, context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// Authorization attribute that restricts access to Staff roles only (Admin, Admissions, Cashier).
/// Sponsors are explicitly denied.
/// OWASP A01: Broken Access Control
/// </summary>
public class StaffOnlyAttribute : TypeFilterAttribute
{
    public StaffOnlyAttribute() : base(typeof(StaffOnlyFilter))
    {
    }
}

public class StaffOnlyFilter : IAuthorizationFilter
{
    private readonly ILogger<StaffOnlyFilter> _logger;
    private static readonly string[] StaffRoles = { "admin", "admissions", "cashier" };

    public StaffOnlyFilter(ILogger<StaffOnlyFilter> logger)
    {
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Unauthorized access attempt to staff-only resource from unauthenticated user. IP: {IP}", 
                context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            return;
        }

        // Check if user is sponsor (explicitly deny)
        if (user.IsInRole("sponsor"))
        {
            var userEmail = user.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Authorization failure: Sponsor user {User} attempted to access staff-only resource. IP: {IP}", 
                userEmail, context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new ForbidResult();
            return;
        }

        // Check if user has any staff role
        if (!StaffRoles.Any(role => user.IsInRole(role)))
        {
            var userEmail = user.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Authorization failure: User {User} attempted to access staff-only resource without staff role. IP: {IP}", 
                userEmail, context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// Authorization attribute that allows Admin and Admissions roles.
/// OWASP A01: Broken Access Control
/// </summary>
public class AdminOrAdmissionsAttribute : TypeFilterAttribute
{
    public AdminOrAdmissionsAttribute() : base(typeof(AdminOrAdmissionsFilter))
    {
    }
}

public class AdminOrAdmissionsFilter : IAuthorizationFilter
{
    private readonly ILogger<AdminOrAdmissionsFilter> _logger;

    public AdminOrAdmissionsFilter(ILogger<AdminOrAdmissionsFilter> logger)
    {
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            return;
        }

        if (!user.IsInRole("admin") && !user.IsInRole("admissions"))
        {
            var userEmail = user.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Authorization failure: User {User} attempted to access admin/admissions resource. IP: {IP}", 
                userEmail, context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// Authorization attribute that allows Sponsor role only.
/// Staff are explicitly denied (isolation requirement).
/// OWASP A01: Broken Access Control
/// </summary>
public class SponsorOnlyAttribute : TypeFilterAttribute
{
    public SponsorOnlyAttribute() : base(typeof(SponsorOnlyFilter))
    {
    }
}

public class SponsorOnlyFilter : IAuthorizationFilter
{
    private readonly ILogger<SponsorOnlyFilter> _logger;

    public SponsorOnlyFilter(ILogger<SponsorOnlyFilter> logger)
    {
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            return;
        }

        if (!user.IsInRole("sponsor"))
        {
            var userEmail = user.Identity?.Name ?? "Unknown";
            _logger.LogWarning("Authorization failure: Non-sponsor user {User} attempted to access sponsor-only resource. IP: {IP}", 
                userEmail, context.HttpContext.Connection.RemoteIpAddress);
            context.Result = new ForbidResult();
            return;
        }
    }
}
