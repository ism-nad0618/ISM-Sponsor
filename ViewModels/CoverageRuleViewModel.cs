using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels
{
    public class CoverageRuleViewModel
    {
        public int RuleId { get; set; }

        [Required(ErrorMessage = "Coverage Target is required")]
        [Display(Name = "Coverage Target")]
        public string CoverageTarget { get; set; } = "Item"; // Item or Category

        [Display(Name = "Item")]
        public string? ItemId { get; set; }

        [Display(Name = "Category")]
        public string? CategoryId { get; set; }

        [Required(ErrorMessage = "Coverage Type is required")]
        [Display(Name = "Coverage Type")]
        public string CoverageType { get; set; } = "Full"; // Full, Percentage, FixedAmount, UpToCap

        [Display(Name = "Coverage Percentage")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        public decimal? CoveragePercentage { get; set; }

        [Display(Name = "Fixed Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal? CoverageFixedAmount { get; set; }

        [Display(Name = "Cap Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Cap must be positive")]
        public decimal? CapAmount { get; set; }

        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Display(Name = "Exception Note")]
        [DataType(DataType.MultilineText)]
        public string? ExceptionNote { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        // For validation
        public bool IsValid()
        {
            // Must have either ItemId or CategoryId
            if (string.IsNullOrEmpty(ItemId) && string.IsNullOrEmpty(CategoryId))
                return false;

            // Coverage type specific validation
            if (CoverageType == "Percentage" && (CoveragePercentage == null || CoveragePercentage <= 0 || CoveragePercentage > 100))
                return false;

            if (CoverageType == "FixedAmount" && (CoverageFixedAmount == null || CoverageFixedAmount <= 0))
                return false;

            if (CoverageType == "UpToCap" && (CapAmount == null || CapAmount <= 0))
                return false;

            // Date validation
            if (EffectiveFrom.HasValue && EffectiveTo.HasValue && EffectiveFrom > EffectiveTo)
                return false;

            return true;
        }
    }
}
