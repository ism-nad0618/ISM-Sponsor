using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels.Settings;

/// <summary>
/// ViewModel for School Year create/edit operations.
/// </summary>
public class SchoolYearViewModel
{
    [Required(ErrorMessage = "School Year ID is required")]
    [StringLength(10, ErrorMessage = "School Year ID cannot exceed 10 characters")]
    [Display(Name = "School Year ID")]
    public string SchoolYearId { get; set; } = string.Empty;

    [Required(ErrorMessage = "School Year Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "School Year Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valid From date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Valid From")]
    public DateTime ValidFrom { get; set; }

    [Required(ErrorMessage = "Valid To date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Valid To")]
    public DateTime ValidTo { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
