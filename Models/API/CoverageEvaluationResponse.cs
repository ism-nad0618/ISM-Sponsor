namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Coverage evaluation result with allocation breakdown.
    /// Example: { "decisionId": "DEC-001", "coverageStatus": "Split", "billTo": "Split", "sponsorAmount": 75000, "parentAmount": 25000, "reasonCode": "CAP_PARTIAL", "ruleVersion": "RV-1" }
    /// </summary>
    public class CoverageEvaluationResponse
    {
        /// <summary>
        /// Coverage decision: Covered, NotCovered, or Split
        /// </summary>
        public CoverageDecision Decision { get; set; }

        /// <summary>
        /// Bill recipient: Sponsor, Parent, or Split
        /// </summary>
        public BillTo BillTo { get; set; }

        /// <summary>
        /// Student ID
        /// </summary>
        public string? StudentId { get; set; }

        /// <summary>
        /// Student name
        /// </summary>
        public string? StudentName { get; set; }

        /// <summary>
        /// Sponsor ID (e.g., "SP002")
        /// </summary>
        public string? SponsorId { get; set; }

        /// <summary>
        /// Sponsor name
        /// </summary>
        public string? SponsorName { get; set; }

        /// <summary>
        /// Amount covered by sponsor (in cents)
        /// </summary>
        public decimal SponsorAmount { get; set; }

        /// <summary>
        /// Amount payable by parent (in cents)
        /// </summary>
        public decimal ParentAmount { get; set; }

        /// <summary>
        /// Total charge amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Machine-readable reason code (e.g., "FULL_COVERAGE", "CAP_PARTIAL", "NO_LOG")
        /// </summary>
        public string ReasonCode { get; set; } = string.Empty;

        public string Explanation { get; set; } = string.Empty;

        public int? MatchedRuleId { get; set; }

        /// <summary>
        /// Version identifier of the coverage rule that was applied (e.g., "RV-1")
        /// </summary>
        public string? RuleVersion { get; set; }

        /// <summary>
        /// Audit record ID for traceability (0 if preview mode)
        /// </summary>
        public int AuditRecordId { get; set; }

        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Correlation ID for end-to-end traceability. Inherited from request or generated.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Decision identifier for external systems (e.g., "DEC-001")
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

        /// <summary>
        /// Billing allocations when decision is Split or billTo is SponsorAndParent.
        /// Contains exactly two allocations: one for Sponsor and one for Parent.
        /// </summary>
        public List<BillingAllocationDto>? Allocations { get; set; }
    }
}
