using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using ISMSponsor.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services;

/// <summary>
/// Service for retrieving and filtering audit logs.
/// Audit logs are read-only and provide tamper-evident records of system actions.
/// </summary>
public class AuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves audit logs with optional filtering.
    /// </summary>
    public async Task<List<ActivityLog>> GetAuditLogsAsync(AuditFilterViewModel? filter = null)
    {
        var query = _context.ActivityLogs.AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Module))
            {
                query = query.Where(log => log.Item == filter.Module);
            }

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                query = query.Where(log => log.Details.Contains(filter.Action));
            }

            if (!string.IsNullOrWhiteSpace(filter.Actor))
            {
                query = query.Where(log => log.UserDisplay.Contains(filter.Actor));
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(log => log.Date >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                var toDate = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(log => log.Date <= toDate);
            }

            if (!string.IsNullOrWhiteSpace(filter.SponsorId))
            {
                query = query.Where(log => log.Details.Contains(filter.SponsorId));
            }

            if (!string.IsNullOrWhiteSpace(filter.ReasonCode))
            {
                query = query.Where(log => log.Details.Contains(filter.ReasonCode));
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(log =>
                    log.Details.Contains(filter.SearchTerm) ||
                    log.Item.Contains(filter.SearchTerm) ||
                    log.UserDisplay.Contains(filter.SearchTerm));
            }
        }

        return await query
            .OrderByDescending(log => log.Date)
            .Take(500) // Limit to most recent 500 records
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific audit log by ID.
    /// </summary>
    public async Task<ActivityLog?> GetAuditLogByIdAsync(int id)
    {
        return await _context.ActivityLogs
            .FirstOrDefaultAsync(log => log.ActivityLogId == id);
    }

    /// <summary>
    /// Gets recent audit logs for dashboard summary.
    /// </summary>
    public async Task<List<ActivityLog>> GetRecentAuditLogsAsync(int count = 10)
    {
        return await _context.ActivityLogs
            .OrderByDescending(log => log.Date)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Gets recent coverage evaluation audit logs.
    /// </summary>
    public async Task<List<CoverageEvaluationAudit>> GetRecentCoverageEvaluationsAsync(int count = 10)
    {
        return await _context.CoverageEvaluationAudits
            .OrderByDescending(audit => audit.EvaluatedOn)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific coverage evaluation audit by ID.
    /// </summary>
    public async Task<CoverageEvaluationAudit?> GetCoverageEvaluationByIdAsync(int id)
    {
        return await _context.CoverageEvaluationAudits
            .FirstOrDefaultAsync(audit => audit.AuditId == id);
    }

    /// <summary>
    /// Gets recent failed operations for monitoring.
    /// </summary>
    public async Task<List<ActivityLog>> GetRecentFailuresAsync(int count = 10)
    {
        return await _context.ActivityLogs
            .Where(log => log.Details.Contains("error") || log.Details.Contains("failed"))
            .OrderByDescending(log => log.Date)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Gets available modules for filtering dropdown.
    /// </summary>
    public async Task<List<string>> GetAvailableModulesAsync()
    {
        return await _context.ActivityLogs
            .Select(log => log.Item)
            .Distinct()
            .OrderBy(item => item)
            .ToListAsync();
    }

    /// <summary>
    /// Gets audit statistics for dashboard.
    /// </summary>
    public async Task<Dictionary<string, int>> GetAuditStatisticsAsync(DateTime? since = null)
    {
        var sinceDate = since ?? DateTime.UtcNow.AddDays(-7);

        var stats = new Dictionary<string, int>();

        stats["TotalActions"] = await _context.ActivityLogs
            .Where(log => log.Date >= sinceDate)
            .CountAsync();

        stats["CoverageEvaluations"] = await _context.CoverageEvaluationAudits
            .Where(audit => audit.EvaluatedOn >= sinceDate)
            .CountAsync();

        stats["SponsorRequests"] = await _context.ActivityLogs
            .Where(log => log.Item == "SponsorRequest" && log.Date >= sinceDate)
            .CountAsync();

        stats["LoGActions"] = await _context.ActivityLogs
            .Where(log => log.Item == "LetterOfGuarantee" && log.Date >= sinceDate)
            .CountAsync();

        return stats;
    }
}
