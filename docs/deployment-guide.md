# Deployment Guide - ISM Sponsor Management System

## Overview
This guide provides step-by-step instructions for deploying the ISM Sponsor Management System to Dev and Pilot environments.

---

## Prerequisites

### Required Tools
- Azure CLI (`az`) version 2.50+
- .NET SDK 8.0+
- Entity Framework Core CLI tools
- PowerShell 7.0+ (for smoke tests)
- Access to Azure subscription

### Required Access
- Azure DevOps project access
- Azure subscription contributor role
- Azure Key Vault secrets officer role
- SQL database administrator access

---

## Environment Setup

### 1. Azure Resources (One-Time Setup)

#### Resource Group
```bash
az group create \
  --name ISM-Sponsor-RG \
  --location eastus
```

#### App Service Plan
```bash
az appservice plan create \
  --name ISM-Sponsor-Plan \
  --resource-group ISM-Sponsor-RG \
  --sku P1V2 \
  --is-linux false
```

#### App Services (Dev)
```bash
az webapp create \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG \
  --plan ISM-Sponsor-Plan \
  --runtime "DOTNET|8.0"
```

#### App Services (Pilot with Staging Slot)
```bash
# Production slot
az webapp create \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG \
  --plan ISM-Sponsor-Plan \
  --runtime "DOTNET|8.0"

# Staging slot
az webapp deployment slot create \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG \
  --slot staging
```

#### SQL Database
```bash
# SQL Server
az sql server create \
  --name ism-sponsor-sql \
  --resource-group ISM-Sponsor-RG \
  --location eastus \
  --admin-user ismadmin \
  --admin-password [SECURE-PASSWORD]

# Database (Dev)
az sql db create \
  --name ISMSponsorDB-Dev \
  --server ism-sponsor-sql \
  --resource-group ISM-Sponsor-RG \
  --service-objective S1

# Database (Pilot)
az sql db create \
  --name ISMSponsorDB-Pilot \
  --server ism-sponsor-sql \
  --resource-group ISM-Sponsor-RG \
  --service-objective S1
```

#### Azure Key Vault
```bash
# Development Key Vault
az keyvault create \
  --name ism-sponsor-dev-kv \
  --resource-group ISM-Sponsor-RG \
  --location eastus

# Pilot Key Vault
az keyvault create \
  --name ism-sponsor-pilot-kv \
  --resource-group ISM-Sponsor-RG \
  --location eastus
```

#### Application Insights
```bash
# Create Application Insights
az monitor app-insights component create \
  --app ism-sponsor-insights \
  --resource-group ISM-Sponsor-RG \
  --location eastus \
  --application-type web
```

---

### 2. Azure Key Vault Secrets

#### Store Secrets (Dev)
```bash
# Database connection string
az keyvault secret set \
  --vault-name ism-sponsor-dev-kv \
  --name ISMSponsor-Dev-DbConnectionString \
  --value "Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsorDB-Dev;User Id=ismadmin;Password=[PASSWORD];Encrypt=True;"

# Application Insights connection string
APP_INSIGHTS_CONN=$(az monitor app-insights component show \
  --app ism-sponsor-insights \
  --resource-group ISM-Sponsor-RG \
  --query connectionString -o tsv)

az keyvault secret set \
  --vault-name ism-sponsor-dev-kv \
  --name ISMSponsor-Dev-AppInsightsConnectionString \
  --value "$APP_INSIGHTS_CONN"
```

#### Store Secrets (Pilot)
```bash
# Database connection string
az keyvault secret set \
  --vault-name ism-sponsor-pilot-kv \
  --name ISMSponsor-Pilot-DbConnectionString \
  --value "Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsorDB-Pilot;User Id=ismadmin;Password=[PASSWORD];Encrypt=True;"

# Application Insights connection string
az keyvault secret set \
  --vault-name ism-sponsor-pilot-kv \
  --name ISMSponsor-Pilot-AppInsightsConnectionString \
  --value "$APP_INSIGHTS_CONN"

# Azure AD secrets (Pilot only)
az keyvault secret set \
  --vault-name ism-sponsor-pilot-kv \
  --name ISMSponsor-Pilot-AzureAd-ClientSecret \
  --value "[AZURE_AD_CLIENT_SECRET]"
```

---

### 3. App Service Configuration

#### Configure Key Vault References (Dev)
```bash
az webapp config appsettings set \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG \
  --settings \
    ConnectionStrings__DefaultConnection="@Microsoft.KeyVault(SecretUri=https://ism-sponsor-dev-kv.vault.azure.net/secrets/ISMSponsor-Dev-DbConnectionString/)" \
    ApplicationInsights__ConnectionString="@Microsoft.KeyVault(SecretUri=https://ism-sponsor-dev-kv.vault.azure.net/secrets/ISMSponsor-Dev-AppInsightsConnectionString/)"
```

#### Configure Key Vault References (Pilot)
```bash
az webapp config appsettings set \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG \
  --settings \
    ConnectionStrings__DefaultConnection="@Microsoft.KeyVault(SecretUri=https://ism-sponsor-pilot-kv.vault.azure.net/secrets/ISMSponsor-Pilot-DbConnectionString/)" \
    ApplicationInsights__ConnectionString="@Microsoft.KeyVault(SecretUri=https://ism-sponsor-pilot-kv.vault.azure.net/secrets/ISMSponsor-Pilot-AppInsightsConnectionString/)" \
    AzureAd__ClientSecret="@Microsoft.KeyVault(SecretUri=https://ism-sponsor-pilot-kv.vault.azure.net/secrets/ISMSponsor-Pilot-AzureAd-ClientSecret/)"
```

#### Enable Managed Identity
```bash
# Dev
az webapp identity assign \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG

# Pilot
az webapp identity assign \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG

# Grant Key Vault access (Dev)
DEV_IDENTITY=$(az webapp identity show --name ism-sponsor-dev --resource-group ISM-Sponsor-RG --query principalId -o tsv)
az keyvault set-policy \
  --name ism-sponsor-dev-kv \
  --object-id $DEV_IDENTITY \
  --secret-permissions get list

# Grant Key Vault access (Pilot)
PILOT_IDENTITY=$(az webapp identity show --name ism-sponsor-pilot --resource-group ISM-Sponsor-RG --query principalId -o tsv)
az keyvault set-policy \
  --name ism-sponsor-pilot-kv \
  --object-id $PILOT_IDENTITY \
  --secret-permissions get list
```

---

## Manual Deployment (For Testing)

### 1. Build Application
```bash
cd "/path/to/ISM Sponsor"

# Clean previous builds
dotnet clean

# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release
```

### 2. Publish Application
```bash
# Publish to folder
dotnet publish ISMSponsor.csproj \
  --configuration Release \
  --output ./publish \
  --self-contained false
```

### 3. Deploy to Azure (Dev)
```bash
# Create deployment zip
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to App Service
az webapp deployment source config-zip \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG \
  --src deploy.zip
```

### 4. Run Database Migrations (Dev)
```bash
# Get connection string from Key Vault
CONNECTION_STRING=$(az keyvault secret show \
  --vault-name ism-sponsor-dev-kv \
  --name ISMSponsor-Dev-DbConnectionString \
  --query value -o tsv)

# Run migrations locally targeting Dev database
dotnet ef database update \
  --connection "$CONNECTION_STRING"
```

---

## CI/CD Pipeline Deployment (Recommended)

### 1. Configure Azure DevOps

#### Service Connection
1. Navigate to Azure DevOps → Project Settings → Service connections
2. Create new service connection: Azure Resource Manager
3. Name: `Azure-ISM-Connection`
4. Subscription: Select ISM subscription
5. Resource Group: `ISM-Sponsor-RG`
6. Grant pipeline permissions

#### Variable Groups
```yaml
# Variable Group: ISM-Sponsor-Dev
Variables:
  - AppServiceName: ism-sponsor-dev
  - ResourceGroup: ISM-Sponsor-RG
  - Environment: Development

# Variable Group: ISM-Sponsor-Pilot
Variables:
  - AppServiceName: ism-sponsor-pilot
  - ResourceGroup: ISM-Sponsor-RG
  - Environment: Pilot
```

### 2. Trigger Pipeline

#### Push to Develop Branch (Auto-Deploy to Dev)
```bash
git checkout develop
git add .
git commit -m "Deploy to Dev"
git push origin develop
```

#### Push to Main Branch (Manual Approval for Pilot)
```bash
git checkout main
git merge develop
git push origin main

# Pipeline will:
# 1. Build and test
# 2. Security scan
# 3. Deploy to Dev
# 4. Wait for manual approval
# 5. (After approval) Deploy to Pilot
```

### 3. Monitor Pipeline
```bash
# View pipeline runs
az pipelines runs list --project "ISM Sponsor" --top 5

# View specific run
az pipelines runs show --id [RUN_ID] --project "ISM Sponsor"
```

---

## Post-Deployment Steps

### 1. Verify Deployment
```bash
# Check app service status
az webapp show \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG \
  --query state -o tsv

# Expected: Running
```

### 2. Run Smoke Tests
```powershell
# Dev environment
./docs/SmokeTests.ps1 -BaseUrl "https://ism-sponsor-dev.azurewebsites.net"

# Pilot environment
./docs/SmokeTests.ps1 -BaseUrl "https://ism-sponsor-pilot.azurewebsites.net"
```

### 3. Check Application Logs
```bash
# Stream logs (Dev)
az webapp log tail \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG

# Download logs
az webapp log download \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG \
  --log-file dev-logs.zip
```

### 4. Verify Health Endpoints
```bash
# Basic health check
curl https://ism-sponsor-dev.azurewebsites.net/health

# Detailed health check (requires admin authentication)
curl https://ism-sponsor-dev.azurewebsites.net/api/health/detailed \
  -H "Authorization: Bearer [TOKEN]"
```

### 5. Verify Security Headers
```bash
curl -I https://ism-sponsor-dev.azurewebsites.net/

# Expected headers:
# X-Content-Type-Options: nosniff
# X-Frame-Options: DENY
# Content-Security-Policy: [policy]
# Strict-Transport-Security: max-age=31536000 (Pilot only)
```

---

## Rollback Procedures

### Rollback from Pilot Staging Slot
```bash
# If smoke tests fail before swap, just delete staging deployment
az webapp deployment slot delete \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG \
  --slot staging

# Then redeploy previous version
```

### Rollback Pilot Production Slot
```bash
# Swap back to previous slot
az webapp deployment slot swap \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG \
  --slot staging \
  --action swap

# Or restore from backup
az webapp config backup restore \
  --resource-group ISM-Sponsor-RG \
  --webapp-name ism-sponsor-pilot \
  --backup-name [BACKUP_NAME]
```

### Rollback Dev Environment
```bash
# Redeploy previous version from artifacts
az webapp deployment source config-zip \
  --name ism-sponsor-dev \
  --resource-group ISM-Sponsor-RG \
  --src [previous-version].zip
```

### Database Rollback
```bash
# Restore database from backup
az sql db restore \
  --resource-group ISM-Sponsor-RG \
  --server ism-sponsor-sql \
  --name ISMSponsorDB-Pilot \
  --dest-name ISMSponsorDB-Pilot-Restored \
  --time "2026-03-09T10:00:00Z"

# Then swap connection strings
```

---

## Monitoring and Maintenance

### Application Insights Queries

#### Authentication Failures (Last 24 Hours)
```kusto
traces
| where timestamp > ago(24h)
| where message contains "Authentication failure"
| project timestamp, message, customDimensions
| order by timestamp desc
```

#### Authorization Failures
```kusto
traces
| where timestamp > ago(24h)
| where message contains "Authorization failure"
| project timestamp, user_AuthenticatedId, message
| summarize count() by user_AuthenticatedId
| order by count_ desc
```

#### Application Errors
```kusto
exceptions
| where timestamp > ago(24h)
| summarize count() by type, outerMessage
| order by count_ desc
```

### Performance Monitoring
```bash
# Check App Service metrics
az monitor metrics list \
  --resource /subscriptions/[SUB_ID]/resourceGroups/ISM-Sponsor-RG/providers/Microsoft.Web/sites/ism-sponsor-pilot \
  --metric "CpuPercentage" "MemoryPercentage" "HttpResponseTime" \
  --start-time 2026-03-10T00:00:00Z \
  --end-time 2026-03-10T23:59:59Z \
  --interval PT1H
```

### Database Maintenance
```sql
-- Check database size
SELECT 
    DB_NAME() AS DatabaseName,
    SUM(size) * 8 / 1024 AS SizeMB
FROM sys.master_files
WHERE database_id = DB_ID();

-- Check table sizes
SELECT 
    t.NAME AS TableName,
    p.rows AS RowCount,
    SUM(a.total_pages) * 8 AS TotalSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
GROUP BY t.Name, p.Rows
ORDER BY SUM(a.total_pages) DESC;
```

---

## Troubleshooting

### Issue: Configuration Validation Fails on Startup
**Symptoms**: App fails to start, logs show "Configuration validation failed"

**Solution**:
1. Check application logs for specific validation error
2. Verify Key Vault secrets are set correctly
3. Verify managed identity has Key Vault access
4. Check appsettings.[Environment].json for placeholder values

```bash
# Check Key Vault access
az keyvault secret show \
  --vault-name ism-sponsor-dev-kv \
  --name ISMSponsor-Dev-DbConnectionString
```

---

### Issue: Database Connection Fails
**Symptoms**: Health check fails, "Cannot open database" errors

**Solution**:
1. Verify SQL Server firewall allows Azure services
2. Check connection string in Key Vault
3. Verify managed identity has SQL access

```bash
# Allow Azure services
az sql server firewall-rule create \
  --resource-group ISM-Sponsor-RG \
  --server ism-sponsor-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

### Issue: Authentication Not Working
**Symptoms**: Login redirects loop, authentication errors

**Solution**:
1. Verify Azure AD configuration in Key Vault
2. Check redirect URIs in Azure AD app registration
3. Verify cookie secure policy matches HTTPS status

```bash
# Check app settings
az webapp config appsettings list \
  --name ism-sponsor-pilot \
  --resource-group ISM-Sponsor-RG \
  --query "[?name=='AzureAd__ClientId']"
```

---

## Emergency Contacts

- **On-Call Engineer**: [Phone/Email]
- **Azure Subscription Owner**: [Contact]
- **Database Administrator**: [Contact]
- **Security Team**: security@ismmanila.org
- **DevOps Team**: devops-team@ismmanila.org

---

## Version History

| Version | Date | Changes | Deployed By |
|---------|------|---------|-------------|
| 1.0.0 | 2026-03-10 | Initial release with Step 7 | DevOps Team |
| | | | |

---

**Document Owner**: ISM Technology Team  
**Last Updated**: March 2026
