# Deployment Hardening Implementation Summary

**Date:** April 8, 2026  
**Phase:** Phase 1 - Deployment Hardening  
**Status:** ✅ Complete

---

## Overview

This document summarizes the deployment hardening changes implemented to make the ISM Sponsor Management System production-ready for Azure App Service deployment. All changes focus on secure configuration, Azure Key Vault integration, safe database initialization, and comprehensive health monitoring.

---

## Changes Implemented

### 1. Azure Key Vault Integration

**Purpose:** Eliminate secrets from source control and configuration files.

**Changes:**
- ✅ Added `Azure.Extensions.AspNetCore.Configuration.Secrets` package
- ✅ Added `Azure.Identity` package for Managed Identity authentication
- ✅ Program.cs: Integrated Key Vault configuration provider using `DefaultAzureCredential`
- ✅ Key Vault loads automatically in non-Development environments
- ✅ Falls back to environment variables if Key Vault not configured (staging flexibility)

**Configuration Required:**
```json
{
  "KeyVault": {
    "Name": "ism-sponsor-pilot-kv"
  }
}
```

**Secrets to Store in Key Vault:**
- `DatabaseConnectionString` - SQL Server connection string
- `ApplicationInsights--ConnectionString` - Application Insights telemetry
- `AzureAd--ClientSecret` - Azure AD authentication secret
- `Integration--PowerSchoolApiKey` - PowerSchool API authentication
- `Integration--NetSuiteOAuthToken` - NetSuite OAuth token
- (Additional secrets as needed)

**Authentication:**
- Azure App Service: Uses **System-Assigned Managed Identity** (recommended)
- Local development: Falls back to Azure CLI or Visual Studio credentials

---

### 2. Safe Database Initialization

**Purpose:** Prevent concurrent migration conflicts and startup delays in production.

**Changes:**
- ✅ **Removed auto-migration** from `DbInitializer.Initialize()`
- ✅ Added `Database:RunMigrationsOnStartup` flag (default: false)
- ✅ Migrations run automatically **only in Development** or when explicitly enabled
- ✅ Production/Pilot: Migrations must run via deployment pipeline

**Behavior:**
- **Development:** Auto-migration enabled by default
- **Production/Pilot:** Auto-migration disabled; requires pipeline execution

**Migration Command for Pipeline:**
```bash
dotnet ef database update --connection "$CONNECTION_STRING"
```

**Configuration:**
```json
{
  "Database": {
    "RunMigrationsOnStartup": false
  }
}
```

---

### 3. Mandatory Application Insights (Non-Development)

**Purpose:** Ensure telemetry and monitoring in production/pilot.

**Changes:**
- ✅ Application Insights **required** in Production/Pilot environments
- ✅ Application startup **fails fast** if connection string missing
- ✅ ConfigurationValidationService validates Application Insights presence
- ✅ Development: Application Insights optional

**Startup Behavior:**
- **Development:** Warning logged if missing, app continues
- **Production/Pilot:** Exception thrown, app refuses to start

**Error Message:**
```
ApplicationInsights:ConnectionString is required for non-Development environments.
Configure in Azure Key Vault or App Service settings.
```

---

### 4. Granular Health Check Endpoints

**Purpose:** Support Kubernetes-style readiness and liveness probes.

**Changes:**
- ✅ Added `/health` - Overall health with detailed check results (JSON)
- ✅ Added `/health/ready` - Readiness probe (database + config + services)
- ✅ Added `/health/live` - Liveness probe (lightweight, process running)
- ✅ Health checks tagged for filtering (`ready`, `live`)

**Endpoint Descriptions:**

| Endpoint | Purpose | Checks | Use Case |
|----------|---------|--------|----------|
| `/health` | Comprehensive health status | All checks with details | Monitoring dashboards, troubleshooting |
| `/health/ready` | Readiness probe | Database, config, sync, audit | Load balancer routing, deployment readiness |
| `/health/live` | Liveness probe | Database only | Process restart decisions |

**Response Format (JSON):**
```json
{
  "status": "Healthy",
  "timestamp": "2026-04-08T12:34:56Z",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database connection successful",
      "duration": 45.2
    }
  ]
}
```

---

### 5. Enhanced Configuration Validation

**Purpose:** Fail fast on startup if configuration is invalid.

**Changes:**
- ✅ Added `ValidateMonitoring()` method
- ✅ Validates Application Insights in production/pilot
- ✅ Validates health checks are enabled
- ✅ Enhanced error messages with ✗ symbols for clarity

**Validation Rules:**
- **Database:** Connection string present and no PLACEHOLDER values
- **Azure AD:** TenantId, ClientId, ClientSecret configured in non-Development
- **Security:** HTTPS, HSTS, cookie security enforced in production/pilot
- **Monitoring:** Application Insights required in production/pilot
- **Health Checks:** Enabled in non-Development

---

### 6. HTTP Resilience Package

**Purpose:** Prepare for resilient integration adapter implementation.

**Changes:**
- ✅ Added `Microsoft.Extensions.Http.Polly` package
- ✅ Ready for retry, timeout, circuit breaker patterns (Phase 2)

**Future Use:**
```csharp
builder.Services.AddHttpClient<IPowerSchoolAdapter, RealPowerSchoolAdapter>()
    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))))
    .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

---

### 7. Environment-Specific Configuration Files

**Purpose:** Separate Development, Pilot, and Production settings.

**Changes:**
- ✅ Created `appsettings.Production.json` (token replacement syntax)
- ✅ Updated `appsettings.Pilot.json` with Key Vault reference
- ✅ Updated `.gitignore` to exclude sensitive files
- ✅ Kept `appsettings.json.example` and `appsettings.Pilot.json` as templates

**File Strategy:**
- `.gitignore` excludes: `appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`
- Source control includes: `appsettings.json.example`, `appsettings.Pilot.json` (template with tokens)

---

## Environment Variables Required

### Azure App Service Configuration

Configure these in **Azure App Service → Configuration → Application Settings**:

| Setting Name | Example Value | Source |
|--------------|---------------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Pilot` or `Production` | Manual |
| `KeyVault__Name` | `ism-sponsor-pilot-kv` | Manual |
| `ApplicationInsights__ConnectionString` | (auto-injected from Key Vault) | Key Vault |
| `ConnectionStrings__DefaultConnection` | (auto-injected from Key Vault) | Key Vault |

### Azure DevOps Pipeline Variables

Configure these in **Azure Pipelines → Library → Variable Groups**:

| Variable Name | Example Value | Secret? |
|---------------|---------------|---------|
| `KeyVault.Name` | `ism-sponsor-pilot-kv` | No |
| `AzureAd.TenantId` | `abc123...` | No |
| `AzureAd.ClientId` | `xyz789...` | No |
| `AzureAd.Domain` | `ismschool.onmicrosoft.com` | No |
| `Integration.PowerSchoolApiUrl` | `https://ps.ismmanila.org/api/v1` | No |
| `DatabaseConnectionString` | `Server=...` | **Yes** |
| `ApplicationInsights.ConnectionString` | `InstrumentationKey=...` | **Yes** |
| `AzureAd.ClientSecret` | `secret...` | **Yes** |

**Note:** Secrets should be stored in **Azure Key Vault**, not directly in pipeline variables.

---

## Azure Key Vault Setup

### 1. Create Key Vault

```bash
az keyvault create \
  --name ism-sponsor-pilot-kv \
  --resource-group ISM-Sponsor-RG \
  --location eastus
```

### 2. Grant App Service Access

```bash
# Enable Managed Identity on App Service
az webapp identity assign \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG

# Get the Managed Identity Principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG \
  --query principalId -o tsv)

# Grant Secrets Get permission
az keyvault set-policy \
  --name ism-sponsor-pilot-kv \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

### 3. Store Secrets

```bash
# Database connection string
az keyvault secret set \
  --vault-name ism-sponsor-pilot-kv \
  --name DatabaseConnectionString \
  --value "Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsorDB-Pilot;User Id=ismadmin;Password=SECURE_PASSWORD;Encrypt=True;"

# Application Insights
az keyvault secret set \
  --vault-name ism-sponsor-pilot-kv \
  --name ApplicationInsights--ConnectionString \
  --value "InstrumentationKey=abc123...;IngestionEndpoint=https://..."

# Azure AD Client Secret
az keyvault secret set \
  --vault-name ism-sponsor-pilot-kv \
  --name AzureAd--ClientSecret \
  --value "SECRET_VALUE_HERE"
```

**Note:** Secret names use `--` for nested configuration (e.g., `ApplicationInsights--ConnectionString`).

---

## Deployment Pipeline Changes Required

### azure-pipelines.yml Updates

**Add Key Vault Access Grant (before deployment):**

```yaml
- task: AzureCLI@2
  displayName: 'Grant App Service Key Vault Access'
  inputs:
    azureSubscription: 'Azure-ISM-Connection'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      # Enable Managed Identity if not already enabled
      az webapp identity assign \
        --name ism-sponsor-pilot \
        --resource-group ISM-Sponsor-RG
      
      # Get Principal ID
      PRINCIPAL_ID=$(az webapp identity show \
        --name ism-sponsor-pilot \
        --resource-group ISM-Sponsor-RG \
        --query principalId -o tsv)
      
      # Grant Key Vault access
      az keyvault set-policy \
        --name ism-sponsor-pilot-kv \
        --object-id $PRINCIPAL_ID \
        --secret-permissions get list
```

**Add Database Migration Task (before app deployment):**

```yaml
- task: AzureCLI@2
  displayName: 'Run Database Migrations'
  inputs:
    azureSubscription: 'Azure-ISM-Connection'
    scriptType: 'bash'
    scriptLocation: 'inlineScript'
    inlineScript: |
      # Get connection string from Key Vault
      CONNECTION_STRING=$(az keyvault secret show \
        --name DatabaseConnectionString \
        --vault-name ism-sponsor-pilot-kv \
        --query value -o tsv)
      
      # Run EF Core migrations
      dotnet ef database update \
        --connection "$CONNECTION_STRING" \
        --project $(Build.SourcesDirectory)/ISMSponsor.csproj
```

**Update Smoke Tests:**

```yaml
- task: PowerShell@2
  displayName: 'Run Smoke Tests (Pilot)'
  inputs:
    targetType: 'inline'
    script: |
      $baseUrl = "https://ism-sponsor-pilot-staging.azurewebsites.net"
      
      # Readiness check
      $readyResponse = Invoke-WebRequest -Uri "$baseUrl/health/ready" -UseBasicParsing
      if ($readyResponse.StatusCode -ne 200) {
        throw "Readiness check failed"
      }
      Write-Host "✓ Readiness check passed"
      
      # Liveness check
      $liveResponse = Invoke-WebRequest -Uri "$baseUrl/health/live" -UseBasicParsing
      if ($liveResponse.StatusCode -ne 200) {
        throw "Liveness check failed"
      }
      Write-Host "✓ Liveness check passed"
```

---

## Testing the Changes

### Local Development Testing

1. **Without Key Vault (Development mode):**
   ```bash
   dotnet run --environment Development
   ```
   - Should start successfully
   - Logs: "Skipping Key Vault (Development environment)"

2. **With Key Vault (Pilot simulation):**
   ```bash
   # Login to Azure CLI
   az login
   
   # Run as Pilot environment
   dotnet run --environment Pilot
   ```
   - Should load secrets from Key Vault
   - Logs: "Azure Key Vault configuration loaded from https://..."

### Azure App Service Testing

1. **Deploy to staging slot**
2. **Check health endpoints:**
   ```bash
   curl https://ism-sponsor-pilot-staging.azurewebsites.net/health/ready
   curl https://ism-sponsor-pilot-staging.azurewebsites.net/health/live
   ```
3. **Verify Application Insights telemetry in Azure Portal**
4. **Check application logs:**
   ```bash
   az webapp log tail --name ism-sponsor-pilot --resource-group ISM-Sponsor-RG --slot staging
   ```

---

## Rollback Strategy

### If Deployment Fails

1. **Slot Swap Rollback:**
   ```bash
   az webapp deployment slot swap \
     --name ism-sponsor-pilot \
     --resource-group ISM-Sponsor-RG \
     --slot staging \
     --target-slot production
   ```

2. **Revert Code Changes:**
   ```bash
   git revert HEAD
   git push
   ```
   - Pipeline will automatically redeploy previous version

3. **Database Rollback (if migrations applied):**
   ```bash
   # Identify last good migration
   dotnet ef migrations list
   
   # Rollback to previous migration
   dotnet ef database update PreviousMigrationName --connection "$CONNECTION_STRING"
   ```

### Disable Auto-Migration (Emergency)

If auto-migration causes issues before pipeline fix:

**Azure Portal → App Service → Configuration → Application Settings:**
```json
{
  "Database__RunMigrationsOnStartup": "false"
}
```

Restart app service.

---

## Validation Checklist

Before promoting to production:

- [ ] ✅ Key Vault created and populated with secrets
- [ ] ✅ App Service Managed Identity assigned
- [ ] ✅ Key Vault access policy granted to App Service
- [ ] ✅ Application starts successfully (check logs for "Configuration validation passed")
- [ ] ✅ `/health/ready` returns 200 OK with "Healthy" status
- [ ] ✅ `/health/live` returns 200 OK
- [ ] ✅ Application Insights receiving telemetry (check Azure Portal)
- [ ] ✅ Database connection successful (check health endpoint)
- [ ] ✅ No auto-migration on startup (check logs for "production-safe mode")
- [ ] ✅ HTTPS enforced (try HTTP, should redirect)
- [ ] ✅ Security headers present (check with `curl -I`)
- [ ] ✅ Login page accessible
- [ ] ✅ Admin can log in successfully

---

## Known Limitations

1. **Key Vault Caching:** Configuration values from Key Vault are cached. Restart app service after changing secrets.
2. **Managed Identity Delay:** After assigning Managed Identity, wait 1-2 minutes for propagation before granting Key Vault access.
3. **Migration Conflicts:** If multiple pipeline runs occur simultaneously, database migration may conflict. Use deployment locks or serialization.

---

## Next Steps (Phase 2)

1. **Implement Real Integration Adapters** (replace mocks)
2. **Add Polly Resilience Patterns** (retry, circuit breaker)
3. **Add Swagger/OpenAPI Documentation**
4. **Configure Azure API Management**
5. **Set up Application Insights Alerts**

---

## References

- [Azure Key Vault Configuration Provider](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- [Managed Identities for Azure Resources](https://learn.microsoft.com/en-us/azure/active-identity/managed-identities-azure-resources/)
- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

---

**Implementation Complete:** All deployment hardening changes have been successfully implemented and are ready for Azure deployment.
