# Azure SQL Migration Guide

## Overview
This guide walks you through migrating your ISM Sponsor database from Docker SQL Server (Mac) to Azure SQL Database.

---

## Current Setup
- **Database Engine**: SQL Server in Docker container
- **Host**: localhost:1433
- **Database**: ISMSponsor
- **Authentication**: SQL Authentication (sa user)

## Target Setup
- **Database Engine**: Azure SQL Database
- **Authentication**: Azure AD + SQL Authentication
- **Tier**: Standard S2 (recommended for production)

---

## Migration Steps

### Phase 1: Prepare Azure SQL Database

#### Step 1: Create Azure SQL Server
```bash
# Set variables
RESOURCE_GROUP="ISM-Sponsor-RG"
SQL_SERVER_NAME="ism-sponsor-sql"  # Must be globally unique
LOCATION="eastus"
ADMIN_USER="ismsponadmin"
ADMIN_PASSWORD="YourSecurePassword123!"  # Change this!

# Create SQL Server
az sql server create \
  --name $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $ADMIN_USER \
  --admin-password $ADMIN_PASSWORD
```

#### Step 2: Configure Firewall Rules
```bash
# Allow Azure services
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your current IP (for migration)
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowMyIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

#### Step 3: Create Database
```bash
# Create database (Standard S2 tier)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name ISMSponsor \
  --service-objective S2 \
  --backup-storage-redundancy Local
```

---

### Phase 2: Export Data from Docker SQL

#### Option A: Using Entity Framework (Recommended)

This approach uses EF Core migrations to create the schema, then exports/imports data.

**Step 1: Generate SQL Scripts**
```bash
cd "/Users/cruzr/Documents/ISM Sponsor"

# Generate script for all migrations
dotnet ef migrations script -o migration-script.sql
```

**Step 2: Export Data Only**
Create a PowerShell script to export data:

```powershell
# Save as export-data.ps1
$server = "localhost,1433"
$database = "ISMSponsor"
$user = "sa"
$password = "Qwerty012210"

$tables = @(
    "AspNetUsers",
    "AspNetRoles",
    "AspNetUserRoles",
    "SchoolYears",
    "Sponsors",
    "SponsorAddresses",
    "SponsorContacts",
    "Students",
    "LogCoverages",
    "LoGCoverageRules",
    "Items",
    "ItemCategories",
    "ChangeRequests",
    "ActivityLogs",
    "UserPreferences",
    "CoverageEvaluationAudits",
    "SponsorChangeRequests",
    "SponsorDuplicateCandidates",
    "MergeOperations",
    "SyncLogs",
    "UserFeedback"
)

foreach ($table in $tables) {
    $exportPath = "./data-export/$table.csv"
    bcp "$database.dbo.$table" out $exportPath -c -t, -S $server -U $user -P $password
    Write-Host "Exported $table to $exportPath"
}
```

#### Option B: Using BACPAC (Simpler but requires SQL Server tools)

**Step 1: Install SQL Server tools**
```bash
# Install mssql-tools
brew tap microsoft/mssql-release https://github.com/Microsoft/homebrew-mssql-release
brew update
brew install mssql-tools
```

**Step 2: Export database**
```bash
# Using sqlpackage
sqlpackage /Action:Export \
  /SourceServerName:localhost,1433 \
  /SourceDatabaseName:ISMSponsor \
  /SourceUser:sa \
  /SourcePassword:Qwerty012210 \
  /TargetFile:ISMSponsor.bacpac \
  /SourceTrustServerCertificate:True
```

---

### Phase 3: Import to Azure SQL

#### Using EF Migrations (Recommended)

**Step 1: Update Connection String Temporarily**

Create a new file `appsettings.AzureMigration.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsor;User Id=ismsponadmin;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"
  }
}
```

**Step 2: Apply Migrations**
```bash
# Set environment to use migration config
export ASPNETCORE_ENVIRONMENT=AzureMigration

# Apply all migrations to Azure SQL
dotnet ef database update
```

**Step 3: Import Data**
```powershell
# Save as import-data.ps1
$server = "ism-sponsor-sql.database.windows.net"
$database = "ISMSponsor"
$user = "ismsponadmin"
$password = "YourSecurePassword123!"

$tables = @(
    "AspNetRoles",
    "AspNetUsers",
    "AspNetUserRoles",
    "SchoolYears",
    "Sponsors",
    "SponsorAddresses",
    "SponsorContacts",
    "Students",
    "ItemCategories",
    "Items",
    "LoGCoverageRules",
    "LogCoverages",
    "ChangeRequests",
    "ActivityLogs",
    "UserPreferences",
    "CoverageEvaluationAudits",
    "SponsorChangeRequests",
    "SponsorDuplicateCandidates",
    "MergeOperations",
    "SyncLogs",
    "UserFeedback"
)

foreach ($table in $tables) {
    $importPath = "./data-export/$table.csv"
    if (Test-Path $importPath) {
        bcp "$database.dbo.$table" in $importPath -c -t, -S $server -U $user -P $password -q
        Write-Host "Imported $table from $importPath"
    }
}
```

#### Using BACPAC Import

```bash
# Import BACPAC to Azure SQL
sqlpackage /Action:Import \
  /SourceFile:ISMSponsor.bacpac \
  /TargetServerName:ism-sponsor-sql.database.windows.net \
  /TargetDatabaseName:ISMSponsor \
  /TargetUser:ismsponadmin \
  /TargetPassword:YourSecurePassword123! \
  /TargetTrustServerCertificate:False
```

---

### Phase 4: Update Application Configuration

#### Step 1: Store Connection String in Azure Key Vault

```bash
# Create Key Vault (if not exists)
KEY_VAULT_NAME="ism-sponsor-kv"
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Store connection string
CONNECTION_STRING="Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsor;User Id=ismsponadmin;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"

az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "DatabaseConnectionString" \
  --value "$CONNECTION_STRING"
```

#### Step 2: Update Application Settings

The Production and Pilot configs already have placeholders. Update your Azure Pipeline variables or App Service configuration:

**For Azure App Service:**
```bash
# Set connection string in App Service
az webapp config connection-string set \
  --resource-group $RESOURCE_GROUP \
  --name ism-sponsor-dev \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$CONNECTION_STRING"
```

**For Local Development with Azure SQL:**

Update `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsor;User Id=ismsponadmin;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"
  }
}
```

---

### Phase 5: Verification

#### Step 1: Test Connection
```bash
# Test using dotnet ef
dotnet ef dbcontext info --connection "Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsor;User Id=ismsponadmin;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"
```

#### Step 2: Verify Data
```bash
# Run the application locally against Azure SQL
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

Navigate to:
- https://localhost:5001/Health - Check health endpoint
- https://localhost:5001/Admin - Verify data loads correctly

#### Step 3: Run Smoke Tests
```bash
# Execute smoke tests against new database
pwsh docs/smoke-tests.md
```

---

## Security Hardening

### 1. Enable Azure AD Authentication (Recommended)

```bash
# Set Azure AD admin for SQL Server
az sql server ad-admin create \
  --resource-group $RESOURCE_GROUP \
  --server-name $SQL_SERVER_NAME \
  --display-name "ISM Admin" \
  --object-id $(az ad signed-in-user show --query id -o tsv)
```

Update connection string to use Managed Identity in production:
```
Server=ism-sponsor-sql.database.windows.net;Database=ISMSponsor;Authentication=Active Directory Default;
```

### 2. Remove Public Access (Production)

```bash
# Disable public network access (use Private Endpoint instead)
az sql server update \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --enable-public-network false
```

### 3. Enable Auditing and Threat Detection

```bash
# Create storage account for audit logs
az storage account create \
  --name ismsponsoraudit \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS

# Enable auditing
az sql server audit-policy update \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --state Enabled \
  --storage-account ismsponsoraudit

# Enable threat detection
az sql server threat-policy update \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER_NAME \
  --state Enabled \
  --storage-account ismsponsoraudit
```

---

## Cost Optimization

### Database Tier Comparison

| Tier | Specs | Use Case | Monthly Cost (Est.) |
|------|-------|----------|---------------------|
| Basic | 5 DTUs, 2GB | Development | $5 |
| S0 (Standard) | 10 DTUs, 250GB | Small workload | $15 |
| S2 (Standard) | 50 DTUs, 250GB | Pilot/Production | $75 |
| S3 (Standard) | 100 DTUs, 250GB | High traffic | $150 |
| P1 (Premium) | 125 DTUs, 500GB | Mission critical | $465 |

**Recommendation:**
- **Development**: Basic or S0
- **Pilot**: S2
- **Production**: S2 (scale to S3 if needed)

### Cost Saving Tips

1. **Auto-pause**: Enable serverless tier for dev environments
2. **Backup retention**: Reduce to 7 days for non-production
3. **Geo-replication**: Only enable for production
4. **Monitoring**: Use built-in Azure monitoring instead of third-party tools

---

## Rollback Plan

If migration fails or issues arise:

### Quick Rollback to Docker SQL

1. **Revert connection string** in `appsettings.Development.json`
2. **Restart Docker SQL container**
3. **Verify application connectivity**

### Database Restore

```bash
# Restore from point-in-time (Azure SQL)
az sql db restore \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name ISMSponsor \
  --dest-name ISMSponsor-Restored \
  --time "2026-04-21T10:00:00Z"
```

---

## Troubleshooting

### Connection Timeout
- Verify firewall rules include your current IP
- Check if Azure services are allowed
- Verify connection string format

### Migration Fails
- Check SQL Server version compatibility
- Verify all required tables exist
- Review migration logs in `dotnet ef` output

### Performance Issues
- Review query performance with Azure SQL Insights
- Consider indexing strategy
- Check DTU utilization and scale up if needed

### Authentication Errors
- Verify credentials are correct
- Check if user has necessary permissions
- For Azure AD: verify tenant configuration

---

## Post-Migration Tasks

- [ ] Update CI/CD pipelines with new connection strings
- [ ] Configure automated backups
- [ ] Set up monitoring and alerts
- [ ] Document new connection procedures for team
- [ ] Remove Docker SQL container (after verification)
- [ ] Update development environment setup docs
- [ ] Test disaster recovery procedures

---

## Support Resources

- [Azure SQL Documentation](https://docs.microsoft.com/azure/azure-sql/)
- [EF Core Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- [Azure SQL Best Practices](https://docs.microsoft.com/azure/azure-sql/database/performance-guidance)

