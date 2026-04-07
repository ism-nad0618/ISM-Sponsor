using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.ViewModels.Settings;

/// <summary>
/// ViewModel for Student create/edit operations.
/// </summary>
public class StudentViewModel
{
    [Required(ErrorMessage = "Student ID is required")]
    [StringLength(50, ErrorMessage = "Student ID cannot exceed 50 characters")]
    [Display(Name = "Student ID")]
    public string StudentId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Student Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    [Display(Name = "Student Name")]
    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "School Year is required")]
    [Display(Name = "School Year")]
    public string SchoolYearId { get; set; } = string.Empty;

    [StringLength(450)]
    [Display(Name = "Sponsor ID")]
    public string? SponsorId { get; set; }

    [Required(ErrorMessage = "Grade Level is required")]
    [Display(Name = "Grade Level")]
    [StringLength(20, ErrorMessage = "Grade Level cannot exceed 20 characters")]
    public string GradeLevel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Status")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    public string Status { get; set; } = "Active";

    [Display(Name = "Notes")]
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
