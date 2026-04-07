namespace ISMSponsor.Models.Domain
{
    /// <summary>
    /// Defines a specific coverage rule within a Letter of Guarantee
    /// </summary>
    public class LoGCoverageRule
    {
        public int RuleId { get; set; }
        public int LogId { get; set; }

        // What is covered
        public string CoverageTarget { get; set; } = string.Empty; // "Item" or "Category"
        public string? ItemId { get; set; }
        public string? CategoryId { get; set; }

        // How much is covered
        public string CoverageType { get; set; } = string.Empty; // "Full", "Percentage", "FixedAmount", "UpToCap"
        public decimal? CoveragePercentage { get; set; }
        public decimal? CoverageFixedAmount { get; set; }
        public decimal? CapAmount { get; set; }

        // When rule applies
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        // Additional details
        public string? ExceptionNote { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Audit
        public DateTime CreatedOn { get; set; }
        public string? CreatedByUserId { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedByUserId { get; set; }

        // Navigation properties
        public LogCoverage? LetterOfGuarantee { get; set; }
        public Item? Item { get; set; }
        public ItemCategory? Category { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }
        public ApplicationUser? ModifiedByUser { get; set; }
    }
}
