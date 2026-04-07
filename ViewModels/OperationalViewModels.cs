namespace ISMSponsor.ViewModels;

/// <summary>
/// Operational monitoring dashboard for pilot support.
/// </summary>
public class OperationalMonitoringViewModel
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Application health
    public HealthStatus ApplicationHealth { get; set; } = new();
    
    // Recent sync activity
    public List<RecentSyncAttempt> RecentSyncAttempts { get; set; } = new();
    public List<RecentSyncFailure> RecentSyncFailures { get; set; } = new();
    
    // Recent coverage evaluation activity
    public List<RecentCoverageEvaluation> RecentEvaluations { get; set; } = new();
    public List<CoverageEvaluationFailure> EvaluationFailures { get; set; } = new();
    
    // Authorization failures
    public List<AuthorizationFailureEvent> AuthorizationFailures { get; set; } = new();
    
    // Recent sponsor requests
    public List<RecentSponsorRequestEvent> RecentRequests { get; set; } = new();
    
    // Recent LoG status changes
    public List<RecentLogStatusChange> RecentLogChanges { get; set; } = new();
    
    // Data consistency warnings
    public List<DataConsistencyWarning> ConsistencyWarnings { get; set; } = new();
}

public class HealthStatus
{
    public string Status { get; set; } = "Unknown"; // Healthy, Degraded, Unhealthy
    public bool DatabaseHealthy { get; set; }
    public bool ConfigurationValid { get; set; }
    public bool SyncHealthy { get; set; }
    public bool AuditHealthy { get; set; }
    public DateTime LastChecked { get; set; }
}

public class RecentSyncAttempt
{
    public int SyncLogId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; }
    public int RetryCount { get; set; }
}

public class RecentSyncFailure
{
    public int SyncLogId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
    public int RetryCount { get; set; }
    public bool CanRetry { get; set; }
}

public class RecentCoverageEvaluation
{
    public int AuditId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; }
}

public class CoverageEvaluationFailure
{
    public string StudentId { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}

public class AuthorizationFailureEvent
{
    public int ActivityLogId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string RequiredRole { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}

public class RecentSponsorRequestEvent
{
    public int RequestId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}

public class RecentLogStatusChange
{
    public int LogCoverageId { get; set; }
    public string SponsorId { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}

public class DataConsistencyWarning
{
    public string WarningType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public DateTime DetectedAt { get; set; }
}

/// <summary>
/// Smoke test support for post-deployment verification.
/// </summary>
public class SmokeTestViewModel
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string Environment { get; set; } = "Development";
    public string Version { get; set; } = string.Empty;
    
    public List<SmokeTestItem> TestItems { get; set; } = new();
    
    // Summary
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int NotRunTests { get; set; }
}

public class SmokeTestItem
{
    public int TestId { get; set; }
    public string Category { get; set; } = string.Empty; // Authentication, Sponsor, LoG, Coverage, Sync, Audit
    public string TestName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "NotRun"; // NotRun, Pass, Fail
    public string? Notes { get; set; }
    public string? Evidence { get; set; }
    public DateTime? TestedAt { get; set; }
    public string? TestedBy { get; set; }
}

/// <summary>
/// Operational runbook support.
/// </summary>
public class RunbookViewModel
{
    public List<RunbookItem> Runbooks { get; set; } = new();
}

public class RunbookItem
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Sync, DataMismatch, Access, Deployment, Coverage
    public string Description { get; set; } = string.Empty;
    public List<string> WhatToCheck { get; set; } = new();
    public List<string> WhereToLook { get; set; } = new();
    public List<string> CommonCauses { get; set; } = new();
    public List<string> ActionSteps { get; set; } = new();
    public string? WhenToEscalate { get; set; }
}

/// <summary>
/// Release readiness and rollback support.
/// </summary>
public class ReleaseChecklist
{
    public string ReleaseName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    
    public List<ChecklistItem> PreDeploymentChecks { get; set; } = new();
    public List<ChecklistItem> DeploymentSteps { get; set; } = new();
    public List<ChecklistItem> PostDeploymentVerification { get; set; } = new();
    public List<ChecklistItem> RollbackProcedure { get; set; } = new();
}

public class ChecklistItem
{
    public string Item { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CompletedBy { get; set; }
}

/// <summary>
/// User feedback and continuous improvement support.
/// </summary>
public class FeedbackViewModel
{
    public List<FeedbackItem> RecentFeedback { get; set; } = new();
    public Dictionary<string, int> FeedbackByCategory { get; set; } = new();
    public Dictionary<string, int> FeedbackBySeverity { get; set; } = new();
}

public class FeedbackItem
{
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

public class SubmitFeedbackViewModel
{
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AffectedResource { get; set; }
}
