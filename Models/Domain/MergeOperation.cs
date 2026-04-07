using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.Domain;

/// <summary>
/// Represents a controlled merge operation where one sponsor record absorbs another.
/// Provides full audit trail of merge decisions and outcomes.
/// </summary>
public class MergeOperation
{
    [Key]
    public int MergeOperationId { get; set; }

    /// <summary>
    /// The surviving sponsor record that remains active after merge.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string SurvivingSponsorId { get; set; } = string.Empty;

    /// <summary>
    /// The sponsor record that will be retired/merged into the survivor.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string MergedSponsorId { get; set; } = string.Empty;

    /// <summary>
    /// Status: Pending, InProgress, Completed, Failed, RolledBack
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// When the merge was initiated.
    /// </summary>
    [Required]
    public DateTime InitiatedOn { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(450)]
    public string InitiatedByUserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string InitiatedByUserDisplay { get; set; } = string.Empty;

    /// <summary>
    /// When the merge completed (successfully or failed).
    /// </summary>
    public DateTime? CompletedOn { get; set; }

    /// <summary>
    /// Reason or justification for the merge.
    /// </summary>
    [MaxLength(1000)]
    public string? MergeReason { get; set; }

    /// <summary>
    /// JSON or structured field selections made during merge review.
    /// Captures which fields were chosen from which source.
    /// </summary>
    public string? FieldSelections { get; set; }

    /// <summary>
    /// Snapshot of surviving sponsor state before merge.
    /// </summary>
    public string? SurvivorBeforeSnapshot { get; set; }

    /// <summary>
    /// Snapshot of merged sponsor state before merge.
    /// </summary>
    public string? MergedSponsorSnapshot { get; set; }

    /// <summary>
    /// Count of child records reassigned (contacts, addresses, LoGs, etc.)
    /// </summary>
    public int ChildRecordsReassigned { get; set; }

    /// <summary>
    /// Count of sponsor users reassigned or linked.
    /// </summary>
    public int UsersReassigned { get; set; }

    /// <summary>
    /// Count of active LoGs reassigned.
    /// </summary>
    public int LogsReassigned { get; set; }

    /// <summary>
    /// Count of change requests reassigned.
    /// </summary>
    public int RequestsReassigned { get; set; }

    /// <summary>
    /// If merge failed, the error message.
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional notes or outcomes from the merge.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sponsor? SurvivingSponsor { get; set; }
    public virtual Sponsor? MergedSponsor { get; set; }
    public virtual ApplicationUser? InitiatedByUser { get; set; }
    public virtual ICollection<SponsorDuplicateCandidate>? RelatedCandidates { get; set; }
}
