using ISMSponsor.Data;
using ISMSponsor.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ISMSponsor.HealthChecks;

/// <summary>
/// Health check for database connectivity.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple query to verify database connection
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (canConnect)
            {
                return HealthCheckResult.Healthy("Database connection successful");
            }

            return HealthCheckResult.Unhealthy("Database connection failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection error", ex);
        }
    }
}

/// <summary>
/// Health check for configuration validity.
/// </summary>
public class ConfigurationHealthCheck : IHealthCheck
{
    private readonly ConfigurationValidationService _configValidation;

    public ConfigurationHealthCheck(ConfigurationValidationService configValidation)
    {
        _configValidation = configValidation;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = _configValidation.GetConfigurationSummary();
            return Task.FromResult(HealthCheckResult.Healthy("Configuration valid", summary));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Configuration invalid", ex));
        }
    }
}

/// <summary>
/// Health check for integration sync status.
/// </summary>
public class SyncHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public SyncHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check recent sync failures (last 24 hours)
            var yesterday = DateTime.UtcNow.AddHours(-24);
            var recentFailures = await Task.Run(() =>
                _context.SyncLogs
                    .Where(s => s.Status == "Failed" && s.AttemptedAt >= yesterday)
                    .Count(), cancellationToken);

            var totalRecent = await Task.Run(() =>
                _context.SyncLogs
                    .Where(s => s.AttemptedAt >= yesterday)
                    .Count(), cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["RecentSyncFailures"] = recentFailures,
                ["TotalRecentSyncs"] = totalRecent,
                ["FailureRate"] = totalRecent > 0 ? (double)recentFailures / totalRecent : 0
            };

            if (recentFailures == 0)
            {
                return HealthCheckResult.Healthy("No recent sync failures", data);
            }

            if (recentFailures > 10)
            {
                return HealthCheckResult.Degraded($"High sync failure count: {recentFailures}", data: data);
            }

            return HealthCheckResult.Healthy($"Sync operational with {recentFailures} recent failures", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Sync health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for audit log integrity.
/// </summary>
public class AuditHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public AuditHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check recent activity logs (last hour)
            var lastHour = DateTime.UtcNow.AddHours(-1);
            var recentLogs = await Task.Run(() =>
                _context.ActivityLogs
                    .Where(a => a.Date >= lastHour)
                    .Count(), cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["RecentActivityLogs"] = recentLogs
            };

            return HealthCheckResult.Healthy("Audit logging operational", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Audit health check failed", ex);
        }
    }
}
