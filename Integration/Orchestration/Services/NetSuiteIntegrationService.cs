using ISMSponsor.Data;
using ISMSponsor.Integration.Adapters;
using ISMSponsor.Integration.Contracts;
using ISMSponsor.Integration.Orchestration.Mappers;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ISMSponsor.Integration.Orchestration.Services;

/// <summary>
/// Handles NetSuite-specific sponsor synchronization.
/// Target: NetSuite Sponsors List custom record
/// </summary>
public interface INetSuiteIntegrationService
{
    Task<SyncResult> UpsertSponsorAsync(string sponsorId, string eventType);
}

public class NetSuiteIntegrationService : INetSuiteIntegrationService
{
    private readonly AppDbContext _context;
    private readonly INetSuiteAdapter _netSuiteAdapter;
    private readonly NetSuiteSponsorMapper _mapper;
    private readonly ILogger<NetSuiteIntegrationService> _logger;

    public NetSuiteIntegrationService(
        AppDbContext context,
        INetSuiteAdapter netSuiteAdapter,
        ILogger<NetSuiteIntegrationService> logger)
    {
        _context = context;
        _netSuiteAdapter = netSuiteAdapter;
        _mapper = new NetSuiteSponsorMapper();
        _logger = logger;
    }

    public async Task<SyncResult> UpsertSponsorAsync(string sponsorId, string eventType)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            // Load sponsor with addresses/contacts
            var sponsor = await _context.Sponsors
                .Include(s => s.Addresses)
                .Include(s => s.Contacts)
                .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

            if (sponsor == null)
            {
                return CreateFailureResult(sponsorId, eventType, "Sponsor not found", correlationId);
            }

            // Map to NetSuite format
            var payload = _mapper.MapToNetSuite(sponsor);

            // Call NetSuite adapter (upsert to Sponsors List)
            var dto = new NetSuiteSponsorDto
            {
                SponsorId = payload.SponsorCode,
                SponsorName = payload.SponsorName,
                LegalName = payload.LegalName,
                Tin = payload.TaxId,
                NetSuiteId = payload.NetSuiteId,
                BillingAddress = payload.Address,
                IsActive = payload.IsActive
            };

            var result = await _netSuiteAdapter.SyncSponsorAsync(dto, eventType);

            // Update sponsor with NetSuite ID if returned
            if (result.Success && !string.IsNullOrEmpty(result.ExternalReferenceId))
            {
                sponsor.NetSuiteId = result.ExternalReferenceId;
                sponsor.ModifiedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Log sync
            await LogSyncAttemptAsync(sponsorId, eventType, result, correlationId, payload);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NetSuite sponsor sync failed for {SponsorId}, event {EventType}", sponsorId, eventType);
            return CreateFailureResult(sponsorId, eventType, ex.Message, correlationId);
        }
    }

    private async Task LogSyncAttemptAsync(string sponsorId, string eventType, SyncResult result, string correlationId, NetSuiteSponsorPayload payload)
    {
        var syncLog = new SyncLog
        {
            EntityType = "Sponsor",
            EntityId = sponsorId,
            TargetSystem = IntegrationTargets.NetSuite,
            EventType = $"{IntegrationTargetEntities.NetSuite_SponsorsList}:{eventType}",
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

    private SyncResult CreateFailureResult(string sponsorId, string eventType, string errorMessage, string correlationId)
    {
        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorMessage = errorMessage,
            ErrorCode = "NS_SYNC_ERR",
            ProcessedAt = DateTime.UtcNow
        };
    }
}
