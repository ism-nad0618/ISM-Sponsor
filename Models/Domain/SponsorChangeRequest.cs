using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.Domain
{
    public class SponsorChangeRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        [MaxLength(450)]
        public string SponsorId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RequestField { get; set; } = string.Empty; // Store enum as string

        [MaxLength(500)]
        public string? CurrentValue { get; set; }

        [Required]
        [MaxLength(500)]
        public string RequestedValue { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? RequestReason { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Store enum as string

        // Requester information
        [Required]
        [MaxLength(450)]
        public string SubmittedByUserId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? SubmittedByUserDisplay { get; set; }

        [Required]
        public DateTime SubmittedOn { get; set; }

        // Reviewer information
        [MaxLength(450)]
        public string? ReviewedByUserId { get; set; }

        [MaxLength(100)]
        public string? ReviewedByUserDisplay { get; set; }

        public DateTime? ReviewedOn { get; set; }

        [MaxLength(1000)]
        public string? ReviewNotes { get; set; }

        // Application information
        [MaxLength(450)]
        public string? AppliedByUserId { get; set; }

        [MaxLength(100)]
        public string? AppliedByUserDisplay { get; set; }

        public DateTime? AppliedOn { get; set; }

        [MaxLength(500)]
        public string? AppliedValue { get; set; }

        // Navigation properties
        public virtual Sponsor? Sponsor { get; set; }
        public virtual ApplicationUser? SubmittedByUser { get; set; }
        public virtual ApplicationUser? ReviewedByUser { get; set; }
        public virtual ApplicationUser? AppliedByUser { get; set; }
    }
}
