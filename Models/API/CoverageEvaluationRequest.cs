using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.API
{
    public class CoverageEvaluationRequest
    {
        [Required(ErrorMessage = "School year is required")]
        [MaxLength(450)]
        public string SchoolYearId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student ID is required")]
        [MaxLength(450)]
        public string StudentId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? SponsorId { get; set; }

        public int? LogId { get; set; }

        [MaxLength(450)]
        public string? ItemId { get; set; }

        [MaxLength(450)]
        public string? CategoryId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Charge date is required")]
        public DateTime ChargeDate { get; set; }

        /// <summary>
        /// Optional correlation ID for end-to-end traceability. If omitted, system generates one.
        /// </summary>
        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Explicit charge line identifier. Used for reconciliation and audit.
        /// </summary>
        [MaxLength(50)]
        public string? ChargeLineId { get; set; }

        /// <summary>
        /// If true, evaluation is preview-only (no audit logging). Default: false (commit/persist).
        /// </summary>
        public bool IsPreview { get; set; } = false;

        /// <summary>
        /// Optional reference to a coverage override. If provided, system validates and applies if eligible.
        /// </summary>
        [MaxLength(50)]
        public string? StudentOverrideId { get; set; }

        /// <summary>
        /// Flag indicating if this charge requires documentation verification from sponsor.
        /// </summary>
        public bool RequiresCoverageDocumentation { get; set; } = false;
    }
}
