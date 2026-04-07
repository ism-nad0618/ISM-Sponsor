using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISMSponsor.Models.Domain;

/// <summary>
/// Represents a potential duplicate sponsor pair detected by the system.
/// Supports duplicate review and controlled merge workflows.
/// </summary>
public class SponsorDuplicateCandidate
{
    [Key]
    public int CandidateId { get; set; }

    [Required]
    [MaxLength(450)]
    public string PrimarySponsorId { get; set; } = string.Empty;

    [Required]
    [MaxLength(450)]
    public string DuplicateSponsorId { get; set; } = string.Empty;

    /// <summary>
    /// Match confidence score (0-100). Higher values indicate stronger match.
    /// </summary>
    [Required]
    public decimal MatchScore { get; set; }

    /// <summary>
    /// Comma-separated list of match reasons (e.g., "SameTIN,SimilarName,SameAddress")
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string MatchReasons { get; set; } = string.Empty;

    /// <summary>
    /// Detailed explanation of why these sponsors were matched.
    /// </summary>
    [MaxLength(1000)]
    public string? MatchExplanation { get; set; }

    /// <summary>
    /// Status: Pending, ReviewedNotDuplicate, MergeScheduled, Merged, Ignored
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// When this candidate pair was detected.
    /// </summary>
    [Required]
    public DateTime DetectedOn { get; set; } = DateTime.UtcNow;

    [MaxLength(450)]
    public string? DetectedByUserId { get; set; }

    /// <summary>
    /// When an admin reviewed this candidate.
    /// </summary>
    public DateTime? ReviewedOn { get; set; }

    [MaxLength(450)]
    public string? ReviewedByUserId { get; set; }

    [MaxLength(200)]
    public string? ReviewedByUserDisplay { get; set; }

    [MaxLength(1000)]
    public string? ReviewNotes { get; set; }

    /// <summary>
    /// If merged, the ID of the merge operation that processed this candidate.
    /// </summary>
    public int? MergeOperationId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(PrimarySponsorId))]
    public virtual Sponsor? PrimarySponsor { get; set; }

    [ForeignKey(nameof(DuplicateSponsorId))]
    public virtual Sponsor? DuplicateSponsor { get; set; }

    public virtual ApplicationUser? ReviewedByUser { get; set; }

    public virtual MergeOperation? MergeOperation { get; set; }
}
