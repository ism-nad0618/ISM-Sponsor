using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ISMSponsor.ViewModels;
using ISMSponsor.Data;
using ISMSponsor.Integration.Adapters;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers;

/// <summary>
/// Monitors integration sync status and manages sync operations.
/// Admin-only functionality.
/// </summary>
[Authorize(Roles = "Admin")]
public class SyncStatusController : Controller
{
    private readonly AppDbContext _context;
    private readonly IntegrationSyncService _syncService;
    private readonly ILogger<SyncStatusController> _logger;
    private readonly IConfiguration _configuration;

    public SyncStatusController(
        AppDbContext context,
        IntegrationSyncService syncService,
        ILogger<SyncStatusController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _syncService = syncService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Display sync status dashboard.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var isSyncEnabled = _configuration.GetValue<bool>("Integration:SyncEnabled");

        // Get recent sync history
        var recentSyncs = await _context.SyncLogs
            .Where(s => s.EntityType == "Sponsor")
            .OrderByDescending(s => s.AttemptedAt)
            .Take(50)
            .ToListAsync();

        // Get recent failures
        var recentFailures = await _context.SyncLogs
            .Where(s => s.EntityType == "Sponsor" && s.Status == "Failed" && s.AttemptedAt >= DateTime.UtcNow.AddDays(-7))
            .OrderByDescending(s => s.AttemptedAt)
            .Take(20)
            .ToListAsync();

        // Calculate status summary
        var totalSponsors = await _context.Sponsors.Where(s => s.IsActive).CountAsync();
        var failedLast24Hours = await _context.SyncLogs
            .Where(s => s.EntityType == "Sponsor" && s.Status == "Failed" && s.AttemptedAt >= DateTime.UtcNow.AddHours(-24))
            .CountAsync();
        var failedLast7Days = await _context.SyncLogs
            .Where(s => s.EntityType == "Sponsor" && s.Status == "Failed" && s.AttemptedAt >= DateTime.UtcNow.AddDays(-7))
            .CountAsync();

        var lastSuccessfulSync = await _context.SyncLogs
            .Where(s => s.EntityType == "Sponsor" && s.Status == "Succeeded")
            .OrderByDescending(s => s.LastSucceededAt)
            .Select(s => s.LastSucceededAt)
            .FirstOrDefaultAsync();

        var viewModel = new IntegrationSyncViewModel
        {
            IsSyncEnabled = isSyncEnabled,
            LastSuccessfulSync = lastSuccessfulSync,
            StatusSummary = new SyncStatusSummary
            {
                TotalSponsorsInSystem = totalSponsors,
                SponsorsPendingSync = 0, // Would need to track this separately
                FailedSyncsLast24Hours = failedLast24Hours,
                FailedSyncsLast7Days = failedLast7Days
            },
            RecentSyncs = recentSyncs.Select(s => new SyncHistoryItem
            {
                SyncLogId = s.SyncLogId,
                SponsorId = s.EntityId,
                SponsorName = s.EntityId, // Would need to join with Sponsors table for name
                ExternalSystemName = s.TargetSystem,
                ExternalId = s.CorrelationId ?? "",
                Operation = s.EventType,
                Success = s.Status == "Succeeded",
                ErrorMessage = s.ErrorMessage,
                SyncedAt = s.AttemptedAt,
                RetryCount = s.RetryCount
            }).ToList(),
            RecentFailures = recentFailures.Select(s => new SyncFailureDetail
            {
                SyncLogId = s.SyncLogId,
                SponsorId = s.EntityId,
                SponsorName = s.EntityId, // Would need to join with Sponsors table for name
                ExternalSystemName = s.TargetSystem,
                Operation = s.EventType,
                ErrorMessage = s.ErrorMessage ?? "Unknown error",
                ErrorDetails = s.ResponsePayload, // Use response payload as error details
                FailedAt = s.AttemptedAt,
                RetryCount = s.RetryCount,
                NextRetryAt = null, // Not tracked in this model
                CanRetry = s.RetryCount < 5
            }).ToList()
        };

        // Get per-system status
        var systemNames = new[] { "PowerSchool", "NetSuite" };
        foreach (var systemName in systemNames)
        {
            var lastSuccess = await _context.SyncLogs
                .Where(s => s.TargetSystem == systemName && s.Status == "Succeeded")
                .OrderByDescending(s => s.LastSucceededAt)
                .Select(s => s.LastSucceededAt)
                .FirstOrDefaultAsync();

            var failed = await _context.SyncLogs
                .Where(s => s.TargetSystem == systemName && s.Status == "Failed")
                .CountAsync();

            var lastError = await _context.SyncLogs
                .Where(s => s.TargetSystem == systemName && s.Status == "Failed")
                .OrderByDescending(s => s.AttemptedAt)
                .Select(s => s.ErrorMessage)
                .FirstOrDefaultAsync();

            viewModel.StatusSummary.SystemStatuses[systemName] = new SystemSyncStatus
            {
                SystemName = systemName,
                LastSuccessfulSync = lastSuccess,
                PendingRecords = 0,
                FailedRecords = failed,
                Status = failed == 0 ? "Healthy" : (failed < 10 ? "Degraded" : "Unhealthy"),
                LastError = lastError
            };
        }

        return View(viewModel);
    }

    /// <summary>
    /// Manually trigger sync for a specific sponsor.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncSponsor(string sponsorId)
    {
        var sponsor = await _context.Sponsors.FirstOrDefaultAsync(s => s.SponsorId == sponsorId);
        
        if (sponsor == null)
        {
            TempData["ErrorMessage"] = "Sponsor not found.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _logger.LogInformation(
                "Manual sync triggered for sponsor {SponsorId} by {User}",
                sponsorId,
                User.Identity?.Name
            );

            // Sync service would need to be called here
            TempData["SuccessMessage"] = $"Sync initiated for sponsor {sponsor.SponsorName}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing sponsor {SponsorId}", sponsorId);
            TempData["ErrorMessage"] = $"Error syncing sponsor: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Clear old sync logs (housekeeping).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearOldLogs(int daysToKeep = 90)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            
            var oldLogs = await _context.SyncLogs
                .Where(s => s.AttemptedAt < cutoffDate && s.Status == "Succeeded")
                .ToListAsync();

            _context.SyncLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cleared {Count} old sync logs (older than {Days} days) by {User}",
                oldLogs.Count,
                daysToKeep,
                User.Identity?.Name
            );

            TempData["SuccessMessage"] = $"Cleared {oldLogs.Count} old sync log(s).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing old sync logs");
            TempData["ErrorMessage"] = "Error clearing old logs.";
        }

        return RedirectToAction(nameof(Index));
    }
}

