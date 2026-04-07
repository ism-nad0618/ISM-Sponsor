using ISMSponsor.Data;
using ISMSponsor.Integration.Adapters;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ISMSponsor.Services;

/// <summary>
/// Service for publishing the sponsor master list to PowerSchool's Sponsor_OrgName custom field.
/// This service maintains PowerSchool's popup menu values in sync with the ISM Sponsor Master.
/// </summary>
public interface IPowerSchoolSponsorListService
{
    /// <summary>
    /// Publishes the current list of active sponsor names to PowerSchool's Sponsor_OrgName field.
    /// Triggered by sponsor create, update, activate, deactivate, or merge events.
    /// </summary>
    Task<PublishResult> PublishSponsorOrgListAsync(string triggeringEvent, string? triggeredBySponsorId = null);

    /// <summary>
    /// Queues a sponsor list refresh to be processed asynchronously.
    /// Safe to call without blocking the sponsor save operation.
    /// </summary>
    Task QueueSponsorOrgListRefreshAsync(string triggeringEvent, string? triggeredBySponsorId = null);

    /// <summary>
    /// Gets the current list of active sponsor names formatted for PowerSchool popup menu.
    /// </summary>
    Task<List<string>> GetActiveSponsorNamesAsync();
}

public class PowerSchoolSponsorListService : IPowerSchoolSponsorListService
{
    private readonly AppDbContext _context;
    private readonly IPowerSchoolAdapter _powerSchoolAdapter;
    private readonly ILogger<PowerSchoolSponsorListService> _logger;

    public PowerSchoolSponsorListService(
        AppDbContext context,
        IPowerSchoolAdapter powerSchoolAdapter,
        ILogger<PowerSchoolSponsorListService> logger)
    {
        _context = context;
        _powerSchoolAdapter = powerSchoolAdapter;
        _logger = logger;
    }

    public async Task<PublishResult> PublishSponsorOrgListAsync(string triggeringEvent, string? triggeredBySponsorId = null)
    {
        var correlationId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation(
                "Publishing sponsor list to PowerSchool. Event: {Event}, TriggeredBy: {SponsorId}, CorrelationId: {CorrelationId}",
                triggeringEvent, triggeredBySponsorId ?? "System", correlationId);

            // Get active, approved sponsors
            var sponsorNames = await GetActiveSponsorNamesAsync();

            if (sponsorNames.Count == 0)
            {
                _logger.LogWarning("No active sponsors found for PowerSchool Sponsor_OrgName list");
            }

            // Format payload for PowerSchool popup menu
            var payload = FormatForPowerSchoolPopup(sponsorNames);

            // Publish to PowerSchool via adapter
            var adapterResult = await _powerSchoolAdapter.PublishSponsorOrgListAsync(sponsorNames, correlationId);

            // Log the sync operation
            var syncLog = new SyncLog
            {
                EntityType = "SponsorMaster",
                EntityId = triggeredBySponsorId ?? "ALL",
                TargetSystem = "PowerSchool",
                EventType = $"PublishSponsorOrgList:{triggeringEvent}",
                PayloadVersion = "1.0",
                AttemptedAt = startTime,
                LastSucceededAt = adapterResult.Success ? DateTime.UtcNow : null,
                RetryCount = 0,
                Status = adapterResult.Success ? "Succeeded" : "Failed",
                ErrorMessage = adapterResult.ErrorMessage,
                CorrelationId = correlationId,
                RequestPayload = payload,
                ResponsePayload = adapterResult.Message,
                ExternalReferenceId = adapterResult.ExternalReferenceId,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };

            _context.SyncLogs.Add(syncLog);
            await _context.SaveChangesAsync();

            if (adapterResult.Success)
            {
                _logger.LogInformation(
                    "Successfully published {Count} sponsors to PowerSchool Sponsor_OrgName. CorrelationId: {CorrelationId}",
                    sponsorNames.Count, correlationId);
            }
            else
            {
                _logger.LogError(
                    "Failed to publish sponsor list to PowerSchool. Error: {Error}, CorrelationId: {CorrelationId}",
                    adapterResult.ErrorMessage, correlationId);
            }

            return new PublishResult
            {
                Success = adapterResult.Success,
                SponsorCount = sponsorNames.Count,
                CorrelationId = correlationId,
                ErrorMessage = adapterResult.ErrorMessage,
                SyncLogId = syncLog.SyncLogId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Exception during PowerSchool sponsor list publish. Event: {Event}, CorrelationId: {CorrelationId}",
                triggeringEvent, correlationId);

            // Log the failed sync
            var failedSyncLog = new SyncLog
            {
                EntityType = "SponsorMaster",
                EntityId = triggeredBySponsorId ?? "ALL",
                TargetSystem = "PowerSchool",
                EventType = $"PublishSponsorOrgList:{triggeringEvent}",
                AttemptedAt = startTime,
                RetryCount = 0,
                Status = "Failed",
                ErrorMessage = $"{ex.GetType().Name}: {ex.Message}",
                CorrelationId = correlationId,
                CreatedOn = DateTime.UtcNow
            };

            _context.SyncLogs.Add(failedSyncLog);
            await _context.SaveChangesAsync();

            return new PublishResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                CorrelationId = correlationId,
                SyncLogId = failedSyncLog.SyncLogId
            };
        }
    }

    public async Task QueueSponsorOrgListRefreshAsync(string triggeringEvent, string? triggeredBySponsorId = null)
    {
        // Fire-and-forget async task - does not block sponsor save operation
        // In production, this could be replaced with Azure Service Bus, Hangfire, or similar
        _ = Task.Run(async () =>
        {
            try
            {
                await PublishSponsorOrgListAsync(triggeringEvent, triggeredBySponsorId);
            }
            catch (Exception ex)
            {
                // Already logged in PublishSponsorOrgListAsync
                _logger.LogError(ex, "Background sponsor list publish failed for event {Event}", triggeringEvent);
            }
        });

        await Task.CompletedTask;
    }

    public async Task<List<string>> GetActiveSponsorNamesAsync()
    {
        // Get active sponsors with Approved status (for Phase 2 approval workflow compatibility)
        // Exclude merged sponsors and inactive sponsors
        var sponsors = await _context.Sponsors
            .Where(s => s.IsActive && !s.IsMerged)
            .Where(s => s.ApprovalStatus == null || s.ApprovalStatus == "Approved") // Include legacy nulls + approved
            .OrderBy(s => s.SponsorName)
            .Select(s => s.SponsorName)
            .ToListAsync();

        // Sanitize sponsor names for PowerSchool popup menu
        var sanitized = sponsors
            .Select(name => SanitizeSponsorName(name))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        return sanitized;
    }

    /// <summary>
    /// Formats sponsor names for PowerSchool custom field popup menu.
    /// PowerSchool expects newline-separated values for popup/radio button data.
    /// </summary>
    private string FormatForPowerSchoolPopup(List<string> sponsorNames)
    {
        if (sponsorNames.Count == 0)
        {
            return string.Empty;
        }

        // PowerSchool popup format: one value per line
        var sb = new StringBuilder();
        foreach (var name in sponsorNames)
        {
            sb.AppendLine(name);
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Sanitizes sponsor name for safe use in PowerSchool popup menu.
    /// Removes or escapes characters that could break the popup format.
    /// </summary>
    private string SanitizeSponsorName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Trim and normalize whitespace
        name = name.Trim();
        name = System.Text.RegularExpressions.Regex.Replace(name, @"\s+", " ");

        // Remove newlines and carriage returns (would break popup format)
        name = name.Replace("\n", " ").Replace("\r", " ");

        // Remove leading/trailing pipes and semicolons (common delimiters)
        name = name.Trim('|', ';');

        return name;
    }
}

/// <summary>
/// Result of a PowerSchool sponsor list publish operation.
/// </summary>
public class PublishResult
{
    public bool Success { get; set; }
    public int SponsorCount { get; set; }
    public string? CorrelationId { get; set; }
    public string? ErrorMessage { get; set; }
    public int? SyncLogId { get; set; }
}
