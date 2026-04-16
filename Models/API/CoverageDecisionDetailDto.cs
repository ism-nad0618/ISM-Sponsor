using System;

namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for retrieving detailed coverage decision information.
    /// Used by decision retrieval endpoints (/decisions/{auditId}, /decisions?correlationId=...).
    /// Includes full audit trail context, percentages, and rule snapshot.
    /// </summary>
    public class CoverageDecisionDetailDto
    {
        /// <summary>
        /// Unique audit record ID for this decision (primary key in database).
        /// </summary>
        public int AuditId { get; set; }

        /// <summary>
        /// Correlation ID for end-to-end traceability across systems.
        /// Used to track decision through PowerSchool, ISM, NetSuite, OBS ecosystem.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Student ID for which coverage was evaluated.
        /// </summary>
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Sponsor ID directly responsible (from Letter of Guarantee).
        /// </summary>
        public string? SponsorId { get; set; }

        /// <summary>
        /// Requested charge amount (in school currency, typically PHP).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Date of the charge/fee being evaluated.
        /// Used to validate rule effective dates.
        /// </summary>
        public DateTime ChargeDate { get; set; }

        /// <summary>
        /// Coverage decision: Covered (full sponsor responsibility), Split (shared), NotCovered (parent responsibility).
        /// </summary>
        public CoverageDecision Decision { get; set; }

        /// <summary>
        /// Primary bill-to party: Sponsor, Parent, or Split.
        /// Directs NetSuite/OBS billing distribution.
        /// </summary>
        public BillTo BillTo { get; set; }

        /// <summary>
        /// Amount approved to sponsor (in school currency).
        /// </summary>
        public decimal SponsorAmount { get; set; }

        /// <summary>
        /// Amount approved to parent (in school currency).
        /// </summary>
        public decimal ParentAmount { get; set; }

        /// <summary>
        /// Sponsor coverage percentage (0-100%).
        /// Calculated as (SponsorAmount / Amount) * 100.
        /// </summary>
        public decimal SponsorPercent { get; set; }

        /// <summary>
        /// Parent coverage percentage (0-100%).
        /// Calculated as (ParentAmount / Amount) * 100.
        /// Typically SponsorPercent + ParentPercent = 100.
        /// </summary>
        public decimal ParentPercent { get; set; }

        /// <summary>
        /// Machine-readable reason code explaining the decision.
        /// Examples: FULL_COVERAGE_ITEM, PERCENTAGE_SPLIT, NO_ACTIVE_LOG, etc.
        /// </summary>
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable explanation of the decision for UI display.
        /// </summary>
        public string? Explanation { get; set; }

        /// <summary>
        /// Version identifier for the LoG rule that produced this decision.
        /// Format: LOG{LogId}-RULE{RuleId}-{Timestamp}.
        /// Enables tracking rule changes and versioning.
        /// </summary>
        public string? RuleVersion { get; set; }

        /// <summary>
        /// JSON snapshot of the matched rule at time of evaluation.
        /// Preserves rule configuration for audit compliance and replay analysis.
        /// If deserialization needed, parser should be flexible to rule schema changes.
        /// </summary>
        public string? RuleSnapshot { get; set; }

        /// <summary>
        /// When this decision was evaluated (UTC timestamp).
        /// </summary>
        public DateTime EvaluatedOn { get; set; }

        /// <summary>
        /// User ID who triggered the evaluation (if evaluation was explicit).
        /// </summary>
        public string EvaluatedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Display name or email of the user who triggered evaluation.
        /// </summary>
        public string? EvaluatedByUserDisplay { get; set; }

        /// <summary>
        /// Success flag: true if evaluation completed, false if error occurred.
        /// Check ErrorMessage if Success=false.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if evaluation failed (Success=false).
        /// Examples: validation errors, rule loading failures, database issues.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
