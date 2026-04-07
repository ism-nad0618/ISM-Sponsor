using ISMSponsor.Data;
using ISMSponsor.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services;

public class MonitoringService
{
    private readonly AppDbContext _context;

    public MonitoringService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OperationalMonitoringViewModel> GetOperationalStatusAsync()
    {
        var model = new OperationalMonitoringViewModel
        {
            ApplicationHealth = await GetHealthStatusAsync(),
            RecentSyncAttempts = await GetRecentSyncAttemptsAsync(),
            RecentSyncFailures = await GetRecentSyncFailuresAsync(),
            RecentEvaluations = await GetRecentCoverageEvaluationsAsync(),
            EvaluationFailures = await GetCoverageEvaluationFailuresAsync(),
            AuthorizationFailures = await GetAuthorizationFailuresAsync(),
            RecentRequests = await GetRecentSponsorRequestsAsync(),
            RecentLogChanges = await GetRecentLogStatusChangesAsync(),
            ConsistencyWarnings = await GetDataConsistencyWarningsAsync()
        };

        return model;
    }

    private async Task<HealthStatus> GetHealthStatusAsync()
    {
        var health = new HealthStatus
        {
            LastChecked = DateTime.UtcNow
        };

        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync();
            health.DatabaseHealthy = true;

            // Check configuration (simplified)
            health.ConfigurationValid = true;

            // Check sync health (no failures in last 15 minutes)
            var recentSyncFailures = await _context.Set<Models.Domain.SyncLog>()
                .CountAsync(s => s.Status == "Failed" && s.AttemptedAt >= DateTime.UtcNow.AddMinutes(-15));
            health.SyncHealthy = recentSyncFailures == 0;

            // Check audit health
            var recentAuditEntries = await _context.ActivityLogs
                .CountAsync();
            health.AuditHealthy = recentAuditEntries >= 0; // Can be 0 if no activity

            // Overall status
            if (health.DatabaseHealthy && health.ConfigurationValid && health.SyncHealthy && health.AuditHealthy)
                health.Status = "Healthy";
            else if (!health.DatabaseHealthy)
                health.Status = "Unhealthy";
            else
                health.Status = "Degraded";
        }
        catch
        {
            health.Status = "Unhealthy";
            health.DatabaseHealthy = false;
        }

        return health;
    }

    private async Task<List<RecentSyncAttempt>> GetRecentSyncAttemptsAsync()
    {
        return await _context.Set<Models.Domain.SyncLog>()
            .OrderByDescending(s => s.AttemptedAt)
            .Take(20)
            .Select(s => new RecentSyncAttempt
            {
                SyncLogId = s.SyncLogId,
                EntityType = s.EntityType,
                EntityId = s.EntityId,
                TargetSystem = s.TargetSystem,
                Status = s.Status,
                AttemptedAt = s.AttemptedAt,
                RetryCount = s.RetryCount
            })
            .ToListAsync();
    }

    private async Task<List<RecentSyncFailure>> GetRecentSyncFailuresAsync()
    {
        var failures = await _context.Set<Models.Domain.SyncLog>()
            .Where(s => s.Status == "Failed")
            .OrderByDescending(s => s.AttemptedAt)
            .Take(20)
            .ToListAsync();

        return failures.Select(s => new RecentSyncFailure
        {
            SyncLogId = s.SyncLogId,
            EntityType = s.EntityType,
            TargetSystem = s.TargetSystem,
            ErrorMessage = s.ResponsePayload ?? "Unknown error",
            FailedAt = s.AttemptedAt,
            RetryCount = s.RetryCount,
            CanRetry = s.RetryCount < 3
        }).ToList();
    }

    private async Task<List<RecentCoverageEvaluation>> GetRecentCoverageEvaluationsAsync()
    {
        var evaluations = await _context.Set<Models.Domain.CoverageEvaluationAudit>()
            .OrderByDescending(a => a.EvaluatedOn)
            .Take(20)
            .ToListAsync();

        return evaluations.Select(a => new RecentCoverageEvaluation
        {
            AuditId = a.AuditId,
            StudentId = a.StudentId,
            ItemCode = a.ItemId ?? "",
            Decision = a.Decision,
            EvaluatedAt = a.EvaluatedOn
        }).ToList();
    }

    private async Task<List<CoverageEvaluationFailure>> GetCoverageEvaluationFailuresAsync()
    {
        // In a real implementation, you'd track coverage evaluation failures
        // For now, return empty list
        return new List<CoverageEvaluationFailure>();
    }

    private async Task<List<AuthorizationFailureEvent>> GetAuthorizationFailuresAsync()
    {
        // Simplified - ActivityLog doesn't track authorization failures in detail
        return new List<AuthorizationFailureEvent>();
    }

    private async Task<List<RecentSponsorRequestEvent>> GetRecentSponsorRequestsAsync()
    {
        var requests = await _context.ChangeRequests
            .OrderByDescending(cr => cr.RequestedOn)
            .Take(20)
            .ToListAsync();

        return requests.Select(cr => new RecentSponsorRequestEvent
        {
            RequestId = cr.ChangeRequestId,
            SponsorId = cr.SponsorId,
            RequestType = cr.Field,
            Status = cr.Status,
            Action = cr.Field,
            OccurredAt = cr.RequestedOn
        }).ToList();
    }

    private async Task<List<RecentLogStatusChange>> GetRecentLogStatusChangesAsync()
    {
        // Get recent LoG status changes - simplified
        var recentLogs = await _context.LogCoverages
            .Where(l => l.ModifiedOn.HasValue)
            .OrderByDescending(l => l.ModifiedOn)
            .Take(20)
            .ToListAsync();

        return recentLogs.Select(l => new RecentLogStatusChange
        {
            LogCoverageId = l.LogId,
            SponsorId = l.SponsorId,
            OldStatus = "",
            NewStatus = l.LogStatus,
            ChangedAt = l.ModifiedOn ?? l.CreatedOn,
            ChangedBy = l.ModifiedByUserId ?? "System"
        }).ToList();
    }

    private async Task<List<DataConsistencyWarning>> GetDataConsistencyWarningsAsync()
    {
        var warnings = new List<DataConsistencyWarning>();

        // Check for sponsors without students
        var sponsorsWithoutStudents = await _context.Sponsors
            .Include(s => s.Students)
            .Where(s => s.IsActive && (s.Students == null || s.Students.Count == 0))
            .CountAsync();

        if (sponsorsWithoutStudents > 0)
        {
            warnings.Add(new DataConsistencyWarning
            {
                WarningType = "OrphanedSponsors",
                EntityType = "Sponsor",
                EntityId = "",
                Description = $"{sponsorsWithoutStudents} active sponsors have no students",
                Severity = "Low",
                DetectedAt = DateTime.UtcNow
            });
        }

        // Check for sponsors without active LoGs
        var sponsorsWithoutLogs = await _context.Sponsors
            .Include(s => s.LettersOfGuarantee)
            .Where(s => s.IsActive && (s.LettersOfGuarantee == null || !s.LettersOfGuarantee.Any(l => l.IsActive)))
            .CountAsync();

        if (sponsorsWithoutLogs > 0)
        {
            warnings.Add(new DataConsistencyWarning
            {
                WarningType = "MissingLoGs",
                EntityType = "Sponsor",
                EntityId = "",
                Description = $"{sponsorsWithoutLogs} active sponsors have no active LoGs",
                Severity = "Medium",
                DetectedAt = DateTime.UtcNow
            });
        }

        // Check for stale pending requests
        var stalePendingRequests = await _context.ChangeRequests
            .Where(cr => cr.Status == "Pending" && cr.RequestedOn < DateTime.UtcNow.AddDays(-7))
            .CountAsync();

        if (stalePendingRequests > 0)
        {
            warnings.Add(new DataConsistencyWarning
            {
                WarningType = "StalePendingRequests",
                EntityType = "ChangeRequest",
                EntityId = "",
                Description = $"{stalePendingRequests} pending requests older than 7 days",
                Severity = "High",
                DetectedAt = DateTime.UtcNow
            });
        }

        return warnings;
    }
}
