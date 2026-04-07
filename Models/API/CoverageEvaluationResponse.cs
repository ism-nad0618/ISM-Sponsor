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
    }
}
