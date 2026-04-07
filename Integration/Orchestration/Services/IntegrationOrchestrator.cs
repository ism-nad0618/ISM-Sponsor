using ISMSponsor.Integration.Contracts;
using ISMSponsor.Services;
using Microsoft.Extensions.Logging;

namespace ISMSponsor.Integration.Orchestration.Services;

/// <summary>
/// Central orchestrator for all downstream sponsor integrations.
/// Coordinates synchronization to PowerSchool, NetSuite, OBS, and SCP.
/// </summary>
public interface IIntegrationOrchestrator
{
    /// <summary>
    /// Queue sponsor synchronization for all downstream targets (fire-and-forget).
    /// Safe to call without blocking sponsor save operations.
    /// </summary>
    Task QueueSponsorSyncAsync(string sponsorId, IntegrationEventType eventType, string? username = null);
    
    /// <summary>
    /// Process sponsor synchronization to all downstream targets synchronously.
    /// Use for testing or when immediate sync is required.
    /// </summary>
    Task<DownstreamSyncResult> ProcessSponsorSyncAsync(string sponsorId, IntegrationEventType eventType, string? username = null);
}

public class IntegrationOrchestrator : IIntegrationOrchestrator
{
    private readonly IPowerSchoolSponsorListService _powerSchoolService;
    private readonly INetSuiteIntegrationService _netSuiteService;
    private readonly IObsIntegrationService _obsService;
    private readonly IScpIntegrationService _scpService;
    private readonly ILogger<IntegrationOrchestrator> _logger;

    public IntegrationOrchestrator(
        IPowerSchoolSponsorListService powerSchoolService,
        INetSuiteIntegrationService netSuiteService,
        IObsIntegrationService obsService,
        IScpIntegrationService scpService,
        ILogger<IntegrationOrchestrator> logger)
    {
        _powerSchoolService = powerSchoolService;
        _netSuiteService = netSuiteService;
        _obsService = obsService;
        _scpService = scpService;
        _logger = logger;
    }

    public async Task QueueSponsorSyncAsync(string sponsorId, IntegrationEventType eventType, string? username = null)
    {
        // Fire-and-forget async processing - does NOT block sponsor save
        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessSponsorSyncAsync(sponsorId, eventType, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Background sponsor sync failed for {SponsorId}, event {EventType}", 
                    sponsorId, eventType);
            }
        });

        await Task.CompletedTask;
    }

    public async Task<DownstreamSyncResult> ProcessSponsorSyncAsync(string sponsorId, IntegrationEventType eventType, string? username = null)
    {
        var startTime = DateTime.UtcNow;
        var correlationId = Guid.NewGuid().ToString();
        var eventTypeString = eventType.ToString();

        _logger.LogInformation(
            "Starting downstream sponsor sync for {SponsorId}, event {EventType}, correlationId {CorrelationId}",
            sponsorId, eventType, correlationId);

        var result = new DownstreamSyncResult
        {
            SponsorId = sponsorId,
            EventType = eventType,
            CorrelationId = correlationId,
            StartedAt = startTime
        };

        // 1. PowerSchool: Publish full sponsor list
        try
        {
            var psResult = await _powerSchoolService.PublishSponsorOrgListAsync(eventTypeString, sponsorId);
            result.PowerSchoolResult = new SyncResult
            {
                Success = psResult.Success,
                Status = psResult.Success ? "Succeeded" : "Failed",
                Message = psResult.ErrorMessage ?? $"Published {psResult.SponsorCount} sponsors",
                ErrorMessage = psResult.ErrorMessage
            };
            _logger.LogInformation("PowerSchool sync: {Status}", result.PowerSchoolResult.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PowerSchool sync exception for {SponsorId}", sponsorId);
            result.PowerSchoolResult = CreateExceptionResult(ex);
        }

        // 2. NetSuite: Upsert to Sponsors List
        try
        {
            result.NetSuiteResult = await _netSuiteService.UpsertSponsorAsync(sponsorId, eventTypeString);
            _logger.LogInformation("NetSuite sync: {Status}", result.NetSuiteResult.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NetSuite sync exception for {SponsorId}", sponsorId);
            result.NetSuiteResult = CreateExceptionResult(ex);
        }

        // 3. Online Billing System: Upsert to BOTH CompanySponsors AND CompanySponsorAccount
        try
        {
            var obsResults = await _obsService.UpsertBothAsync(sponsorId, eventTypeString, username);
            result.ObsCompanySponsorsResult = obsResults.FirstOrDefault(); // CompanySponsors
            result.ObsCompanySponsorAccountResult = obsResults.Skip(1).FirstOrDefault(); // CompanySponsorAccount
            
            _logger.LogInformation(
                "OBS sync - CompanySponsors: {Status1}, CompanySponsorAccount: {Status2}", 
                result.ObsCompanySponsorsResult?.Status ?? "No Result",
                result.ObsCompanySponsorAccountResult?.Status ?? "No Result");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OBS sync exception for {SponsorId}", sponsorId);
            result.ObsCompanySponsorsResult = CreateExceptionResult(ex);
            result.ObsCompanySponsorAccountResult = CreateExceptionResult(ex);
        }

        // 4. Student Charging Portal: Upsert to Sponsors table
        try
        {
            result.ScpResult = await _scpService.UpsertSponsorAsync(sponsorId, eventTypeString);
            _logger.LogInformation("SCP sync: {Status}", result.ScpResult.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SCP sync exception for {SponsorId}", sponsorId);
            result.ScpResult = CreateExceptionResult(ex);
        }

        result.CompletedAt = DateTime.UtcNow;
        result.Duration = result.CompletedAt - result.StartedAt;
        result.Success = result.GetOverallSuccess();

        _logger.LogInformation(
            "Completed downstream sponsor sync for {SponsorId}. Duration: {Duration}ms, Success: {Success}",
            sponsorId, result.Duration.TotalMilliseconds, result.Success);

        return result;
    }

    private SyncResult CreateExceptionResult(Exception ex)
    {
        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorCode = "EXCEPTION",
            ErrorMessage = $"{ex.GetType().Name}: {ex.Message}",
            ProcessedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Consolidated result from all downstream target synchronizations.
/// </summary>
public class DownstreamSyncResult
{
    public string SponsorId { get; set; } = string.Empty;
    public IntegrationEventType EventType { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }

    // Individual target results
    public SyncResult? PowerSchoolResult { get; set; }
    public SyncResult? NetSuiteResult { get; set; }
    public SyncResult? ObsCompanySponsorsResult { get; set; }
    public SyncResult? ObsCompanySponsorAccountResult { get; set; }
    public SyncResult? ScpResult { get; set; }

    /// <summary>
    /// Returns true if ALL targets succeeded, false if ANY target failed.
    /// </summary>
    public bool GetOverallSuccess()
    {
        var results = new[] 
        { 
            PowerSchoolResult, 
            NetSuiteResult, 
            ObsCompanySponsorsResult, 
            ObsCompanySponsorAccountResult, 
            ScpResult 
        };

        return results.All(r => r == null || r.Success);
    }

    /// <summary>
    /// Returns count of failed targets.
    /// </summary>
    public int GetFailureCount()
    {
        var results = new[] 
        { 
            PowerSchoolResult, 
            NetSuiteResult, 
            ObsCompanySponsorsResult, 
            ObsCompanySponsorAccountResult, 
            ScpResult 
        };

        return results.Count(r => r != null && !r.Success);
    }
}
