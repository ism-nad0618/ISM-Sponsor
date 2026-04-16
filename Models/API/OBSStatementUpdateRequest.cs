using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Request DTO for updating Online Billing System statements.
    /// Contains coverage allocation data for statement presentation.
    /// </summary>
    public class OBSStatementUpdateRequest
    {
        /// <summary>
        /// Coverage decision audit ID.
        /// </summary>
        [Required]
        public int DecisionId { get; set; }

        /// <summary>
        /// Student ID for the statement.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Sponsor ID to display on the statement.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string SponsorId { get; set; } = string.Empty;

        /// <summary>
        /// Statement line reference or charge line ID.
        /// </summary>
        [MaxLength(50)]
        public string? StatementLineReference { get; set; }

        /// <summary>
        /// Bill-To party: Sponsor, Parent, or Split.
        /// </summary>
        [Required]
        public BillTo BillTo { get; set; }

        /// <summary>
        /// Amount displayed on sponsor's statement.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SponsorDisplayAmount { get; set; }

        /// <summary>
        /// Amount displayed on parent's statement.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal ParentDisplayAmount { get; set; }

        /// <summary>
        /// Charge item description (for statement display).
        /// </summary>
        [MaxLength(500)]
        public string? ChargeDescription { get; set; }

        /// <summary>
        /// Charge date (for statement grouping).
        /// </summary>
        [Required]
        public DateTime ChargeDate { get; set; }

        /// <summary>
        /// School year for the statement.
        /// </summary>
        [MaxLength(450)]
        public string? SchoolYearId { get; set; }

        /// <summary>
        /// Decision metadata for audit display.
        /// </summary>
        public OBSDecisionMetadata? DecisionMetadata { get; set; }

        /// <summary>
        /// Correlation ID for end-to-end traceability.
        /// </summary>
        [MaxLength(100)]
        public string? CorrelationId { get; set; }
    }

    /// <summary>
    /// Decision metadata for OBS statement display.
    /// </summary>
    public class OBSDecisionMetadata
    {
        public string ReasonCode { get; set; } = string.Empty;
        public string? RuleVersion { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }
}
