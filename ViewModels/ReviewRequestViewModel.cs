using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels
{
    public class ReviewRequestViewModel
    {
        public int RequestId { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty; // "Approve", "Reject", "Apply"

        [MaxLength(1000, ErrorMessage = "Review notes cannot exceed 1000 characters")]
        [Display(Name = "Review Notes")]
        public string? ReviewNotes { get; set; }
    }
}
