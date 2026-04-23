# Azure App Service Configuration - Complete Guide

## ⚠️ Connection String Format Issue

Your connection string has formatting issues that cause "bad request invalid hostname" error in Azure:

### ❌ INCORRECT (from appsettings.Development.json):
```
Server=YOUR_SERVER.database.windows.net,1433;Database=ISMSponsor;User Id=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD\\;TrustServerCertificate=true;MultipleActiveResultSets=true
```

**Problems:**
1. Comma after hostname: `,1433` should use semicolon or be omitted
2. Double backslash in password: `\\` - escaping differs between JSON and Azure App Settings
3. TrustServerCertificate should be false for Azure SQL

### ✅ CORRECT (for Azure App Service):
```
Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=ISMSponsor;User ID=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;MultipleActiveResultSets=true;
```

**OR simpler (recommended):**
```
Server=YOUR_SERVER.database.windows.net;Database=ISMSponsor;User Id=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true
```

> **Note:** In Azure App Service settings, use **single backslash** in passwords (no JSON escaping).

---

## 🔧 Required Azure App Service Configuration

Navigate to your App Service → **Configuration** → **Application settings**

### **1. Environment & Core Settings**

| Name | Value | Notes |
|------|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Set to Production or Pilot |
| `WEBSITE_TIME_ZONE` | `SE Asia Standard Time` | Optional: Match your timezone |

### **2. Database Connection** ⚠️ REQUIRED

| Name | Value |
|------|-------|
| `ConnectionStrings__DefaultConnection` | `Server=YOUR_SERVER.database.windows.net;Database=ISMSponsor;User Id=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true` |

> **Important:** Use double underscore `__` between `ConnectionStrings` and `DefaultConnection`

### **3. Security Settings** ⚠️ REQUIRED FOR PRODUCTION

| Name | Value | Required |
|------|-------|----------|
| `Security__UseHttpsRedirection` | `true` | ✅ Yes |
| `Security__UseHsts` | `true` | ✅ Yes |
| `Security__CookieSecurePolicy` | `Always` | ✅ Yes |
| `Security__AntiForgeryEnabled` | `true` | ✅ Yes |
| `Security__RequireAuthenticatedByDefault` | `true` | Optional |

### **4. Authentication Settings** (Choose ONE)

#### **Option A: Google OAuth** (Recommended if using Google Sign-In)

| Name | Value |
|------|-------|
| `Authentication__Google__ClientId` | `YOUR_GOOGLE_CLIENT_ID` |
| `Authentication__Google__ClientSecret` | `YOUR_GOOGLE_CLIENT_SECRET` |

**Important:** Update the authorized redirect URI in Google Cloud Console:
- Add: `https://YOUR_APP_NAME.azurewebsites.net/Account/GoogleCallback`

#### **Option B: Azure AD** (If not using Google)

| Name | Value |
|------|-------|
| `AzureAd__TenantId` | Your Azure AD Tenant ID |
| `AzureAd__ClientId` | Your App Registration Client ID |
| `AzureAd__ClientSecret` | Your App Registration Secret |
| `AzureAd__Domain` | `ismschool.onmicrosoft.com` |

### **5. Sponsor Authentication Settings**

| Name | Value | Notes |
|------|-------|-------|
| `SponsorAuth__RequireEmailConfirmation` | `true` | Recommended for production |
| `SponsorAuth__PasswordRequirements__RequireDigit` | `true` | |
| `SponsorAuth__PasswordRequirements__RequireNonAlphanumeric` | `true` | |
| `SponsorAuth__PasswordRequirements__RequireUppercase` | `true` | |
| `SponsorAuth__PasswordRequirements__RequiredLength` | `8` | Minimum 8 |
| `SponsorAuth__LockoutSettings__MaxFailedAccessAttempts` | `5` | |
| `SponsorAuth__LockoutSettings__LockoutDurationMinutes` | `30` | Minimum 5 |

### **6. Database Migration Control**

| Name | Value | Notes |
|------|-------|-------|
| `Database__RunMigrationsOnStartup` | `false` | **DO NOT** auto-migrate in Production |

> Run migrations manually via Kudu or deployment pipeline

### **7. Integration Endpoints** (Optional)

| Name | Value | Default |
|------|-------|---------|
| `IntegrationEndpoints__SyncEnabled` | `false` | Set to `true` only when ready |
| `IntegrationEndpoints__PowerSchoolApiUrl` | Your PowerSchool API URL | Required if sync enabled |
| `IntegrationEndpoints__StudentChargingPortalApiUrl` | Your SCP API URL | Required if sync enabled |
| `IntegrationEndpoints__NetSuiteApiUrl` | Your NetSuite API URL | Required if sync enabled |
| `IntegrationEndpoints__OnlineBillingSystemApiUrl` | Your OBS API URL | Required if sync enabled |

### **8. Application Insights** (Recommended)

| Name | Value |
|------|-------|
| `ApplicationInsights__ConnectionString` | `InstrumentationKey=xxx;IngestionEndpoint=https://...` |

Get from: Azure Portal → Application Insights → Properties → Connection String

### **9. Health Checks**

| Name | Value |
|------|-------|
| `HealthChecks__Enabled` | `true` |
| `HealthChecks__DetailedErrors` | `false` |

### **10. Azure Key Vault** (Optional - For Better Secret Management)

| Name | Value |
|------|-------|
| `KeyVault__Name` | Your Key Vault name (without .vault.azure.net) |

**Setup Steps:**
1. Create Azure Key Vault
2. Enable **Managed Identity** on your App Service
3. Grant the Managed Identity "Key Vault Secrets User" role
4. Store secrets in Key Vault with names like: `ConnectionStrings--DefaultConnection`

---

## 📋 Quick Configuration Script

Copy this into Azure Cloud Shell or local Azure CLI:

```bash
#!/bin/bash

# Variables - UPDATE THESE
RESOURCE_GROUP="your-resource-group"
APP_NAME="your-app-name"
DB_CONNECTION="Server=YOUR_SERVER.database.windows.net;Database=ISMSponsor;User Id=YOUR_DB_USERNAME;Password=YOUR_DB_PASSWORD;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"

# Core settings
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    ConnectionStrings__DefaultConnection="$DB_CONNECTION" \
    Security__UseHttpsRedirection="true" \
    Security__UseHsts="true" \
    Security__CookieSecurePolicy="Always" \
    Security__AntiForgeryEnabled="true" \
    Security__RequireAuthenticatedByDefault="true" \
    Database__RunMigrationsOnStartup="false" \
    IntegrationEndpoints__SyncEnabled="false" \
    HealthChecks__Enabled="true" \
    HealthChecks__DetailedErrors="false" \
    SponsorAuth__RequireEmailConfirmation="true" \
    SponsorAuth__PasswordRequirements__RequiredLength="8" \
    SponsorAuth__LockoutSettings__MaxFailedAccessAttempts="5" \
    SponsorAuth__LockoutSettings__LockoutDurationMinutes="30"

# Google OAuth (if using)
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings \
    Authentication__Google__ClientId="YOUR_GOOGLE_CLIENT_ID" \
    Authentication__Google__ClientSecret="YOUR_GOOGLE_CLIENT_SECRET"

echo "✅ Configuration complete!"
```

---

## 🔍 Enable Diagnostic Logging

### **1. Enable Application Logging**

```bash
az webapp log config \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --application-logging filesystem \
  --level information \
  --detailed-error-messages true \
  --failed-request-tracing true
```

### **2. View Live Logs**

```bash
az webapp log tail --resource-group $RESOURCE_GROUP --name $APP_NAME
```

### **3. Enable stdout Logging** (Critical for diagnosing startup issues)

In Kudu Console (`https://YOUR_APP.scm.azurewebsites.net`) or via FTP, create/edit `/site/wwwroot/web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\ISMSponsor.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile="\\?\%home%\LogFiles\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

View stdout logs: `/home/LogFiles/` in Kudu

---

## 🚀 Deployment Checklist

- [ ] Built and published successfully: `dotnet publish -c Release`
- [ ] Updated Azure App Service configuration with all required settings
- [ ] Verified connection string format (no comma issue, correct password escaping)
- [ ] Configured Google OAuth redirect URI in Google Cloud Console (if using)
- [ ] Enabled diagnostic logging in Azure
- [ ] Checked Azure SQL Server firewall allows Azure services
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Disabled auto-migrations: `Database__RunMigrationsOnStartup=false`
- [ ] Deployed application files to Azure
- [ ] Checked log stream for successful startup
- [ ] Verified health check endpoint: `https://YOUR_APP.azurewebsites.net/health`
- [ ] Tested login functionality

---

## 🐛 Troubleshooting

### **Issue: "bad request invalid hostname"**

**Cause:** Connection string format error

**Fix:** 
1. Remove comma after hostname or use `tcp:` prefix
2. Use single backslash in passwords (Azure settings don't need JSON escaping)
3. Use the corrected format shown at top of this document

### **Issue: "HTTP Error 500.30"**

**Cause:** Configuration validation failed

**Fix:** 
1. Check log stream: `az webapp log tail`
2. Look for `[FAIL] Configuration validation failed`
3. Ensure all REQUIRED settings in section above are configured
4. Verify connection string is set: `ConnectionStrings__DefaultConnection`
5. Verify security settings for Production environment

### **Issue: "PLACEHOLDER" errors**

**Cause:** appsettings.Production.json has placeholder values

**Fix:** Azure App Service settings override file settings. Ensure all placeholders are replaced in App Service configuration.

### **Issue: Can't connect to database**

**Fixes:**
1. Check Azure SQL Server firewall rules
2. Allow Azure services and resources: **ON**
3. Verify credentials are correct
4. Test connection from Kudu console:

```bash
# In Kudu Console
sqlcmd -S YOUR_SERVER.database.windows.net -d ISMSponsor -U YOUR_DB_USERNAME -P 'YOUR_DB_PASSWORD'
```

### **Issue: Configuration validation errors**

**Check for:**
- [ ] `SponsorAuth__LockoutSettings__LockoutDurationMinutes` >= 5
- [ ] `SponsorAuth__PasswordRequirements__RequiredLength` >= 8
- [ ] `Security__UseHttpsRedirection` = true (Production)
- [ ] `Security__UseHsts` = true (Production)
- [ ] `Security__CookieSecurePolicy` = Always (Production)
- [ ] `Security__AntiForgeryEnabled` = true

---

## 📊 Verify Deployment

After deployment, check these endpoints:

1. **Health Check:**
   ```
   https://YOUR_APP.azurewebsites.net/health
   ```
   Should return: `Healthy`

2. **Readiness Check:**
   ```
   https://YOUR_APP.azurewebsites.net/health/ready
   ```

3. **Swagger API Docs** (Pilot environment):
   ```
   https://YOUR_APP.azurewebsites.net/api/docs
   ```

4. **Login Page:**
   ```
   https://YOUR_APP.azurewebsites.net/Account/Login
   ```

---

## 🔐 Security Recommendations

1. **Use Azure Key Vault** for secrets instead of App Service settings
2. **Enable Managed Identity** on your App Service
3. **Rotate secrets regularly** (database passwords, OAuth secrets)
4. **Use SSL/TLS** (Azure provides free certificate)
5. **Enable Web Application Firewall** if using Application Gateway
6. **Review access logs** regularly
7. **Set up alerts** in Application Insights for errors/performance

---

## 📝 Next Steps

1. **Rebuild and republish:**
   ```bash
   dotnet publish ISMSponsor.csproj -c Release -o ./publish
   ```

2. **Configure Azure App Service** with correct settings (see above)

3. **Deploy to Azure:**
   - Via Azure DevOps pipeline
   - Via GitHub Actions
   - Via FTP/Kudu
   - Via Azure CLI

4. **Monitor startup logs** for successful initialization

5. **Test application** functionality

---

Need help? Check live logs with:
```bash
az webapp log tail --resource-group YOUR_RG --name YOUR_APP --provider application
```
