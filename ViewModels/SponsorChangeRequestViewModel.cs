using System.ComponentModel.DataAnnotations;
using ISMSponsor.Models.Domain;

namespace ISMSponsor.ViewModels
{
    public class SponsorChangeRequestViewModel
    {
        [Required(ErrorMessage = "Please select a field to update")]
        public SponsorRequestField RequestField { get; set; }

        [Display(Name = "Current Value")]
        public string? CurrentValue { get; set; }

        [Required(ErrorMessage = "Please enter the requested value")]
        [MaxLength(500, ErrorMessage = "Requested value cannot exceed 500 characters")]
        [Display(Name = "Requested Value")]
        public string RequestedValue { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
        [Display(Name = "Reason for Change")]
        public string? RequestReason { get; set; }

        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
    }
}
