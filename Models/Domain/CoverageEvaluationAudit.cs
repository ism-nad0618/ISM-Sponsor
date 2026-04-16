using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.Domain
{
    public class CoverageEvaluationAudit
    {
        [Key]
        public int AuditId { get; set; }

        [Required]
        public DateTime EvaluatedOn { get; set; }

        [Required]
        [MaxLength(450)]
        public string EvaluatedByUserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EvaluatedByUserDisplay { get; set; } = string.Empty;

        [MaxLength(50)]
        public string EvaluatedByRole { get; set; } = string.Empty;

        // Request context
        [Required]
        [MaxLength(20)]
        public string SchoolYearId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string StudentId { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? SponsorId { get; set; }

        public int? LogId { get; set; }

        [MaxLength(20)]
        public string? ItemId { get; set; }

        [MaxLength(20)]
        public string? CategoryId { get; set; }

        [Required]
        public decimal RequestedAmount { get; set; }

        [Required]
        public DateTime ChargeDate { get; set; }

        // Evaluation result
        [Required]
        [MaxLength(20)]
        public string Decision { get; set; } = string.Empty; // Covered, Split, NotCovered

        [Required]
        [MaxLength(20)]
        public string BillTo { get; set; } = string.Empty; // Sponsor, Parent, Split

        public decimal SponsorAmount { get; set; }

        public decimal ParentAmount { get; set; }

        /// <summary>
        /// Percentage of requested amount approved to sponsor (0-100).
        /// Calculated as (SponsorAmount / RequestedAmount) * 100.
        /// Nullable for backward compatibility.
        /// </summary>
        public decimal? SponsorPercent { get; set; }

        /// <summary>
        /// Percentage of requested amount approved to parent (0-100).
        /// Calculated as (ParentAmount / RequestedAmount) * 100.
        /// Nullable for backward compatibility.
        /// </summary>
        public decimal? ParentPercent { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Explanation { get; set; }

        public int? MatchedRuleId { get; set; }

        [MaxLength(50)]
        public string? RuleVersion { get; set; }

        /// <summary>
        /// Unique correlation ID for end-to-end traceability across systems.
        /// Can be used to track decision through PowerSchool, ISM, NetSuite, OBS.
        /// Generated if not provided in request.
        /// </summary>
        [MaxLength(450)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// JSON snapshot of the matched rule at time of evaluation.
        /// Enables audit trail replay and compliance reporting.
        /// Contains rule configuration, effective dates, and coverage parameters.
        /// </summary>
        [MaxLength(4000)]
        public string? RuleSnapshot { get; set; }

        [Required]
        public bool Success { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public virtual ApplicationUser? EvaluatedByUser { get; set; }
        public virtual LogCoverage? LetterOfGuarantee { get; set; }
        public virtual LoGCoverageRule? MatchedRule { get; set; }
    }
}
