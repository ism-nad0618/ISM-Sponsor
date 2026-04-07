namespace ISMSponsor.Integration.Orchestration;

/// <summary>
/// Target system identifiers for downstream integration.
/// </summary>
public static class IntegrationTargets
{
    public const string PowerSchool = "PowerSchool";
    public const string NetSuite = "NetSuite";
    public const string OnlineBillingSystem = "OnlineBillingSystem";
    public const string StudentChargingPortal = "StudentChargingPortal";
}

/// <summary>
/// Target entity/table identifiers for detailed tracking.
/// </summary>
public static class IntegrationTargetEntities
{
    // PowerSchool targets
    public const string PowerSchool_SponsorOrgName = "Sponsor_OrgName";
    
    // NetSuite targets
    public const string NetSuite_SponsorsList = "SponsorsList";
    
    // Online Billing System targets
    public const string OBS_CompanySponsors = "FINDB01.CompanySponsors";
    public const string OBS_CompanySponsorAccount = "FINDB01.CompanySponsorAccount";
    
    // Student Charging Portal targets
    public const string SCP_Sponsors = "SERVER64.StudentChargingPortal.Sponsors";
}

/// <summary>
/// Integration event types that trigger downstream synchronization.
/// </summary>
public enum IntegrationEventType
{
    SponsorCreate,
    SponsorUpdate,
    SponsorActivate,
    SponsorDeactivate,
    SponsorMerge,
    SponsorApprove,
    SponsorReject
}
