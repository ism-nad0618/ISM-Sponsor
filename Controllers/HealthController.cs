using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using ISMSponsor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ISMSponsor.Models;

namespace ISMSponsor.Controllers;

/// <summary>
/// Health check endpoint for deployment readiness and operational monitoring.
/// Public endpoint for load balancers and monitoring systems.
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HealthController(
        HealthCheckService healthCheckService, 
        IConfiguration configuration,
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _healthCheckService = healthCheckService;
        _configuration = configuration;
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Basic health check endpoint (unauthenticated for load balancer probes).
    /// GET /api/health
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var health = await _healthCheckService.CheckHealthAsync();

        var response = new
        {
            status = health.Status.ToString(),
            timestamp = DateTime.UtcNow,
            version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0"
        };

        var statusCode = health.Status == HealthStatus.Healthy ? 200 :
                         health.Status == HealthStatus.Degraded ? 200 : 503;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Detailed health check with individual component status (authenticated, admin-only).
    /// GET /api/health/detailed
    /// </summary>
    [HttpGet("detailed")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetDetailed()
    {
        var showDetails = _configuration.GetValue<bool>("HealthChecks:DetailedErrors");
        var health = await _healthCheckService.CheckHealthAsync();

        var response = new
        {
            status = health.Status.ToString(),
            timestamp = DateTime.UtcNow,
            version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            checks = health.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data,
                exception = showDetails ? e.Value.Exception?.Message : null
            })
        };

        return Ok(response);
    }

    /// <summary>
    /// Readiness probe endpoint (for Kubernetes-style orchestration).
    /// GET /api/health/ready
    /// </summary>
    [HttpGet("ready")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public async Task<IActionResult> Ready()
    {
        var health = await _healthCheckService.CheckHealthAsync();

        if (health.Status == HealthStatus.Healthy)
        {
            return Ok(new { status = "Ready", timestamp = DateTime.UtcNow });
        }

        return StatusCode(503, new { status = "Not Ready", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Liveness probe endpoint (for Kubernetes-style orchestration).
    /// GET /api/health/live
    /// </summary>
    [HttpGet("live")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public IActionResult Live()
    {
        // Simple liveness check - process is running
        return Ok(new { status = "Alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Check sponsor and user information by sponsor ID (temporary debug endpoint).
    /// GET /api/health/check-sponsor/{sponsorId}
    /// </summary>
    [HttpGet("check-sponsor/{sponsorId}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public async Task<IActionResult> CheckSponsor(string sponsorId)
    {
        var sponsor = await _context.Sponsors
            .Where(s => s.SponsorId == sponsorId)
            .Select(s => new
            {
                s.SponsorId,
                s.SponsorName,
                s.LegalName,
                s.IsActive,
                s.ApprovalStatus,
                s.CreatedOn
            })
            .FirstOrDefaultAsync();

        var users = await _context.Users
            .Where(u => u.SponsorId == sponsorId)
            .Select(u => new
            {
                u.UserName,
                u.DisplayName,
                u.Email,
                u.SponsorId,
                u.IsActive
            })
            .ToListAsync();

        var allSponsorUsers = await _context.Users
            .Where(u => u.SponsorId != null)
            .Select(u => new
            {
                u.UserName,
                u.DisplayName,
                u.SponsorId,
                u.IsActive
            })
            .OrderBy(u => u.UserName)
            .ToListAsync();

        return Ok(new
        {
            sponsorId,
            sponsor,
            usersForThisSponsor = users,
            allSponsorUsers
        });
    }

    /// <summary>
    /// Emergency password reset endpoint (temporary - remove after use).
    /// POST /api/health/reset-password
    /// Body: { "username": "SP001", "newPassword": "YourPassword123!" }
    /// </summary>
    [HttpPost("reset-password")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return NotFound(new { success = false, message = $"User '{request.Username}' not found" });
            }

            // Check if account is locked
            var isLockedOut = await _userManager.IsLockedOutAsync(user);
            if (isLockedOut)
            {
                // Unlock the account
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            // Generate password reset token and reset password properly
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new
                {
                    success = true,
                    message = $"Password reset successfully for user '{request.Username}'",
                    wasLockedOut = isLockedOut,
                    unlocked = isLockedOut
                });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new
            {
                success = false,
                message = "Password reset failed",
                errors = result.Errors.Select(e => e.Description).ToArray()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred",
                error = ex.Message
            });
        }
    }
}

public class ResetPasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
