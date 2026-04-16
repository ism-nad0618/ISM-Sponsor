using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Request DTO for PowerSchool student-sponsor synchronization.
    /// Used to sync student-sponsor links from PowerSchool to ISM Sponsor system.
    /// </summary>
    public class StudentSponsorSyncRequest
    {
        /// <summary>
        /// List of student-sponsor mappings to sync.
        /// </summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one mapping is required")]
        public List<StudentSponsorMapping> Mappings { get; set; } = new();

        /// <summary>
        /// School year ID for which these mappings apply.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string SchoolYearId { get; set; } = string.Empty;

        /// <summary>
        /// Optional correlation ID for end-to-end traceability.
        /// If omitted, system generates one.
        /// </summary>
        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Source system identifier (e.g., "PowerSchool").
        /// </summary>
        [MaxLength(100)]
        public string SourceSystem { get; set; } = "PowerSchool";

        /// <summary>
        /// Timestamp when sync was initiated (UTC).
        /// </summary>
        public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Individual student-sponsor mapping.
    /// </summary>
    public class StudentSponsorMapping
    {
        /// <summary>
        /// Student ID from PowerSchool.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Sponsor ID (matched from Sponsor_OrgName in PowerSchool).
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string SponsorId { get; set; } = string.Empty;

        /// <summary>
        /// Effective start date for this sponsorship.
        /// </summary>
        [Required]
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective end date for this sponsorship (nullable if ongoing).
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Optional student name for validation/logging.
        /// </summary>
        [MaxLength(500)]
        public string? StudentName { get; set; }
    }
}
