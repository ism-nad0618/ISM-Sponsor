using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Request DTO for posting coverage allocation to NetSuite.
    /// Contains all data required for NetSuite billing entry creation.
    /// </summary>
    public class NetSuiteAllocationPostRequest
    {
        /// <summary>
        /// Coverage decision audit ID (AuditId from CoverageEvaluationAudit).
        /// Also referenced as DecisionId in external contexts.
        /// </summary>
        [Required]
        public int DecisionId { get; set; }

        /// <summary>
        /// Student ID for which the charge applies.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Sponsor ID responsible for payment.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string SponsorId { get; set; } = string.Empty;

        /// <summary>
        /// Charge line ID from Student Charging Portal.
        /// </summary>
        [MaxLength(50)]
        public string? ChargeLineId { get; set; }

        /// <summary>
        /// Transaction date for the charge.
        /// </summary>
        [Required]
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Bill-To party: Sponsor, Parent, or Split.
        /// </summary>
        [Required]
        public BillTo BillTo { get; set; }

        /// <summary>
        /// Amount allocated to sponsor.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SponsorAmount { get; set; }

        /// <summary>
        /// Amount allocated to parent.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue)]
        public decimal ParentAmount { get; set; }

        /// <summary>
        /// Sponsor coverage percentage (0-1).
        /// </summary>
        [Required]
        [Range(0, 1)]
        public decimal SponsorPercent { get; set; }

        /// <summary>
        /// Parent coverage percentage (0-1).
        /// </summary>
        [Required]
        [Range(0, 1)]
        public decimal ParentPercent { get; set; }

        /// <summary>
        /// Reason code explaining the coverage decision.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Rule version that produced this allocation.
        /// </summary>
        [MaxLength(50)]
        public string? RuleVersion { get; set; }

        /// <summary>
        /// Correlation ID for end-to-end traceability.
        /// </summary>
        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Currency code (e.g., "PHP").
        /// </summary>
        [MaxLength(3)]
        public string Currency { get; set; } = "PHP";
    }
}
