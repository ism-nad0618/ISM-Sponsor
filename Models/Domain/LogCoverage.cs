namespace ISMSponsor.Models.Domain
{
    public class LogCoverage
    {
        public string SchoolYearId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public string LogStatus { get; set; } = "Draft"; // Draft, Submitted, UnderReview, Approved, Rejected, Expired
        public string? AttachmentFileName { get; set; }
        public DateTime? AttachmentUploadedOn { get; set; }
        public bool IsActive { get; set; }

        public Student? Student { get; set; }
        public Sponsor? Sponsor { get; set; }
    }
}