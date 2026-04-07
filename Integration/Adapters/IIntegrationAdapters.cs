using ISMSponsor.Integration.Contracts;

namespace ISMSponsor.Integration.Adapters;

/// <summary>
/// Interface for PowerSchool integration adapter.
/// Syncs sponsor and student tagging information.
/// </summary>
public interface IPowerSchoolAdapter
{
    Task<SyncResult> SyncSponsorAsync(PowerSchoolSponsorDto sponsor, string eventType);
    Task<SyncResult> SyncStudentTagsAsync(string studentId, string sponsorId);
    Task<SyncResult> RemoveSponsorTagAsync(string studentId, string sponsorId);
    
    /// <summary>
    /// Publishes the sponsor master list to PowerSchool's Sponsor_OrgName custom field popup menu.
    /// Updates the "Data for Popup or Radio Buttons" configuration for the custom field.
    /// </summary>
    /// <param name="sponsorNames">Sorted, sanitized list of active sponsor names</param>
    /// <param name="correlationId">Correlation ID for tracking this publish operation</param>
    /// <returns>Result indicating success/failure of the publish operation</returns>
    Task<SyncResult> PublishSponsorOrgListAsync(List<string> sponsorNames, string correlationId);
}

/// <summary>
/// Interface for Student Charging Portal integration adapter.
/// Syncs coverage rules and sponsor references.
/// </summary>
public interface IStudentChargingPortalAdapter
{
    Task<SyncResult> SyncSponsorAsync(StudentChargingPortalSponsorDto sponsor, string eventType);
    Task<SyncResult> SyncCoverageRulesAsync(string sponsorId, List<CoverageRuleDto> rules);
    Task<SyncResult> UpdateCoverageStatusAsync(string sponsorId, bool isActive);
}

/// <summary>
/// Interface for NetSuite integration adapter.
/// Syncs Bill-To information and sponsor allocation.
/// </summary>
public interface INetSuiteAdapter
{
    Task<SyncResult> SyncSponsorAsync(NetSuiteSponsorDto sponsor, string eventType);
    Task<SyncResult> UpdateBillingAllocationAsync(string sponsorId, decimal allocationAmount);
    Task<SyncResult> SyncPaymentTermsAsync(string sponsorId, string paymentTerms);
}

/// <summary>
/// Interface for Online Billing System integration adapter.
/// Syncs statement-facing sponsor and coverage information.
/// </summary>
public interface IOnlineBillingSystemAdapter
{
    Task<SyncResult> SyncSponsorAsync(OnlineBillingSponsorDto sponsor, string eventType);
    Task<SyncResult> SyncCoveredStudentsAsync(string sponsorId, List<string> studentIds);
    Task<SyncResult> UpdateStatementPreferencesAsync(string sponsorId, string email);
    
    /// <summary>
    /// Syncs sponsor account credentials to FINDB01.CompanySponsorAccount.
    /// WARNING: Handles sensitive credential data.
    /// </summary>
    Task<SyncResult> SyncSponsorAccountAsync(object accountPayload);
}

/// <summary>
/// Main integration service that coordinates sync operations.
/// </summary>
public interface IIntegrationSyncService
{
    Task<List<SyncResult>> SyncSponsorCreateAsync(string sponsorId);
    Task<List<SyncResult>> SyncSponsorUpdateAsync(string sponsorId);
    Task<List<SyncResult>> SyncSponsorMergeAsync(string survivingSponsorId, string mergedSponsorId);
    Task<List<SyncResult>> SyncLoGActivationAsync(int logId);
    Task<SyncResult> RetrySyncAsync(int syncLogId);
}
