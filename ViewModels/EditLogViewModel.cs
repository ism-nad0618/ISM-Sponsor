using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ISMSponsor.ViewModels
{
    public class EditLogViewModel
    {
        public int LogId { get; set; }

        [Required]
        public string SchoolYearId { get; set; } = string.Empty;

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        public string SponsorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string LogStatus { get; set; } = string.Empty;

        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Review Comments")]
        [DataType(DataType.MultilineText)]
        public string? ReviewComments { get; set; }

        [Display(Name = "Replace Attachment (Optional)")]
        public IFormFile? NewAttachment { get; set; }

        // Read-only display properties
        public string? ExistingAttachmentFileName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public DateTime? DeactivatedOn { get; set; }

        // Coverage rules
        public List<CoverageRuleViewModel> CoverageRules { get; set; } = new();
    }
}
