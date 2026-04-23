# Azure SQL Migration - Quick Start

This guide will help you migrate from Docker SQL to Azure SQL in just a few steps.

## Prerequisites

Before starting, ensure you have:
- [ ] Azure CLI installed (`brew install azure-cli`)
- [ ] Azure account with active subscription
- [ ] .NET SDK 8.0+ installed
- [ ] Docker SQL container running with current data

## Three-Step Migration

### Step 1: Setup Azure SQL Database (5-10 minutes)

Run the automated setup script:

```bash
./scripts/azure-sql-setup.sh
```

**What it does:**
- Creates Azure SQL Server and Database
- Configures firewall rules
- Sets up Key Vault
- Enables security features

**You'll need to provide:**
- SQL admin username and password
- Database tier (recommend S2 for pilot/production, Basic for dev)

### Step 2: Apply Schema (2-3 minutes)

Run the migration script:

```bash
./scripts/migrate-to-azure.sh
```

**What it does:**
- Tests connectivity to Azure SQL
- Applies all EF Core migrations (creates tables, indexes, etc.)
- Exports data from Docker SQL
- Prepares import scripts

### Step 3: Import Data

Choose one of these methods:

#### Option A: Using Azure Data Studio (Recommended for simplicity)

1. Install Azure Data Studio: `brew install --cask azure-data-studio`
2. Connect to Azure SQL with credentials from Step 1
3. Right-click database → Tasks → Import Data
4. Select CSV files from `./data-export/` folder
5. Map columns and import

#### Option B: Using BCP Utility

```bash
# Install SQL Server command-line tools
brew tap microsoft/mssql-release https://github.com/Microsoft/homebrew-mssql-release
brew update
brew install mssql-tools

# Run import (replace with your details)
export SERVER="your-server.database.windows.net"
export DB="ISMSponsor"
export USER="your-admin"
export PASS="your-password"

# Import each table
for file in ./data-export/*.csv; do
    table=$(basename "$file" .csv)
    /usr/local/opt/mssql-tools/bin/bcp "$DB.dbo.$table" in "$file" \
        -S "$SERVER" -U "$USER" -P "$PASS" \
        -c -t, -F 2 -C 65001 -b 1000
done
```

#### Option C: Using Custom C# Import (Most reliable)

See `docs/AZURE_SQL_MIGRATION_GUIDE.md` for a detailed data import utility.

## Verification

After importing data:

```bash
# Test connection with EF Core
dotnet ef dbcontext info --connection "Server=YOUR_SERVER.database.windows.net;Database=ISMSponsor;User Id=YOUR_USER;Password=YOUR_PASS;Encrypt=true;MultipleActiveResultSets=true"

# Update appsettings.Development.json with Azure SQL connection string
# Run the application
dotnet run

# Test in browser
open https://localhost:5001/Health
```

## Update Development Configuration

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER.database.windows.net;Database=ISMSponsor;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"
  }
}
```

Get the connection string from:
- Azure Key Vault: `az keyvault secret show --name DatabaseConnectionString --vault-name ism-sponsor-kv --query value -o tsv`
- Or from the output of `azure-sql-setup.sh`

## Rollback Plan

If you need to revert to Docker SQL:

1. Change connection string back to Docker in `appsettings.Development.json`:
   ```json
   "DefaultConnection": "Server=localhost,1433;Database=ISMSponsor;User Id=sa;Password=Qwerty012210;TrustServerCertificate=true"
   ```

2. Ensure Docker SQL container is running:
   ```bash
   docker start <sql-container-name>
   ```

3. Restart application

## Common Issues

### "Cannot connect to Azure SQL"
- Check firewall rules include your current IP
- Verify credentials are correct
- Ensure your network allows outbound connections on port 1433

### "Login failed for user"
- Confirm username and password
- Check if account is locked or expired
- Verify you're using the right server name

### "Data import fails"
- Check for foreign key constraint violations
- Import tables in dependency order
- Disable constraints temporarily during import

### "Timeout during migration"
- Increase command timeout in scripts
- Check Azure SQL DTU usage (may need to scale up)
- Verify network connectivity is stable

## Cost Estimate

| Tier | Monthly Cost | Use Case |
|------|-------------|----------|
| Basic | ~$5 | Development only |
| S0 | ~$15 | Light testing |
| S2 | ~$75 | Pilot/Production |
| S3 | ~$150 | High traffic production |

**Recommendation**: Start with S2 for pilot, monitor performance, scale as needed.

## Next Steps After Migration

1. **Update CI/CD Pipeline**
   - Update Azure Pipeline variable for `DatabaseConnectionString`
   - Test deployment to pilot environment

2. **Enable Advanced Security**
   ```bash
   # Enable Azure AD authentication
   az sql server ad-admin create \
     --resource-group ISM-Sponsor-RG \
     --server-name YOUR_SERVER \
     --display-name "ISM Admin" \
     --object-id $(az ad signed-in-user show --query id -o tsv)
   ```

3. **Setup Monitoring**
   - Configure Azure SQL Insights
   - Set up alerts for DTU usage, storage, failed connections
   - Enable Query Performance Insights

4. **Configure Backups**
   - Review automated backup settings (7-day default)
   - Configure long-term retention if needed
   - Test restore procedure

5. **Remove Docker SQL** (after thorough testing)
   ```bash
   docker stop <sql-container-name>
   docker rm <sql-container-name>
   ```

## Support

- **Full Documentation**: See `docs/AZURE_SQL_MIGRATION_GUIDE.md`
- **Azure SQL Docs**: https://docs.microsoft.com/azure/azure-sql/
- **EF Core Migrations**: https://docs.microsoft.com/ef/core/managing-schemas/migrations/

## Files Created

- `docs/AZURE_SQL_MIGRATION_GUIDE.md` - Complete migration documentation
- `scripts/azure-sql-setup.sh` - Automated Azure setup
- `scripts/migrate-to-azure.sh` - Migration execution script
- `scripts/export-docker-data.sh` - Data export script
- `appsettings.AzureMigration.json` - Temporary config for migration

## Quick Reference Commands

```bash
# Login to Azure
az login

# Check current subscription
az account show

# List your SQL servers
az sql server list --resource-group ISM-Sponsor-RG -o table

# Get connection string from Key Vault
az keyvault secret show --name DatabaseConnectionString --vault-name ism-sponsor-kv --query value -o tsv

# Test database connection
dotnet ef dbcontext info

# Apply migrations
dotnet ef database update

# Generate SQL script from migrations
dotnet ef migrations script -o migration.sql
```

---

**Ready to migrate?** Start with: `./scripts/azure-sql-setup.sh`
