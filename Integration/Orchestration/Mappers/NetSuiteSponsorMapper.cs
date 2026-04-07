using ISMSponsor.Models.Domain;

namespace ISMSponsor.Integration.Orchestration.Mappers;

/// <summary>
/// Maps Sponsor entity to NetSuite Sponsors List custom record format.
/// </summary>
public class NetSuiteSponsorMapper
{
    /// <summary>
    /// Maps sponsor data to NetSuite payload format.
    /// </summary>
    public NetSuiteSponsorPayload MapToNetSuite(Sponsor sponsor)
    {
        return new NetSuiteSponsorPayload
        {
            ExternalId = sponsor.SponsorId, // Primary key for upsert
            SponsorCode = sponsor.SponsorId,
            SponsorName = sponsor.SponsorName,
            LegalName = sponsor.LegalName,
            TaxId = sponsor.Tin,
            Address = sponsor.Address ?? string.Empty,
            IsActive = sponsor.IsActive,
            IsApproved = sponsor.ApprovalStatus == "Approved" || sponsor.ApprovalStatus == null,
            SourceSystem = "ISM_Sponsor_Management",
            LastSyncDate = DateTime.UtcNow,
            NetSuiteId = sponsor.NetSuiteId // Existing NetSuite ID if already synced
        };
    }
}

/// <summary>
/// NetSuite Sponsors List payload structure.
/// </summary>
public class NetSuiteSponsorPayload
{
    public string ExternalId { get; set; } = string.Empty; // Upsert key
    public string SponsorCode { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public DateTime LastSyncDate { get; set; }
    public string? NetSuiteId { get; set; } // NetSuite internal ID after creation
}
