namespace ISMSponsor.Integration.Contracts;

/// <summary>
/// Common sync request for all external systems.
/// </summary>
public class SyncRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Common sync result for all external systems.
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? ExternalReferenceId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Sponsor data contract for PowerSchool sync.
/// </summary>
public class PowerSchoolSponsorDto
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? PowerSchoolId { get; set; }
    public List<string> StudentIds { get; set; } = new();
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Sponsor data contract for Student Charging Portal sync.
/// </summary>
public class StudentChargingPortalSponsorDto
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string? StudentChargingPortalId { get; set; }
    public List<CoverageRuleDto> CoverageRules { get; set; } = new();
    public bool IsActive { get; set; }
}

public class CoverageRuleDto
{
    public string ItemId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
}

/// <summary>
/// Sponsor data contract for NetSuite sync.
/// </summary>
public class NetSuiteSponsorDto
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Tin { get; set; }
    public string? NetSuiteId { get; set; }
    public string? BillingAddress { get; set; }
    public string? PaymentTerms { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Sponsor data contract for Online Billing System sync.
/// </summary>
public class OnlineBillingSponsorDto
{
    public string SponsorId { get; set; } = string.Empty;
    public string SponsorName { get; set; } = string.Empty;
    public string? OnlineBillingSystemId { get; set; }
    public string? StatementEmail { get; set; }
    public List<string> CoveredStudentIds { get; set; } = new();
    public bool IsActive { get; set; }
}
