using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.Domain;

/// <summary>
/// Tracks synchronization events with external systems.
/// Records attempts, successes, failures, and retry state for integration monitoring.
/// </summary>
public class SyncLog
{
    [Key]
    public int SyncLogId { get; set; }

    /// <summary>
    /// Type of entity being synced: Sponsor, Student, LoG, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity being synced (e.g., SponsorId, StudentId, LogId)
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Target system: PowerSchool, StudentChargingPortal, NetSuite, OnlineBillingSystem
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TargetSystem { get; set; } = string.Empty;

    /// <summary>
    /// Sync event type: Create, Update, Merge, Delete, Activate, Deactivate
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Payload version for compatibility tracking.
    /// </summary>
    [MaxLength(20)]
    public string? PayloadVersion { get; set; }

    /// <summary>
    /// When the sync was first attempted.
    /// </summary>
    [Required]
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the sync last succeeded (if ever).
    /// </summary>
    public DateTime? LastSucceededAt { get; set; }

    /// <summary>
    /// Number of retry attempts made.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Sync status: Pending, InProgress, Succeeded, Failed, Skipped
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Error message if sync failed.
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Correlation ID for tracking related sync operations.
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Request payload sent to target system (JSON or XML).
    /// </summary>
    public string? RequestPayload { get; set; }

    /// <summary>
    /// Response payload received from target system.
    /// </summary>
    public string? ResponsePayload { get; set; }

    /// <summary>
    /// External reference ID returned by target system (if applicable).
    /// </summary>
    [MaxLength(450)]
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// When this record was created.
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this record was last updated (for retry tracking).
    /// </summary>
    public DateTime? ModifiedOn { get; set; }
}
