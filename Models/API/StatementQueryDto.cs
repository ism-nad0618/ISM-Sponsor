namespace ISMSponsor.Models.API
{
    /// <summary>
    /// DTO for querying statement data for a student or sponsor.
    /// Returns aggregated coverage allocations for statement presentation.
    /// </summary>
    public class StatementQueryDto
    {
        /// <summary>
        /// Student ID (if querying by student).
        /// </summary>
        public string? StudentId { get; set; }

        /// <summary>
        /// Sponsor ID (if querying by sponsor).
        /// </summary>
        public string? SponsorId { get; set; }

        /// <summary>
        /// School year filter.
        /// </summary>
        public string? SchoolYearId { get; set; }

        /// <summary>
        /// Statement period start date.
        /// </summary>
        public DateTime? PeriodStart { get; set; }

        /// <summary>
        /// Statement period end date.
        /// </summary>
        public DateTime? PeriodEnd { get; set; }

        /// <summary>
        /// List of statement line items.
        /// </summary>
        public List<StatementLineItem> LineItems { get; set; } = new();

        /// <summary>
        /// Total sponsor responsibility.
        /// </summary>
        public decimal TotalSponsorAmount { get; set; }

        /// <summary>
        /// Total parent responsibility.
        /// </summary>
        public decimal TotalParentAmount { get; set; }

        /// <summary>
        /// Grand total for the statement period.
        /// </summary>
        public decimal GrandTotal { get; set; }

        /// <summary>
        /// Currency code.
        /// </summary>
        public string Currency { get; set; } = "PHP";

        /// <summary>
        /// Statement generation timestamp.
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Individual line item on a statement.
    /// </summary>
    public class StatementLineItem
    {
        public int AuditId { get; set; }
        public DateTime ChargeDate { get; set; }
        public string ChargeDescription { get; set; } = string.Empty;
        public decimal SponsorAmount { get; set; }
        public decimal ParentAmount { get; set; }
        public BillTo BillTo { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
    }
}
