using System.ComponentModel.DataAnnotations;

namespace ISMSponsor.Models.Domain;

public class UserFeedback
{
    [Key]
    public int FeedbackId { get; set; }
    public string Category { get; set; } = string.Empty; // WorkflowClarity, ValidationMessages, MissingFields, Reporting, IntegrationReliability, Performance
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Module { get; set; } = string.Empty; // Sponsors, LoG, Coverage, Reports, Sync
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AffectedResource { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed
    public string? Resolution { get; set; }
}
