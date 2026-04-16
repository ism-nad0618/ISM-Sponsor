namespace ISMSponsor.Models.API
{
    public class CoverageEvaluationResponse
    {
        public CoverageDecision Decision { get; set; }

        public BillTo BillTo { get; set; }

        public decimal SponsorAmount { get; set; }

        public decimal ParentAmount { get; set; }

        public string ReasonCode { get; set; } = string.Empty;

        public string Explanation { get; set; } = string.Empty;

        public int? MatchedRuleId { get; set; }

        public string? RuleVersion { get; set; }

        public int AuditRecordId { get; set; }

        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Correlation ID for end-to-end traceability. Inherited from request or generated.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Alternative decision identifier (equivalent to AuditRecordId for external consumers).
        /// </summary>
        public string? DecisionId { get; set; }

        /// <summary>
        /// Timestamp when decision was evaluated (UTC).
        /// </summary>
        public DateTime EvaluatedAt { get; set; }

        /// <summary>
        /// Sponsor coverage percentage (0.0000 to 1.0000). Calculated as SponsorAmount / ChargeAmount.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0, 1)]
        public decimal SponsorPercent { get; set; }

        /// <summary>
        /// Parent coverage percentage (0.0000 to 1.0000). Calculated as ParentAmount / ChargeAmount.
        /// </summary>
        [System.ComponentModel.DataAnnotations.Range(0, 1)]
        public decimal ParentPercent { get; set; }

        /// <summary>
        /// JSON snapshot of the matched LoG coverage rule (for replay/validation).
        /// Immutable proof of which rule applied to this decision.
        /// </summary>
        public string? RuleSnapshot { get; set; }
    }
}
