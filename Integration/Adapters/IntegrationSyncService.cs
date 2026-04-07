using ISMSponsor.Data;
using ISMSponsor.Integration.Contracts;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ISMSponsor.Integration.Adapters;

/// <summary>
/// Coordinates synchronization operations with external systems.
/// Manages sync logs, orchestrates adapters, and handles retries.
/// </summary>
public class IntegrationSyncService : IIntegrationSyncService
{
    private readonly AppDbContext _context;
    private readonly IPowerSchoolAdapter _powerSchoolAdapter;
    private readonly IStudentChargingPortalAdapter _scpAdapter;
    private readonly INetSuiteAdapter _netSuiteAdapter;
    private readonly IOnlineBillingSystemAdapter _obsAdapter;

    public IntegrationSyncService(
        AppDbContext context,
        IPowerSchoolAdapter powerSchoolAdapter,
        IStudentChargingPortalAdapter scpAdapter,
        INetSuiteAdapter netSuiteAdapter,
        IOnlineBillingSystemAdapter obsAdapter)
    {
        _context = context;
        _powerSchoolAdapter = powerSchoolAdapter;
        _scpAdapter = scpAdapter;
        _netSuiteAdapter = netSuiteAdapter;
        _obsAdapter = obsAdapter;
    }

    public async Task<List<SyncResult>> SyncSponsorCreateAsync(string sponsorId)
    {
        var sponsor = await _context.Sponsors
            .Include(s => s.Students)
            .Include(s => s.Contacts)
            .Include(s => s.Addresses)
            .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

        if (sponsor == null)
        {
            return new List<SyncResult>
            {
                new SyncResult
                {
                    Success = false,
                    Status = "Failed",
                    ErrorMessage = $"Sponsor {sponsorId} not found"
                }
            };
        }

        var results = new List<SyncResult>();
        var correlationId = Guid.NewGuid().ToString();

        // Sync to PowerSchool
        var psResult = await SyncToPowerSchoolAsync(sponsor, "Create", correlationId);
        results.Add(psResult);

        // Sync to Student Charging Portal
        var scpResult = await SyncToStudentChargingPortalAsync(sponsor, "Create", correlationId);
        results.Add(scpResult);

        // Sync to NetSuite
        var nsResult = await SyncToNetSuiteAsync(sponsor, "Create", correlationId);
        results.Add(nsResult);

        // Sync to Online Billing System
        var obsResult = await SyncToOnlineBillingSystemAsync(sponsor, "Create", correlationId);
        results.Add(obsResult);

        return results;
    }

    public async Task<List<SyncResult>> SyncSponsorUpdateAsync(string sponsorId)
    {
        var sponsor = await _context.Sponsors
            .Include(s => s.Students)
            .Include(s => s.Contacts)
            .Include(s => s.Addresses)
            .FirstOrDefaultAsync(s => s.SponsorId == sponsorId);

        if (sponsor == null || sponsor.IsMerged)
        {
            return new List<SyncResult>
            {
                new SyncResult
                {
                    Success = false,
                    Status = "Skipped",
                    Message = $"Sponsor {sponsorId} not found or is merged"
                }
            };
        }

        var results = new List<SyncResult>();
        var correlationId = Guid.NewGuid().ToString();

        results.Add(await SyncToPowerSchoolAsync(sponsor, "Update", correlationId));
        results.Add(await SyncToStudentChargingPortalAsync(sponsor, "Update", correlationId));
        results.Add(await SyncToNetSuiteAsync(sponsor, "Update", correlationId));
        results.Add(await SyncToOnlineBillingSystemAsync(sponsor, "Update", correlationId));

        return results;
    }

    public async Task<List<SyncResult>> SyncSponsorMergeAsync(string survivingSponsorId, string mergedSponsorId)
    {
        var survivor = await _context.Sponsors
            .Include(s => s.Students)
            .FirstOrDefaultAsync(s => s.SponsorId == survivingSponsorId);

        if (survivor == null)
        {
            return new List<SyncResult>
            {
                new SyncResult
                {
                    Success = false,
                    Status = "Failed",
                    ErrorMessage = $"Surviving sponsor {survivingSponsorId} not found"
                }
            };
        }

        var results = new List<SyncResult>();
        var correlationId = Guid.NewGuid().ToString();

        // Sync merge event to all systems
        results.Add(await SyncToPowerSchoolAsync(survivor, "Merge", correlationId));
        results.Add(await SyncToStudentChargingPortalAsync(survivor, "Merge", correlationId));
        results.Add(await SyncToNetSuiteAsync(survivor, "Merge", correlationId));
        results.Add(await SyncToOnlineBillingSystemAsync(survivor, "Merge", correlationId));

        return results;
    }

    public async Task<List<SyncResult>> SyncLoGActivationAsync(int logId)
    {
        var log = await _context.LogCoverages
            .Include(l => l.Sponsor)
            .Include(l => l.Student)
            .FirstOrDefaultAsync(l => l.LogId == logId);

        if (log == null)
        {
            return new List<SyncResult>
            {
                new SyncResult
                {
                    Success = false,
                    Status = "Failed",
                    ErrorMessage = $"LoG {logId} not found"
                }
            };
        }

        var results = new List<SyncResult>();
        var correlationId = Guid.NewGuid().ToString();

        // For demo: just log the LoG activation event
        var syncLog = new SyncLog
        {
            EntityType = "LoG",
            EntityId = logId.ToString(),
            TargetSystem = "StudentChargingPortal",
            EventType = log.IsActive ? "Activate" : "Deactivate",
            Status = "Succeeded",
            CorrelationId = correlationId,
            AttemptedAt = DateTime.UtcNow,
            LastSucceededAt = DateTime.UtcNow
        };

        _context.SyncLogs.Add(syncLog);
        await _context.SaveChangesAsync();

        results.Add(new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"LoG {logId} {(log.IsActive ? "activation" : "deactivation")} logged"
        });

        return results;
    }

    public async Task<SyncResult> RetrySyncAsync(int syncLogId)
    {
        var syncLog = await _context.SyncLogs.FindAsync(syncLogId);

        if (syncLog == null)
        {
            return new SyncResult
            {
                Success = false,
                Status = "Failed",
                ErrorMessage = $"Sync log {syncLogId} not found"
            };
        }

        if (syncLog.Status == "Succeeded")
        {
            return new SyncResult
            {
                Success = false,
                Status = "Skipped",
                Message = "Sync already succeeded, retry not needed"
            };
        }

        // Increment retry count
        syncLog.RetryCount++;
        syncLog.ModifiedOn = DateTime.UtcNow;
        syncLog.AttemptedAt = DateTime.UtcNow;
        syncLog.Status = "InProgress";

        await _context.SaveChangesAsync();

        // Simulate retry (in real implementation, would call actual adapter)
        await Task.Delay(100);

        // 70% success rate on retry
        var random = new Random();
        if (random.Next(100) < 70)
        {
            syncLog.Status = "Succeeded";
            syncLog.LastSucceededAt = DateTime.UtcNow;
            syncLog.ErrorMessage = null;

            await _context.SaveChangesAsync();

            return new SyncResult
            {
                Success = true,
                Status = "Succeeded",
                Message = $"Retry succeeded for {syncLog.TargetSystem} {syncLog.EventType}"
            };
        }

        syncLog.Status = "Failed";
        syncLog.ErrorMessage = $"Retry {syncLog.RetryCount} failed";

        await _context.SaveChangesAsync();

        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorMessage = $"Retry {syncLog.RetryCount} failed for {syncLog.TargetSystem}"
        };
    }

    // Helper methods for each system
    private async Task<SyncResult> SyncToPowerSchoolAsync(Sponsor sponsor, string eventType, string correlationId)
    {
        var dto = new PowerSchoolSponsorDto
        {
            SponsorId = sponsor.SponsorId,
            SponsorName = sponsor.SponsorName,
            LegalName = sponsor.LegalName,
            PowerSchoolId = sponsor.PowerSchoolId,
            StudentIds = sponsor.Students?.Select(s => s.StudentId).ToList() ?? new(),
            ContactEmail = sponsor.Contacts?.FirstOrDefault()?.Email,
            ContactPhone = sponsor.Contacts?.FirstOrDefault()?.Phone,
            IsActive = sponsor.IsActive
        };

        var result = await _powerSchoolAdapter.SyncSponsorAsync(dto, eventType);

        await CreateSyncLogAsync("Sponsor", sponsor.SponsorId, "PowerSchool", eventType,
            correlationId, result, dto);

        return result;
    }

    private async Task<SyncResult> SyncToStudentChargingPortalAsync(Sponsor sponsor, string eventType, string correlationId)
    {
        var dto = new StudentChargingPortalSponsorDto
        {
            SponsorId = sponsor.SponsorId,
            SponsorName = sponsor.SponsorName,
            StudentChargingPortalId = sponsor.StudentChargingPortalId,
            IsActive = sponsor.IsActive
            // Note: Coverage rules would be loaded from LoGs in real implementation
        };

        var result = await _scpAdapter.SyncSponsorAsync(dto, eventType);

        await CreateSyncLogAsync("Sponsor", sponsor.SponsorId, "StudentChargingPortal", eventType,
            correlationId, result, dto);

        return result;
    }

    private async Task<SyncResult> SyncToNetSuiteAsync(Sponsor sponsor, string eventType, string correlationId)
    {
        var dto = new NetSuiteSponsorDto
        {
            SponsorId = sponsor.SponsorId,
            SponsorName = sponsor.SponsorName,
            LegalName = sponsor.LegalName,
            Tin = sponsor.Tin,
            NetSuiteId = sponsor.NetSuiteId,
            BillingAddress = sponsor.Address,
            IsActive = sponsor.IsActive
        };

        var result = await _netSuiteAdapter.SyncSponsorAsync(dto, eventType);

        await CreateSyncLogAsync("Sponsor", sponsor.SponsorId, "NetSuite", eventType,
            correlationId, result, dto);

        return result;
    }

    private async Task<SyncResult> SyncToOnlineBillingSystemAsync(Sponsor sponsor, string eventType, string correlationId)
    {
        var dto = new OnlineBillingSponsorDto
        {
            SponsorId = sponsor.SponsorId,
            SponsorName = sponsor.SponsorName,
            OnlineBillingSystemId = sponsor.OnlineBillingSystemId,
            StatementEmail = sponsor.Contacts?.FirstOrDefault()?.Email,
            CoveredStudentIds = sponsor.Students?.Select(s => s.StudentId).ToList() ?? new(),
            IsActive = sponsor.IsActive
        };

        var result = await _obsAdapter.SyncSponsorAsync(dto, eventType);

        await CreateSyncLogAsync("Sponsor", sponsor.SponsorId, "OnlineBillingSystem", eventType,
            correlationId, result, dto);

        return result;
    }

    private async Task CreateSyncLogAsync(string entityType, string entityId, string targetSystem,
        string eventType, string correlationId, SyncResult result, object requestDto)
    {
        var syncLog = new SyncLog
        {
            EntityType = entityType,
            EntityId = entityId,
            TargetSystem = targetSystem,
            EventType = eventType,
            Status = result.Success ? "Succeeded" : "Failed",
            CorrelationId = correlationId,
            AttemptedAt = DateTime.UtcNow,
            LastSucceededAt = result.Success ? DateTime.UtcNow : null,
            ErrorMessage = result.ErrorMessage,
            ExternalReferenceId = result.ExternalReferenceId,
            RequestPayload = JsonSerializer.Serialize(requestDto),
            ResponsePayload = JsonSerializer.Serialize(result),
            PayloadVersion = "1.0"
        };

        _context.SyncLogs.Add(syncLog);
        await _context.SaveChangesAsync();
    }
}
