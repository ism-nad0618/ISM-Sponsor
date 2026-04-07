namespace ISMSponsor.ViewModels;

/// <summary>
/// ViewModel for audit log filtering.
/// </summary>
public class AuditFilterViewModel
{
    public string? Module { get; set; }
    public string? Action { get; set; }
    public string? Actor { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SponsorId { get; set; }
    public string? ReasonCode { get; set; }
    public string? SearchTerm { get; set; }
}
