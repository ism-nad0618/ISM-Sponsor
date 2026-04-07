using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ISMSponsor.ViewModels
{
    public class CreateLogViewModel
    {
        [Required(ErrorMessage = "School Year is required")]
        [Display(Name = "School Year")]
        public string SchoolYearId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student ID is required")]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sponsor is required")]
        [Display(Name = "Sponsor")]
        public string SponsorId { get; set; } = string.Empty;

        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Attachment (Optional)")]
        public IFormFile? Attachment { get; set; }

        // Coverage rules
        public List<CoverageRuleViewModel> CoverageRules { get; set; } = new();
    }
}
