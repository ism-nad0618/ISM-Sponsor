using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ISMSponsor.ViewModels
{
    public class CreateSponsorViewModel
    {
        // Sponsor Information
        [Required(ErrorMessage = "Sponsor ID is required")]
        [Display(Name = "Sponsor ID")]
        public string SponsorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sponsor Name is required")]
        [Display(Name = "Sponsor Name")]
        public string SponsorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Legal Name is required")]
        [Display(Name = "Legal Name")]
        public string LegalName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "TIN is required")]
        [Display(Name = "Tax Identification Number (TIN)")]
        public string Tin { get; set; } = string.Empty;

        // User Account Information
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display Name is required")]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Verification Attachment
        [Required(ErrorMessage = "Verification document is required")]
        [Display(Name = "Verification Document")]
        public IFormFile? VerificationDocument { get; set; }
    }
}
