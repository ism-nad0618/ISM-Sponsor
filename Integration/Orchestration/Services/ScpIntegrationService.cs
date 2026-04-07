using ISMSponsor.Data;
using ISMSponsor.Integration.Adapters;
using ISMSponsor.Integration.Contracts;
using ISMSponsor.Integration.Orchestration.Mappers;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ISMSponsor.Integration.Orchestration.Services;

/// <summary>
/// Handles Student Charging Portal sponsor synchronization.
/// Target: SERVER64.StudentChargingPortal.Sponsors
/// </summary>
public interface IScpIntegrationService
{
    Task<SyncResult> UpsertSponsorAsync(string sponsorId, string eventType);
}

public class ScpIntegrationService : IScpIntegrationService
{
    private readonly AppDbContext _context;
    private readonly IStudentChargingPortalAdapter _scpAdapter;
    private readonly ScpSponsorMapper _mapper;
    private readonly ILogger<ScpIntegrationService> _logger;

    public ScpIntegrationService(
        AppDbContext context,
        IStudentChargingPortalAdapter scpAdapter,
        ILogger<ScpIntegrationService> logger)
    {
        _context = context;
        _scpAdapter = scpAdapter;
        _mapper = new ScpSponsorMapper();
        _logger = logger;
    }

    public async Task<SyncResult> UpsertSponsorAsync(string sponsorId, string eventType)
    {
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            // Load sponsor with contacts for SCP mapping
            var sponsor = await _context.Sponsors
                .Include(s => s.Contacts)
                .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

            if (sponsor == null)
            {
                return CreateFailureResult(sponsorId, eventType, "Sponsor not found", correlationId);
            }

            // Map to SCP format
            var payload = _mapper.MapToScpSponsor(sponsor);

            // Call SCP adapter
            var dto = new StudentChargingPortalSponsorDto
            {
                SponsorId = payload.SponsorId,
                SponsorName = payload.SponsorName,
                StudentChargingPortalId = payload.StudentChargingPortalId,
                IsActive = payload.IsActive
            };

            var result = await _scpAdapter.SyncSponsorAsync(dto, eventType);

            // Update sponsor with SCP ID if returned
            if (result.Success && !string.IsNullOrEmpty(result.ExternalReferenceId))
            {
                sponsor.StudentChargingPortalId = result.ExternalReferenceId;
                sponsor.ModifiedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Log sync
            await LogSyncAttemptAsync(sponsorId, eventType, result, correlationId, payload);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SCP sponsor sync failed for {SponsorId}, event {EventType}", sponsorId, eventType);
            return CreateFailureResult(sponsorId, eventType, ex.Message, correlationId);
        }
    }

    private async Task LogSyncAttemptAsync(string sponsorId, string eventType, SyncResult result, string correlationId, ScpSponsorPayload payload)
    {
        var syncLog = new SyncLog
        {
            EntityType = "Sponsor",
            EntityId = sponsorId,
            TargetSystem = IntegrationTargets.StudentChargingPortal,
            EventType = $"{IntegrationTargetEntities.SCP_Sponsors}:{eventType}",
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
            ErrorCode = "SCP_SYNC_ERR",
            ProcessedAt = DateTime.UtcNow
        };
    }
}
