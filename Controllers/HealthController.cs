using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace ISMSponsor.Controllers;

/// <summary>
/// Health check endpoint for deployment readiness and operational monitoring.
/// Public endpoint for load balancers and monitoring systems.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IConfiguration _configuration;

    public HealthController(HealthCheckService healthCheckService, IConfiguration configuration)
    {
        _healthCheckService = healthCheckService;
        _configuration = configuration;
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
    [AllowAnonymous]
    public IActionResult Live()
    {
        // Simple liveness check - process is running
        return Ok(new { status = "Alive", timestamp = DateTime.UtcNow });
    }
}
