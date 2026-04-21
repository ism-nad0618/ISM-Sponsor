namespace ISMSponsor.Models.API
{
    /// <summary>
    /// Letter of Guarantee (LoG) summary for API responses
    /// </summary>
    public class LogDto
    {
        public int LogId { get; set; }
        public string SchoolYearId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public string LogStatus { get; set; } = "Draft";
        public bool IsActive { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public List<LogItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Coverage item/rule within a Letter of Guarantee
    /// </summary>
    public class LogItemDto
    {
        public int RuleId { get; set; }
        public string CoverageTarget { get; set; } = string.Empty; // "Item" or "Category"
        public string? ItemId { get; set; }
        public string? CategoryId { get; set; }
        public string CoverageType { get; set; } = string.Empty; // "Full", "Percentage", "FixedAmount", "UpToCap"
        public decimal? CoveragePercentage { get; set; }
        public decimal? CoverageFixedAmount { get; set; }
        public decimal? CapAmount { get; set; }
    }

    /// <summary>
    /// Request to create a new Letter of Guarantee
    /// </summary>
    public class CreateLogRequest
    {
        public string SchoolYearId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request to add coverage items to an existing LoG
    /// </summary>
    public class AddLogItemRequest
    {
        public string CoverageTarget { get; set; } = "Item"; // "Item" or "Category"
        public string? ItemId { get; set; }
        public string? CategoryId { get; set; }
        public string CoverageType { get; set; } = "Full"; // "Full", "Percentage", "FixedAmount", "UpToCap"
        public decimal? CoveragePercentage { get; set; }
        public decimal? CoverageFixedAmount { get; set; }
        public decimal? CapAmount { get; set; }
    }

    /// <summary>
    /// Request to create a new sponsor
    /// </summary>
    public class CreateSponsorRequest
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? Tin { get; set; }
        public string? Address { get; set; }
    }
}
