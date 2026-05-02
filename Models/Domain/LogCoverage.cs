namespace ISMSponsor.Models.Domain
{
    /// <summary>
    /// Represents a Letter of Guarantee (LoG) for a student in a school year
    /// </summary>
    public class LogCoverage
    {
        public int LogId { get; set; }
        public string SchoolYearId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        
        // Status workflow: Draft → Submitted → Approved/Rejected (Admin reviews, auto-activates on approval)
        public string LogStatus { get; set; } = "Draft";
        
        // Approval workflow tracking
        public DateTime? SubmittedOn { get; set; }
        public string? SubmittedByUserId { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public string? ReviewedByUserId { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? RejectedOn { get; set; }
        public string? RejectedByUserId { get; set; }
        
        // Activation lifecycle
        public bool IsActive { get; set; } = false;
        public DateTime? ActivatedOn { get; set; }
        public string? ActivatedByUserId { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public string? DeactivatedByUserId { get; set; }
        public string? DeactivationReason { get; set; }
        
        // Effective dates for coverage
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        
        // Attachment and documentation
        public string? AttachmentFileName { get; set; }
        public DateTime? AttachmentUploadedOn { get; set; }
        
        // Notes and tracking
        public string? Notes { get; set; }
        public string? ReviewComments { get; set; }
        
        // Audit fields
        public DateTime CreatedOn { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedByUserId { get; set; }

        // Navigation properties
        public Student? Student { get; set; }
        public Sponsor? Sponsor { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
        public ApplicationUser? ActivatedByUser { get; set; }
        public ApplicationUser? DeactivatedByUser { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }
        public ApplicationUser? ModifiedByUser { get; set; }
        public ICollection<LoGCoverageRule> CoverageRules { get; set; } = new List<LoGCoverageRule>();
    }
}