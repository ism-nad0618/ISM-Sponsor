# Azure Deployment Configuration Guide

## Issues Fixed

The HTTP 500.30 error was caused by missing configuration values during startup. The following changes were made to [Program.cs](Program.cs):

### 1. **Made Google OAuth Optional**
- Previously threw exception if credentials were missing
- Now gracefully skips Google authentication if not configured
- App can start without Google OAuth credentials

### 2. **Made Application Insights Optional**
- Previously threw exception if connection string missing in non-Development
- Now logs warning but allows startup
- Telemetry can be configured later without redeployment

### 3. **Removed Early BuildServiceProvider Calls**
- Eliminated problematic early service provider builds during configuration phase
- Used Console.WriteLine for early startup logging
- Prevents potential DI container issues

### 4. **Added Detailed Startup Logging**
- Console output for each configuration step
- Clear error messages when configuration fails
- Helps diagnose Azure deployment issues

---

## Required Azure App Service Configuration

To deploy successfully to Azure, you **MUST** configure the following settings in Azure App Service:

### **Application Settings** (Configuration → Application settings)

```plaintext
# REQUIRED - Database Connection
ConnectionStrings__DefaultConnection = Server=YOUR_SERVER.database.windows.net;Database=ISMSponsor;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true

# REQUIRED - ASP.NET Core Environment
ASPNETCORE_ENVIRONMENT = Production

# OPTIONAL - Google OAuth (if you want Google sign-in)
Authentication__Google__ClientId = YOUR_GOOGLE_CLIENT_ID
Authentication__Google__ClientSecret = YOUR_GOOGLE_CLIENT_SECRET

# OPTIONAL - Application Insights (recommended for monitoring)
ApplicationInsights__ConnectionString = YOUR_APP_INSIGHTS_CONNECTION_STRING

# OPTIONAL - Azure Key Vault (for centralized secrets)
KeyVault__Name = YOUR_KEY_VAULT_NAME

# OPTIONAL - Database Migration Control
Database__RunMigrationsOnStartup = false

# Security Settings (if not using Key Vault)
Security__UseHttpsRedirection = true
Security__UseHsts = true
Security__CookieSecurePolicy = Always

# Integration Endpoints (if applicable)
Integration__PowerSchoolApiUrl = YOUR_POWERSCHOOL_API_URL
Integration__StudentChargingPortalApiUrl = YOUR_SCP_API_URL
Integration__NetSuiteApiUrl = YOUR_NETSUITE_API_URL
Integration__OnlineBillingSystemApiUrl = YOUR_OBS_API_URL
```

---

## Deployment Steps

### 1. **Update Azure App Service Configuration**

In Azure Portal:
1. Navigate to your App Service
2. Go to **Configuration** → **Application settings**
3. Add the required settings above (especially `ConnectionStrings__DefaultConnection`)
4. Click **Save** and **Continue**

### 2. **Republish the Application**

From your local terminal:
```bash
# Build and publish
dotnet publish ISMSponsor.csproj -c Release -o ./publish

# Deploy to Azure (using Azure CLI - adjust resource group and app name)
az webapp deploy --resource-group YOUR_RESOURCE_GROUP --name YOUR_APP_NAME --src-path ./publish.zip
```

Or use Visual Studio publish profile or GitHub Actions as configured.

### 3. **Enable Diagnostic Logging**

In Azure Portal:
1. Go to **Monitoring** → **App Service logs**
2. Enable **Application Logging (Filesystem)** → Level: Information
3. Enable **Detailed error messages** → On
4. Enable **Failed request tracing** → On
5. Click **Save**

### 4. **View Logs**

After deployment, check the startup logs:
```bash
# Using Azure CLI
az webapp log tail --resource-group YOUR_RESOURCE_GROUP --name YOUR_APP_NAME

# Or in Azure Portal
# Go to Monitoring → Log stream
```

Look for startup messages like:
```
Environment: Production
Checking database connection string...
Database connection string configured
Google OAuth not configured - skipping (credentials missing)
WARNING: ApplicationInsights:ConnectionString not configured
Validating application configuration...
[OK] Configuration validation passed
```

---

## Troubleshooting

### **Still getting 500.30 error?**

1. **Check the connection string format:**
   - Use double underscores: `ConnectionStrings__DefaultConnection`
   - Ensure password special characters are not causing issues
   - Test connection to SQL Server from Azure using kudu console

2. **Check Configuration Validation:**
   - The app validates configuration on startup
   - Check logs for "[FAIL] Configuration validation failed"
   - May need to check `ConfigurationValidationService` requirements

3. **Check Database Accessibility:**
   - Ensure Azure SQL Server firewall allows Azure services
   - Verify credentials are correct
   - Check if database migrations are needed

4. **Enable stdout logging:**
   - In `web.config` or via Azure Portal
   - Set `stdoutLogEnabled="true"`
   - Check the stdout logs in `/home/LogFiles/`

### **Quick Test Commands**

```bash
# Test database connection from Kudu console
sqlcmd -S YOUR_SERVER.database.windows.net -d ISMSponsor -U YOUR_USERNAME -P YOUR_PASSWORD -Q "SELECT 1"

# Check environment variables in Kudu
env | grep -i connection
env | grep -i aspnetcore

# View latest logs
tail -100 /home/LogFiles/Application/app*.txt
```

---

## Optional Enhancements

### **1. Use Azure Key Vault for Secrets**

Instead of storing secrets in App Service settings, use Azure Key Vault:

1. Create a Key Vault in Azure
2. Enable **Managed Identity** for your App Service
3. Grant the Managed Identity access to Key Vault
4. Set only `KeyVault__Name` in App Service settings
5. Store all secrets in Key Vault

### **2. Setup Application Insights**

For monitoring and diagnostics:

1. Create Application Insights resource
2. Copy connection string
3. Add to App Service settings: `ApplicationInsights__ConnectionString`
4. Redeploy

### **3. Configure Google OAuth**

If you want Google sign-in for staff:

1. Create OAuth 2.0 credentials in Google Cloud Console
2. Add authorized redirect URI: `https://YOUR_DOMAIN/Account/GoogleCallback`
3. Add to App Service settings:
   - `Authentication__Google__ClientId`
   - `Authentication__Google__ClientSecret`

---

## Local Testing

To test the production configuration locally:

```bash
# Set environment to Production
export ASPNETCORE_ENVIRONMENT=Production

# Set required configuration
export ConnectionStrings__DefaultConnection="Server=..."

# Run the app
dotnet run
```

The app should start and show similar console output to what you'll see in Azure.

---

## Summary

✅ **Fixed startup configuration issues**  
✅ **Made optional features truly optional**  
✅ **Added detailed startup diagnostics**  
✅ **Published successfully**  

**Next Steps:**
1. Configure Azure App Service settings (especially database connection string)
2. Redeploy the application
3. Check log stream for successful startup
4. Test the application in Azure

The app is now more resilient to configuration issues and will provide clear error messages when configuration is missing, making Azure deployments much easier to diagnose and fix.
