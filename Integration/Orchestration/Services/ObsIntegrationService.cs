using ISMSponsor.Data;
using ISMSponsor.Integration.Adapters;
using ISMSponsor.Integration.Contracts;
using ISMSponsor.Integration.Orchestration.Mappers;
using ISMSponsor.Models.Domain;
using ISMSponsor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ISMSponsor.Integration.Orchestration.Services;

/// <summary>
/// Handles Online Billing System sponsor synchronization.
/// Targets: FINDB01.CompanySponsors AND FINDB01.CompanySponsorAccount
/// </summary>
public interface IObsIntegrationService
{
    Task<SyncResult> UpsertCompanySponsorAsync(string sponsorId, string eventType);
    Task<SyncResult> UpsertCompanySponsorAccountAsync(string sponsorId, string eventType, string? username = null);
    Task<List<SyncResult>> UpsertBothAsync(string sponsorId, string eventType, string? username = null);
}

public class ObsIntegrationService : IObsIntegrationService
{
    private readonly AppDbContext _context;
    private readonly IOnlineBillingSystemAdapter _obsAdapter;
    private readonly ObsCompanySponsorMapper _sponsorMapper;
    private readonly ObsCompanySponsorAccountMapper _accountMapper;
    private readonly ILogger<ObsIntegrationService> _logger;

    public ObsIntegrationService(
        AppDbContext context,
        IOnlineBillingSystemAdapter obsAdapter,
        UserManager<ApplicationUser> userManager,
        ILogger<ObsIntegrationService> logger)
    {
        _context = context;
        _obsAdapter = obsAdapter;
        _sponsorMapper = new ObsCompanySponsorMapper();
        _accountMapper = new ObsCompanySponsorAccountMapper(userManager);
        _logger = logger;
    }

    public async Task<List<SyncResult>> UpsertBothAsync(string sponsorId, string eventType, string? username = null)
    {
        var results = new List<SyncResult>();
        
        // Sync CompanySponsors first (master data)
        var sponsorResult = await UpsertCompanySponsorAsync(sponsorId, eventType);
        results.Add(sponsorResult);
        
        // Then sync CompanySponsorAccount (account/credentials)
        var accountResult = await UpsertCompanySponsorAccountAsync(sponsorId, eventType, username);
        results.Add(accountResult);
        
        return results;
    }

    public async Task<SyncResult> UpsertCompanySponsorAsync(string sponsorId, string eventType)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            var sponsor = await _context.Sponsors
                .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

            if (sponsor == null)
            {
                return CreateFailureResult(sponsorId, eventType, "CompanySponsors", "Sponsor not found", correlationId);
            }

            // Map to OBS CompanySponsors format
            var payload = _sponsorMapper.MapToObsCompanySponsor(sponsor);

            // Call OBS adapter
            var dto = new OnlineBillingSponsorDto
            {
                SponsorId = payload.SponsorId,
                SponsorName = payload.SponsorName,
                OnlineBillingSystemId = payload.OnlineBillingSystemId,
                StatementEmail = sponsor.Contacts?.FirstOrDefault(c => c.IsActive)?.Email ?? string.Empty,
                IsActive = payload.IsActive
            };

            var result = await _obsAdapter.SyncSponsorAsync(dto, eventType);

            // Update sponsor with OBS ID if returned
            if (result.Success && !string.IsNullOrEmpty(result.ExternalReferenceId))
            {
                sponsor.OnlineBillingSystemId = result.ExternalReferenceId;
                sponsor.ModifiedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Log sync
            await LogSyncAttemptAsync(sponsorId, eventType, "CompanySponsors", result, correlationId, payload);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OBS CompanySponsors sync failed for {SponsorId}", sponsorId);
            return CreateFailureResult(sponsorId, eventType, "CompanySponsors", ex.Message, correlationId);
        }
    }

    public async Task<SyncResult> UpsertCompanySponsorAccountAsync(string sponsorId, string eventType, string? username = null)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            var sponsor = await _context.Sponsors
                .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

            if (sponsor == null)
            {
                return CreateFailureResult(sponsorId, eventType, "CompanySponsorAccount", "Sponsor not found", correlationId);
            }

            // Map to OBS CompanySponsorAccount format
            var payload = await _accountMapper.MapToObsCompanySponsorAccountAsync(sponsor, username);

            // Call OBS adapter for account sync
            // NOTE: Password handling is abstracted - actual password sync logic in adapter
            var result = await _obsAdapter.SyncSponsorAccountAsync(payload);

            // Log sync
            await LogSyncAttemptAsync(sponsorId, eventType, "CompanySponsorAccount", result, correlationId, payload);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OBS CompanySponsorAccount sync failed for {SponsorId}", sponsorId);
            return CreateFailureResult(sponsorId, eventType, "CompanySponsorAccount", ex.Message, correlationId);
        }
    }

    private async Task LogSyncAttemptAsync(string sponsorId, string eventType, string targetEntity, SyncResult result, string correlationId, object payload)
    {
        var syncLog = new SyncLog
        {
            EntityType = "Sponsor",
            EntityId = sponsorId,
            TargetSystem = IntegrationTargets.OnlineBillingSystem,
            EventType = $"{targetEntity}:{eventType}",
            Status = result.Status,
            AttemptedAt = DateTime.UtcNow,
            LastSucceededAt = result.Success ? DateTime.UtcNow : null,
            RetryCount = 0,
            ErrorMessage = result.ErrorMessage,
            CorrelationId = correlationId,
            ExternalReferenceId = result.ExternalReferenceId,
            RequestPayload = System.Text.Json.JsonSerializer.Serialize(payload),
            ResponsePayload = result.Message,
            CreatedOn = DateTime.UtcNow
        };

        _context.SyncLogs.Add(syncLog);
        await _context.SaveChangesAsync();
    }

    private SyncResult CreateFailureResult(string sponsorId, string eventType, string targetEntity, string errorMessage, string correlationId)
    {
        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorMessage = $"[{targetEntity}] {errorMessage}",
            ErrorCode = "OBS_SYNC_ERR",
            ProcessedAt = DateTime.UtcNow
        };
    }
}
