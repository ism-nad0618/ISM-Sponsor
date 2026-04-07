using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels.Settings;

/// <summary>
/// ViewModel for User create/edit operations.
/// </summary>
public class UserViewModel
{
    [Display(Name = "User ID")]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, ErrorMessage = "Username cannot exceed 256 characters")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display Name is required")]
    [StringLength(200, ErrorMessage = "Display Name cannot exceed 200 characters")]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    [StringLength(450)]
    [Display(Name = "Sponsor ID (for sponsor users)")]
    public string? SponsorId { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [Display(Name = "Password (leave blank to keep current)")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirmation do not match")]
    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }
}
