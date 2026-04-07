namespace ISMSponsor.ViewModels;

/// <summary>
/// Base report filter and metadata for all reports.
/// </summary>
public class ReportFilterViewModel
{
    public int? SchoolYearId { get; set; }
    public string? SponsorId { get; set; }
    public string? StudentId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ExportFormat { get; set; } // CSV, PDF
}

/// <summary>
/// Admin oversight report: Sponsor Master summary.
/// </summary>
public class SponsorMasterReportViewModel
{
    public ReportFilterViewModel Filters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Summary metrics
    public int TotalSponsors { get; set; }
    public int ActiveSponsors { get; set; }
    public int InactiveSponsors { get; set; }
    public int SponsorsCreatedInPeriod { get; set; }
    public int SponsorsUpdatedInPeriod { get; set; }
    public int MergedSponsors { get; set; }
    
    // Detailed data
    public List<SponsorSummaryRow> SponsorDetails { get; set; } = new();
}

public class SponsorSummaryRow
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int ActiveLogCount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool IsSynced { get; set; }
}
public class SponsorListRow
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public int ActiveLogCount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool IsSynced { get; set; }
}
/// <summary>
/// Admin oversight report: LoG activity summary.
/// </summary>
public class LogActivityReportViewModel
{
    public ReportFilterViewModel Filters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Summary metrics
    public int TotalLoGs { get; set; }
    public int ActiveLoGs { get; set; }
    public int InactiveLoGs { get; set; }
    public int PendingApprovalLoGs { get; set; }
    public int LoGsCreatedInPeriod { get; set; }
    public int LoGsModifiedInPeriod { get; set; }
    
    // By status breakdown
    public Dictionary<string, int> LogsByStatus { get; set; } = new();
    
    // Detailed data
    public List<LogActivityRow> LogDetails { get; set; } = new();
}

public class LogActivityRow
{
    public int LogCoverageId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RuleCount { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ActivatedOn { get; set; }
    public DateTime? DeactivatedOn { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Admin oversight report: Coverage decision summary.
/// </summary>
public class CoverageDecisionReportViewModel
{
    public ReportFilterViewModel Filters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Summary metrics
    public int TotalEvaluations { get; set; }
    public int CoveredCount { get; set; }
    public int SplitCount { get; set; }
    public int NotCoveredCount { get; set; }
    public int ErrorCount { get; set; }
    
    // Detailed data
    public List<CoverageDecisionRow> DecisionDetails { get; set; } = new();
}

public class CoverageDecisionRow
{
    public int AuditId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public decimal ItemAmount { get; set; }
    public string Decision { get; set; } = string.Empty;
    public decimal CoveragePercentage { get; set; }
    public decimal CoveredAmount { get; set; }
    public DateTime EvaluatedAt { get; set; }
}

/// <summary>
/// Admin oversight report: Sync status summary.
/// </summary>
public class SyncStatusReportViewModel
{
    public ReportFilterViewModel Filters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Summary metrics
    public int TotalSyncAttempts { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public decimal SuccessRate { get; set; }
    
    // By system breakdown
    public Dictionary<string, SyncSystemStats> SyncBySystem { get; set; } = new();
    
    // Recent failures
    public List<SyncFailureRow> RecentFailures { get; set; } = new();
}

public class SyncSystemStats
{
    public string SystemName { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int FailedSyncs { get; set; }
    public decimal SuccessRate { get; set; }
    public DateTime? LastSuccessfulSyncAt { get; set; }
    public DateTime? LastFailedSyncAt { get; set; }
}

public class SyncFailureRow
{
    public int SyncLogId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// Admin oversight report: Audit activity summary.
/// </summary>
public class AuditActivityReportViewModel
{
    public ReportFilterViewModel Filters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Summary metrics
    public int TotalAuditEntries { get; set; }
    public int UserActions { get; set; }
    public int SystemActions { get; set; }
    public int SecurityEvents { get; set; }
    
    // By module breakdown
    public Dictionary<string, int> AuditByModule { get; set; } = new();
    
    // By action breakdown
    public Dictionary<string, int> AuditByAction { get; set; } = new();
    
    // Recent critical events
    public List<AuditActivityRow> RecentEvents { get; set; } = new();
}

public class AuditActivityRow
{
    public int ActivityLogId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserDisplay { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Admissions tracking report: Workflow status.
/// </summary>
public class AdmissionsTrackingReportViewModel
{
    public ReportFilterViewModel Filters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Pending work
    public int PendingSponsorRequests { get; set; }
    public int PendingLoGReviews { get; set; }
    
    // Recent activity
    public List<SponsorRequestRow> RecentSponsorRequests { get; set; } = new();
    public List<RecentSponsorRow> RecentlyAddedSponsors { get; set; } = new();
    public List<RecentLogRow> RecentlyChangedLoGs { get; set; } = new();
    
    // Coverage alignment
    public List<CoverageAlignmentRow> CoverageAlignment { get; set; } = new();
}

public class SponsorRequestRow
{
    public int RequestId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedOn { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
}

public class RecentSponsorRow
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public bool IsSynced { get; set; }
}

public class RecentLogRow
{
    public int LogCoverageId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ModifiedOn { get; set; }
    public int RuleCount { get; set; }
}

public class CoverageAlignmentRow
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int ActiveLoGCount { get; set; }
    public int StudentsWithCoverage { get; set; }
    public int StudentsWithoutCoverage { get; set; }
    public decimal CoverageCompleteness { get; set; }
}

/// <summary>
/// Cashier reconciliation support report.
/// </summary>
public class CashierReconciliationReportViewModel
{
    public ReportFilterViewModel Filters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Recent decisions
    public List<RecentCoverageDecisionRow> RecentDecisions { get; set; } = new();
    
    // LoG status by student
    public List<StudentLogStatusRow> StudentLogStatus { get; set; } = new();
    
    // Exception list
    public List<CoverageExceptionRow> CoverageExceptions { get; set; } = new();
}

public class RecentCoverageDecisionRow
{
    public int AuditId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public decimal ItemAmount { get; set; }
    public string Decision { get; set; } = string.Empty;
    public decimal CoveredAmount { get; set; }
    public DateTime EvaluatedAt { get; set; }
}

public class StudentLogStatusRow
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public int ActiveLoGCount { get; set; }
    public int InactiveLoGCount { get; set; }
    public string? MostRecentLogStatus { get; set; }
    public DateTime? LastEvaluationDate { get; set; }
}

public class CoverageExceptionRow
{
    public string StudentId { get; set; } = string.Empty;
    public string SponsorId { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
}
/// <summary>
/// All Sponsor Contacts report for admin and cashier roles.
/// </summary>
public class AllContactsViewModel
{
    public List<SponsorContactRow> Contacts { get; set; } = new();
    public int TotalContacts { get; set; }
    public int ActiveContacts { get; set; }
    public int InactiveContacts { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class SponsorContactRow
{
    public int SponsorContactId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}