using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.API
{
    public class CoverageEvaluationRequest
    {
        [Required(ErrorMessage = "School year is required")]
        [MaxLength(20)]
        public string SchoolYearId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student ID is required")]
        [MaxLength(20)]
        public string StudentId { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? SponsorId { get; set; }

        public int? LogId { get; set; }

        [MaxLength(20)]
        public string? ItemId { get; set; }

        [MaxLength(20)]
        public string? CategoryId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Charge date is required")]
        public DateTime ChargeDate { get; set; }
    }
}
