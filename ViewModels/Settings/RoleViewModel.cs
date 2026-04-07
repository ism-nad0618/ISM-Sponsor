using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels.Settings;

/// <summary>
/// ViewModel for Role create/edit operations.
/// </summary>
public class RoleViewModel
{
    [Display(Name = "Role ID")]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Role Name is required")]
    [StringLength(256, ErrorMessage = "Role name cannot exceed 256 characters")]
    [Display(Name = "Role Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }
}
