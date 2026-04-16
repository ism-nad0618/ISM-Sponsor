namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for active sponsor-student links.
    /// Used by reference endpoints to show which students are linked to which sponsors.
    /// </summary>
    public class ActiveSponsorLinkDto
    {
        /// <summary>
        /// Student ID linked to the sponsor.
        /// </summary>
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Student name (for display purposes).
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Sponsor ID responsible for the student.
        /// </summary>
        public string SponsorId { get; set; } = string.Empty;

        /// <summary>
        /// Sponsor name (for display purposes).
        /// </summary>
        public string SponsorName { get; set; } = string.Empty;

        /// <summary>
        /// School year for which this link is active.
        /// </summary>
        public string SchoolYearId { get; set; } = string.Empty;

        /// <summary>
        /// School year label (e.g., "2025-2026").
        /// </summary>
        public string SchoolYearLabel { get; set; } = string.Empty;

        /// <summary>
        /// Letter of Guarantee ID (LogId) for this link.
        /// </summary>
        public int LogId { get; set; }

        /// <summary>
        /// Effective start date for the LoG.
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// Effective end date for the LoG (nullable if still active).
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Whether this link is currently active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
