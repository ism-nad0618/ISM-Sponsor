# Local Development Setup - Secrets Configuration

## ⚠️ Important: Do NOT commit real secrets to Git!

This guide helps you set up local secrets for development without exposing them in version control.

## 📋 Files You Need to Create

### 1. `appsettings.Development.json`

Copy from `appsettings.json.example` and update with your actual values:

```bash
cp appsettings.json.example appsettings.Development.json
```

Then edit `appsettings.Development.json` with your actual credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ism-sandbox.database.windows.net;Database=ISMSponsor;User Id=ISMSponsorUser;Password=YOUR_ACTUAL_PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_ACTUAL_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_ACTUAL_GOOGLE_CLIENT_SECRET"
    }
  }
}
```

### 2. `appsettings.AzureMigration.json` (if needed)

```bash
cp appsettings.AzureMigration.json.example appsettings.AzureMigration.json
```

Update with your actual Azure database credentials.

## 🔐 Where to Get Credentials

### Database Connection String
- **Server:** Your Azure SQL Server hostname
- **Database:** ISMSponsor
- **User ID:** Database username
- **Password:** Database password (ask your DBA or check Azure Portal)

### Google OAuth Credentials
1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Select your project (or create one)
3. Navigate to: **APIs & Services** → **Credentials**
4. Find your OAuth 2.0 Client ID
5. Copy **Client ID** and **Client Secret**

**Authorized Redirect URIs must include:**
- `http://localhost:5001/Account/GoogleCallback` (Development)
- `https://YOUR_APP.azurewebsites.net/Account/GoogleCallback` (Production)

## 🚀 For Azure Deployment

**Don't use config files!** Use the Azure CLI script instead:

```bash
# Edit with your actual values
nano scripts/configure-azure-app-service.sh

# Update these lines with real credentials:
RESOURCE_GROUP="your-resource-group"
APP_NAME="your-app-name"

# Also update the Google OAuth credentials in the script
```

Then run:
```bash
bash scripts/configure-azure-app-service.sh
```

## ✅ Verify Your Setup

These files should be ignored by Git (check `.gitignore`):
- ✅ `appsettings.Development.json`
- ✅ `appsettings.AzureMigration.json`
- ✅ `appsettings.json` (custom local settings)

These files are safe to commit (no secrets):
- ✅ `appsettings.json.example`
- ✅ `appsettings.AzureMigration.json.example`
- ✅ `appsettings.Production.json` (uses placeholders)
- ✅ `appsettings.Pilot.json`

## 🔍 Check What Files Are Tracked

```bash
# See what's staged for commit
git status

# Make sure no files with secrets are listed!
# If you see appsettings.Development.json or similar, DO NOT commit!
```

## 🛡️ If You Accidentally Committed Secrets

1. **Immediately rotate all exposed credentials** (change passwords, regenerate OAuth secrets)
2. Remove from Git history:
   ```bash
   git rm --cached appsettings.Development.json
   git commit -m "Remove sensitive config file"
   ```
3. Add to `.gitignore` (already done)
4. Update credentials in:
   - Google Cloud Console (OAuth)
   - Azure SQL Server (database password)
   - Azure App Service settings

## 📞 Need Help?

Contact your team lead or check:
- [AZURE_APP_SERVICE_CONFIGURATION.md](AZURE_APP_SERVICE_CONFIGURATION.md) - Azure deployment guide
- [AZURE_DEPLOYMENT_FIX.md](AZURE_DEPLOYMENT_FIX.md) - Troubleshooting guide
