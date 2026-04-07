using ISMSponsor.Integration.Contracts;

namespace ISMSponsor.Integration.Adapters;

/// <summary>
/// Mock PowerSchool adapter for testing and development.
/// Returns realistic success/failure outcomes without making real API calls.
/// </summary>
public class MockPowerSchoolAdapter : IPowerSchoolAdapter
{
    private readonly Random _random = new();

    public async Task<SyncResult> SyncSponsorAsync(PowerSchoolSponsorDto sponsor, string eventType)
    {
        await Task.Delay(100); // Simulate network delay

        // 90% success rate
        if (_random.Next(100) < 90)
        {
            return new SyncResult
            {
                Success = true,
                Status = "Succeeded",
                Message = $"PowerSchool: {eventType} sponsor {sponsor.SponsorName} completed",
                ExternalReferenceId = $"PS-{sponsor.SponsorId}-{Guid.NewGuid().ToString()[..8]}",
                ProcessedAt = DateTime.UtcNow
            };
        }

        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorCode = "PS_ERR_001",
            ErrorMessage = "PowerSchool API temporarily unavailable",
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<SyncResult> SyncStudentTagsAsync(string studentId, string sponsorId)
    {
        await Task.Delay(50);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"PowerSchool: Tagged student {studentId} with sponsor {sponsorId}"
        };
    }

    public async Task<SyncResult> RemoveSponsorTagAsync(string studentId, string sponsorId)
    {
        await Task.Delay(50);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"PowerSchool: Removed sponsor {sponsorId} tag from student {studentId}"
        };
    }

    public async Task<SyncResult> PublishSponsorOrgListAsync(List<string> sponsorNames, string correlationId)
    {
        await Task.Delay(200); // Simulate longer operation for list publish

        // 95% success rate for list publish (more reliable than individual syncs)
        if (_random.Next(100) < 95)
        {
            return new SyncResult
            {
                Success = true,
                Status = "Succeeded",
                Message = $"PowerSchool: Published {sponsorNames.Count} sponsor names to Sponsor_OrgName custom field popup menu",
                ExternalReferenceId = $"PS-ORGLIST-{DateTime.UtcNow:yyyyMMddHHmmss}",
                ProcessedAt = DateTime.UtcNow
            };
        }

        // Simulate occasional failures
        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorCode = "PS_LIST_ERR_001",
            ErrorMessage = "PowerSchool Custom Field API: Unable to update Sponsor_OrgName field data. CHECK: Field exists and API has write permissions.",
            ProcessedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Mock Student Charging Portal adapter.
/// </summary>
public class MockStudentChargingPortalAdapter : IStudentChargingPortalAdapter
{
    private readonly Random _random = new();

    public async Task<SyncResult> SyncSponsorAsync(StudentChargingPortalSponsorDto sponsor, string eventType)
    {
        await Task.Delay(100);

        if (_random.Next(100) < 95)
        {
            return new SyncResult
            {
                Success = true,
                Status = "Succeeded",
                Message = $"SCP: {eventType} sponsor {sponsor.SponsorName} with {sponsor.CoverageRules.Count} rules",
                ExternalReferenceId = $"SCP-{sponsor.SponsorId}",
                ProcessedAt = DateTime.UtcNow
            };
        }

        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorCode = "SCP_ERR_002",
            ErrorMessage = "Student Charging Portal database connection timeout",
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<SyncResult> SyncCoverageRulesAsync(string sponsorId, List<CoverageRuleDto> rules)
    {
        await Task.Delay(75);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"SCP: Synced {rules.Count} coverage rules for sponsor {sponsorId}"
        };
    }

    public async Task<SyncResult> UpdateCoverageStatusAsync(string sponsorId, bool isActive)
    {
        await Task.Delay(50);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"SCP: Updated coverage status to {(isActive ? "Active" : "Inactive")} for sponsor {sponsorId}"
        };
    }
}

/// <summary>
/// Mock NetSuite adapter.
/// </summary>
public class MockNetSuiteAdapter : INetSuiteAdapter
{
    private readonly Random _random = new();

    public async Task<SyncResult> SyncSponsorAsync(NetSuiteSponsorDto sponsor, string eventType)
    {
        await Task.Delay(150);

        if (_random.Next(100) < 85)
        {
            return new SyncResult
            {
                Success = true,
                Status = "Succeeded",
                Message = $"NetSuite: {eventType} customer {sponsor.LegalName ?? sponsor.SponsorName}",
                ExternalReferenceId = $"NS-CUST-{_random.Next(10000, 99999)}",
                ProcessedAt = DateTime.UtcNow
            };
        }

        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorCode = "NS_ERR_003",
            ErrorMessage = "NetSuite REST API rate limit exceeded",
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<SyncResult> UpdateBillingAllocationAsync(string sponsorId, decimal allocationAmount)
    {
        await Task.Delay(100);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"NetSuite: Updated billing allocation ${allocationAmount:N2} for sponsor {sponsorId}"
        };
    }

    public async Task<SyncResult> SyncPaymentTermsAsync(string sponsorId, string paymentTerms)
    {
        await Task.Delay(75);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"NetSuite: Updated payment terms to '{paymentTerms}' for sponsor {sponsorId}"
        };
    }
}

/// <summary>
/// Mock Online Billing System adapter.
/// </summary>
public class MockOnlineBillingSystemAdapter : IOnlineBillingSystemAdapter
{
    private readonly Random _random = new();

    public async Task<SyncResult> SyncSponsorAsync(OnlineBillingSponsorDto sponsor, string eventType)
    {
        await Task.Delay(100);

        if (_random.Next(100) < 92)
        {
            return new SyncResult
            {
                Success = true,
                Status = "Succeeded",
                Message = $"OBS: {eventType} sponsor {sponsor.SponsorName}, covering {sponsor.CoveredStudentIds.Count} students",
                ExternalReferenceId = $"OBS-{sponsor.SponsorId}",
                ProcessedAt = DateTime.UtcNow
            };
        }

        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorCode = "OBS_ERR_004",
            ErrorMessage = "Online Billing System email service unavailable",
            ProcessedAt = DateTime.UtcNow
        };
    }

    public async Task<SyncResult> SyncCoveredStudentsAsync(string sponsorId, List<string> studentIds)
    {
        await Task.Delay(100);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"OBS: Synced {studentIds.Count} covered students for sponsor {sponsorId}"
        };
    }

    public async Task<SyncResult> UpdateStatementPreferencesAsync(string sponsorId, string email)
    {
        await Task.Delay(50);
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = $"OBS: Updated statement email to {email} for sponsor {sponsorId}"
        };
    }

    public async Task<SyncResult> SyncSponsorAccountAsync(object accountPayload)
    {
        await Task.Delay(100); // Simulate network + database write

        // 90% success rate for account sync
        if (_random.Next(100) < 90)
        {
            return new SyncResult
            {
                Success = true,
                Status = "Succeeded",
                Message = $"OBS: Synced sponsor account credentials to FINDB01.CompanySponsorAccount",
                ExternalReferenceId = $"OBS-ACCT-{Guid.NewGuid().ToString()[..8]}",
                ProcessedAt = DateTime.UtcNow
            };
        }

        return new SyncResult
        {
            Success = false,
            Status = "Failed",
            ErrorCode = "OBS_ACCT_ERR_001",
            ErrorMessage = "OBS CompanySponsorAccount: Database write failed. CHECK: Table exists and has proper permissions.",
            ProcessedAt = DateTime.UtcNow
        };
    }
}
