namespace ISMSponsor.Models.Domain
{
    public class ChangeRequest
    {
        public int ChangeRequestId { get; set; }
        public string SponsorId { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty; // sponsorName/legalName/address/tin
        public string FieldLabel { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string? Reason { get; set; } // Reason for the change request
        public string RequestedByUserId { get; set; } = string.Empty;
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; } = "pending"; // pending/approved/rejected
        public string? ResolvedByUserId { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string? ResolutionComment { get; set; } // Admin comment when resolving

        public Sponsor? Sponsor { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? ResolvedByUser { get; set; }
    }
}