using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels
{
    public class ChangeRequestViewModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        [Required]
        public string NewValue { get; set; } = string.Empty;
    }
}