namespace ISMSponsor.Models.Domain
{
    public class Sponsor
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty; // Legacy field, kept for backward compatibility
        public string Tin { get; set; } = string.Empty;

        // Cross-system integration IDs
        public string? PowerSchoolId { get; set; }
        public string? NetSuiteId { get; set; }
        public string? StudentChargingPortalId { get; set; }
        public string? OnlineBillingSystemId { get; set; }
        public string? ExternalSystemId { get; set; }

        // Merge and lineage tracking
        public bool IsMerged { get; set; } = false;
        public string? MergedIntoSponsorId { get; set; }
        public DateTime? MergedOn { get; set; }
        public int? MergeOperationId { get; set; }

        // Approval workflow
        public string? ApprovalStatus { get; set; } // "PendingApproval", "Approved", "Rejected"
        public DateTime? ApprovedOn { get; set; }
        public string? ApprovedByUserId { get; set; }
        public string? ApprovalNotes { get; set; }

        // Metadata
        public DateTime CreatedOn { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedByUserId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<SponsorAddress>? Addresses { get; set; }
        public ICollection<SponsorContact>? Contacts { get; set; }
        public ICollection<Student>? Students { get; set; }
        public ICollection<ChangeRequest>? ChangeRequests { get; set; }
        public ICollection<LogCoverage>? LettersOfGuarantee { get; set; }
    }
}