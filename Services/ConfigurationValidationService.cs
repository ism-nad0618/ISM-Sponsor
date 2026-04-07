namespace ISMSponsor.Services;

/// <summary>
/// Validates application configuration on startup to fail fast if required settings are missing or invalid.
/// Implements security principle: validate configuration before accepting traffic.
/// </summary>
public class ConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly List<string> _errors = new();

    public ConfigurationValidationService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// Validates all required configuration settings.
    /// Throws exception with detailed errors if validation fails.
    /// </summary>
    public void ValidateConfiguration()
    {
        ValidateDatabaseConnection();
        ValidateAuthenticationSettings();
        ValidateSecuritySettings();
        ValidateIntegrationEndpoints();

        if (_errors.Any())
        {
            var errorMessage = "Configuration validation failed:\n" + string.Join("\n", _errors.Select(e => $"  - {e}"));
            throw new InvalidOperationException(errorMessage);
        }
    }

    private void ValidateDatabaseConnection()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _errors.Add("Database connection string 'DefaultConnection' is missing or empty");
        }
        else if (connectionString.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase))
        {
            _errors.Add("Database connection string contains placeholder values");
        }
    }

    private void ValidateAuthenticationSettings()
    {
        // Azure AD validation for non-development environments
        if (!_environment.IsDevelopment())
        {
            var tenantId = _configuration["AzureAd:TenantId"];
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];

            if (string.IsNullOrWhiteSpace(tenantId) || tenantId.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase))
            {
                _errors.Add("AzureAd:TenantId is missing or contains placeholder value");
            }

            if (string.IsNullOrWhiteSpace(clientId) || clientId.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase))
            {
                _errors.Add("AzureAd:ClientId is missing or contains placeholder value");
            }

            if (string.IsNullOrWhiteSpace(clientSecret) || clientSecret.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase))
            {
                _errors.Add("AzureAd:ClientSecret is missing or contains placeholder value (use Azure Key Vault)");
            }
        }

        // Sponsor auth validation
        var lockoutMinutes = _configuration.GetValue<int>("SponsorAuth:LockoutSettings:LockoutDurationMinutes");
        if (lockoutMinutes < 5)
        {
            _errors.Add("SponsorAuth:LockoutSettings:LockoutDurationMinutes must be at least 5 minutes");
        }

        var passwordLength = _configuration.GetValue<int>("SponsorAuth:PasswordRequirements:RequiredLength");
        if (passwordLength < 8)
        {
            _errors.Add("SponsorAuth:PasswordRequirements:RequiredLength must be at least 8 characters");
        }
    }

    private void ValidateSecuritySettings()
    {
        // Production security requirements
        if (_environment.IsProduction() || _environment.EnvironmentName == "Pilot")
        {
            var useHttpsRedirection = _configuration.GetValue<bool>("Security:UseHttpsRedirection");
            if (!useHttpsRedirection)
            {
                _errors.Add("Security:UseHttpsRedirection must be true in production/pilot environments");
            }

            var useHsts = _configuration.GetValue<bool>("Security:UseHsts");
            if (!useHsts)
            {
                _errors.Add("Security:UseHsts must be true in production/pilot environments");
            }

            var cookiePolicy = _configuration["Security:CookieSecurePolicy"];
            if (cookiePolicy != "Always")
            {
                _errors.Add("Security:CookieSecurePolicy must be 'Always' in production/pilot environments");
            }
        }

        var antiForgeryEnabled = _configuration.GetValue<bool>("Security:AntiForgeryEnabled");
        if (!antiForgeryEnabled)
        {
            _errors.Add("Security:AntiForgeryEnabled must be true (CSRF protection required)");
        }
    }

    private void ValidateIntegrationEndpoints()
    {
        var syncEnabled = _configuration.GetValue<bool>("IntegrationEndpoints:SyncEnabled");
        
        if (syncEnabled)
        {
            var powerSchoolUrl = _configuration["IntegrationEndpoints:PowerSchoolApiUrl"];
            var scpUrl = _configuration["IntegrationEndpoints:StudentChargingPortalApiUrl"];
            var netSuiteUrl = _configuration["IntegrationEndpoints:NetSuiteApiUrl"];
            var obsUrl = _configuration["IntegrationEndpoints:OnlineBillingSystemApiUrl"];

            if (string.IsNullOrWhiteSpace(powerSchoolUrl))
            {
                _errors.Add("IntegrationEndpoints:PowerSchoolApiUrl is required when sync is enabled");
            }

            if (string.IsNullOrWhiteSpace(scpUrl))
            {
                _errors.Add("IntegrationEndpoints:StudentChargingPortalApiUrl is required when sync is enabled");
            }

            if (string.IsNullOrWhiteSpace(netSuiteUrl))
            {
                _errors.Add("IntegrationEndpoints:NetSuiteApiUrl is required when sync is enabled");
            }

            if (string.IsNullOrWhiteSpace(obsUrl))
            {
                _errors.Add("IntegrationEndpoints:OnlineBillingSystemApiUrl is required when sync is enabled");
            }
        }
    }

    /// <summary>
    /// Gets configuration summary for health check reporting.
    /// Does not expose sensitive values like secrets.
    /// </summary>
    public Dictionary<string, object> GetConfigurationSummary()
    {
        return new Dictionary<string, object>
        {
            ["Environment"] = _environment.EnvironmentName,
            ["AzureAdConfigured"] = !string.IsNullOrWhiteSpace(_configuration["AzureAd:ClientId"]),
            ["DatabaseConfigured"] = !string.IsNullOrWhiteSpace(_configuration.GetConnectionString("DefaultConnection")),
            ["SyncEnabled"] = _configuration.GetValue<bool>("IntegrationEndpoints:SyncEnabled"),
            ["HttpsRedirection"] = _configuration.GetValue<bool>("Security:UseHttpsRedirection"),
            ["HstsEnabled"] = _configuration.GetValue<bool>("Security:UseHsts"),
            ["AntiForgeryEnabled"] = _configuration.GetValue<bool>("Security:AntiForgeryEnabled"),
            ["HealthChecksEnabled"] = _configuration.GetValue<bool>("HealthChecks:Enabled")
        };
    }
}
