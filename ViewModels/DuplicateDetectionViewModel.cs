using ISMSponsor.Models.Domain;

namespace ISMSponsor.ViewModels;

/// <summary>
/// ViewModel for displaying duplicate sponsor detection results and merge operations.
/// </summary>
public class DuplicateDetectionViewModel
{
    public List<DuplicateSponsorGroupViewModel> DuplicateGroups { get; set; } = new();
    public int TotalDuplicates { get; set; }
    public DateTime? LastDetectionRun { get; set; }
}

/// <summary>
/// Represents a group of potential duplicate sponsors.
/// </summary>
public class DuplicateSponsorGroupViewModel
{
    public int DuplicateId { get; set; }
    public string PrimarySponsorId { get; set; } = string.Empty;
    public string PrimarySponsorName { get; set; } = string.Empty;
    public string? PrimarySponsorEmail { get; set; }
    public string? PrimarySponsorPhone { get; set; }
    
    public string SecondarySponsorId { get; set; } = string.Empty;
    public string SecondarySponsorName { get; set; } = string.Empty;
    public string? SecondarySponsorEmail { get; set; }
    public string? SecondarySponsorPhone { get; set; }
    
    public decimal MatchScore { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    
    public bool IsReviewed { get; set; }
    public string? ReviewNotes { get; set; }
}

/// <summary>
/// ViewModel for merging two sponsors.
/// </summary>
public class SponsorMergeViewModel
{
    public string PrimarySponsorId { get; set; } = string.Empty;
    public string PrimarySponsorName { get; set; } = string.Empty;
    public string? PrimarySponsorEmail { get; set; }
    public string? PrimarySponsorPhone { get; set; }
    public string? PrimaryOrganization { get; set; }
    
    public string SecondarySponsorId { get; set; } = string.Empty;
    public string SecondarySponsorName { get; set; } = string.Empty;
    public string? SecondarySponsorEmail { get; set; }
    public string? SecondarySponsorPhone { get; set; }
    public string? SecondaryOrganization { get; set; }
    
    public string MergeReason { get; set; } = string.Empty;
    public string? MergeNotes { get; set; }
    
    // Field conflict resolution
    public Dictionary<string, string> FieldConflicts { get; set; } = new();
    public Dictionary<string, string> ResolvedValues { get; set; } = new();
}

/// <summary>
/// Result of a merge operation.
/// </summary>
public class MergeResultViewModel
{
    public bool Success { get; set; }
    public int? MergeOperationId { get; set; }
    public int? SurvivorSponsorId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, int> MergedCounts { get; set; } = new();
}
