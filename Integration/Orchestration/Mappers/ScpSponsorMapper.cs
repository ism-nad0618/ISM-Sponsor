using ISMSponsor.Models.Domain;

namespace ISMSponsor.Integration.Orchestration.Mappers;

/// <summary>
/// Maps Sponsor entity to Student Charging Portal SERVER64/StudentChargingPortal/Sponsors table format.
/// </summary>
public class ScpSponsorMapper
{
    /// <summary>
    /// Maps sponsor data to SCP Sponsors table.
    /// </summary>
    public ScpSponsorPayload MapToScpSponsor(Sponsor sponsor)
    {
        return new ScpSponsorPayload
        {
            SponsorId = sponsor.SponsorId, // Primary key for upsert
            SponsorName = sponsor.SponsorName,
            LegalName = sponsor.LegalName,
            TIN = sponsor.Tin,
            Address = sponsor.Address ?? string.Empty,
            IsActive = sponsor.IsActive,
            IsApproved = sponsor.ApprovalStatus == "Approved" || sponsor.ApprovalStatus == null,
            SourceSystem = "ISM_Sponsor_Management",
            LastSyncDate = DateTime.UtcNow,
            StudentChargingPortalId = sponsor.StudentChargingPortalId,
            // SCP-specific fields for charge entry support
            AllowChargeEntry = sponsor.IsActive && (sponsor.ApprovalStatus == "Approved" || sponsor.ApprovalStatus == null),
            PrimarySponsorContact = sponsor.Contacts?.FirstOrDefault(c => c.IsActive)?.Email ?? string.Empty
        };
    }
}

/// <summary>
/// Student Charging Portal Sponsors table payload.
/// Target: SERVER64.StudentChargingPortal.Sponsors
/// </summary>
public class ScpSponsorPayload
{
    public string SponsorId { get; set; } = string.Empty; // Primary key
    public string SponsorName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TIN { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public bool AllowChargeEntry { get; set; } // SCP-specific: can charges be entered?
    public string? PrimarySponsorContact { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public DateTime LastSyncDate { get; set; }
    public string? StudentChargingPortalId { get; set; } // SCP internal ID
}
