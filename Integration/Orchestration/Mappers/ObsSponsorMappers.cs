using ISMSponsor.Models.Domain;
using ISMSponsor.Models;
using Microsoft.AspNetCore.Identity;

namespace ISMSponsor.Integration.Orchestration.Mappers;

/// <summary>
/// Maps Sponsor entity to Online Billing System FINDB01/CompanySponsors table format.
/// </summary>
public class ObsCompanySponsorMapper
{
    /// <summary>
    /// Maps sponsor master data to OBS CompanySponsors table.
    /// </summary>
    public ObsCompanySponsorPayload MapToObsCompanySponsor(Sponsor sponsor)
    {
        return new ObsCompanySponsorPayload
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
            OnlineBillingSystemId = sponsor.OnlineBillingSystemId
        };
    }
}

/// <summary>
/// Maps Sponsor entity + User account to OBS FINDB01/CompanySponsorAccount table format.
/// Handles sponsor login credentials for OBS portal access.
/// </summary>
public class ObsCompanySponsorAccountMapper
{
    private readonly UserManager<ApplicationUser> _userManager;
    
    public ObsCompanySponsorAccountMapper(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    /// <summary>
    /// Maps sponsor account credentials to OBS CompanySponsorAccount table.
    /// WARNING: This handles sensitive credential data. Ensure proper security measures.
    /// </summary>
    public async Task<ObsCompanySponsorAccountPayload> MapToObsCompanySponsorAccountAsync(
        Sponsor sponsor, 
        string? username = null)
    {
        // Find associated user account
        ApplicationUser? user = null;
        if (!string.IsNullOrEmpty(username))
        {
            user = await _userManager.FindByNameAsync(username);
        }
        else
        {
            // Try to find by SponsorId
            user = (await _userManager.GetUsersInRoleAsync("sponsor"))
                .FirstOrDefault(u => u.SponsorId == sponsor.SponsorId);
        }
        
        return new ObsCompanySponsorAccountPayload
        {
            SponsorId = sponsor.SponsorId, // Foreign key to CompanySponsors
            Username = user?.UserName ?? sponsor.SponsorId, // Default to SponsorId if no user
            DisplayName = user?.DisplayName ?? sponsor.SponsorName,
            Email = user?.Email ?? string.Empty,
            IsActive = sponsor.IsActive && user != null,
            AccountCreatedDate = DateTime.UtcNow, // Use current time as fallback
            LastSyncDate = DateTime.UtcNow,
            SourceSystem = "ISM_Sponsor_Management",
            // NOTE: Password handling
            // OBS may require password hash or plaintext (legacy system compatibility)
            // Current approach: Let OBS service handle password retrieval/sync
            // Security: Password hashes are NOT exposed through this mapper
            // If OBS requires password sync, implement in dedicated secure service method
            PasswordSyncRequired = user != null,
            UserId = user?.Id
        };
    }
}

/// <summary>
/// OBS CompanySponsors table payload (sponsor master data).
/// Target: FINDB01.CompanySponsors
/// </summary>
public class ObsCompanySponsorPayload
{
    public string SponsorId { get; set; } = string.Empty; // Primary key
    public string SponsorName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TIN { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public DateTime LastSyncDate { get; set; }
    public string? OnlineBillingSystemId { get; set; } // OBS internal ID
}

/// <summary>
/// OBS CompanySponsorAccount table payload (sponsor login/account data).
/// Target: FINDB01.CompanySponsorAccount
/// </summary>
public class ObsCompanySponsorAccountPayload
{
    public string SponsorId { get; set; } = string.Empty; // Foreign key
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime AccountCreatedDate { get; set; }
    public DateTime LastSyncDate { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    
    // Password handling fields (for secure service layer)
    public bool PasswordSyncRequired { get; set; }
    public string? UserId { get; set; } // For password retrieval if needed
}
