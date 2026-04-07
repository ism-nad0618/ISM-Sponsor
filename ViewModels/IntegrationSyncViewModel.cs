using ISMSponsor.Models.Domain;

namespace ISMSponsor.ViewModels;

/// <summary>
/// ViewModel for displaying integration sync status and history.
/// </summary>
public class IntegrationSyncViewModel
{
    public SyncStatusSummary StatusSummary { get; set; } = new();
    public List<SyncHistoryItem> RecentSyncs { get; set; } = new();
    public List<SyncFailureDetail> RecentFailures { get; set; } = new();
    public bool IsSyncEnabled { get; set; }
    public DateTime? LastSuccessfulSync { get; set; }
    public DateTime? NextScheduledSync { get; set; }
}

/// <summary>
/// Summary of current sync status across all systems.
/// </summary>
public class SyncStatusSummary
{
    public int TotalSponsorsInSystem { get; set; }
    public int SponsorsPendingSync { get; set; }
    public int FailedSyncsLast24Hours { get; set; }
    public int FailedSyncsLast7Days { get; set; }
    
    public Dictionary<string, SystemSyncStatus> SystemStatuses { get; set; } = new();
    
    public bool IsHealthy => FailedSyncsLast24Hours == 0;
    public string HealthStatus
    {
        get
        {
            if (FailedSyncsLast24Hours == 0) return "Healthy";
            if (FailedSyncsLast24Hours < 5) return "Degraded";
            return "Unhealthy";
        }
    }
}

/// <summary>
/// Sync status for a specific external system.
/// </summary>
public class SystemSyncStatus
{
    public string SystemName { get; set; } = string.Empty;
    public DateTime? LastSuccessfulSync { get; set; }
    public int PendingRecords { get; set; }
    public int FailedRecords { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? LastError { get; set; }
}

/// <summary>
/// Individual sync operation history item.
/// </summary>
public class SyncHistoryItem
{
    public int SyncLogId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string ExternalSystemName { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SyncedAt { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// Detailed information about a sync failure.
/// </summary>
public class SyncFailureDetail
{
    public int SyncLogId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string ExternalSystemName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public DateTime FailedAt { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public bool CanRetry { get; set; }
}

/// <summary>
/// Request to manually retry a failed sync.
/// </summary>
public class RetrySyncRequest
{
    public int SyncLogId { get; set; }
    public string? Notes { get; set; }
}
