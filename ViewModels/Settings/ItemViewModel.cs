using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels.Settings;

/// <summary>
/// ViewModel for Item create/edit operations.
/// </summary>
public class ItemViewModel
{
    [Required(ErrorMessage = "Item ID is required")]
    [StringLength(50, ErrorMessage = "Item ID cannot exceed 50 characters")]
    [Display(Name = "Item ID")]
    public string ItemId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Item Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    [Display(Name = "Item Name")]
    public string ItemName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [StringLength(20, ErrorMessage = "Grade Level cannot exceed 20 characters")]
    [Display(Name = "Grade Level")]
    public string GradeLevel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency is required")]
    [StringLength(10, ErrorMessage = "Currency cannot exceed 10 characters")]
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "USD";

    [Required(ErrorMessage = "Status is required")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Active";

    [Required(ErrorMessage = "Category is required")]
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}
