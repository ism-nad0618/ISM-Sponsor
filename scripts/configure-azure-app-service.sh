#!/bin/bash

# ============================================================================
# Azure App Service Configuration Script
# ============================================================================
# This script configures all required settings for ISM Sponsor application
# Run this from Azure Cloud Shell or local machine with Azure CLI installed
#
# ⚠️  IMPORTANT: Replace placeholder values before running!
#     - Update RESOURCE_GROUP and APP_NAME
#     - Update database connection string (line 32-33)
#     - Update Google OAuth credentials (lines 51-52)
#
# Usage:
#   1. Update all placeholder values below (marked with YOUR_*)
#   2. Run: bash configure-azure-app-service.sh
#   3. Or copy/paste into Azure Cloud Shell
# ============================================================================

# ===== UPDATE THESE VALUES =====
RESOURCE_GROUP="YOUR_RESOURCE_GROUP_NAME"
APP_NAME="YOUR_APP_SERVICE_NAME"

# Database credentials (get from Azure Portal or your DBA)
DB_SERVER="YOUR_SERVER.database.windows.net"
DB_NAME="ISMSponsor"
DB_USER="YOUR_DATABASE_USERNAME"
DB_PASSWORD="YOUR_DATABASE_PASSWORD"

# Google OAuth (get from Google Cloud Console)
GOOGLE_CLIENT_ID="YOUR_GOOGLE_CLIENT_ID"
GOOGLE_CLIENT_SECRET="YOUR_GOOGLE_CLIENT_SECRET"
# ================================

echo "🔧 Configuring Azure App Service: $APP_NAME"
echo "📦 Resource Group: $RESOURCE_GROUP"
echo ""

# Construct connection string from variables
CONNECTION_STRING="Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"

# Set all application settings in a SINGLE command
az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_NAME" \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
    Security__UseHttpsRedirection="true" \
    Security__UseHsts="true" \
    Security__CookieSecurePolicy="Always" \
    Security__AntiForgeryEnabled="true" \
    Security__RequireAuthenticatedByDefault="true" \
    Database__RunMigrationsOnStartup="false" \
    IntegrationEndpoints__SyncEnabled="false" \
    IntegrationEndpoints__PowerSchoolApiUrl="https://dev-ps.ismschool.local/api" \
    IntegrationEndpoints__StudentChargingPortalApiUrl="https://dev-scp.ismschool.local/api" \
    IntegrationEndpoints__NetSuiteApiUrl="https://dev-netsuite.ismschool.local/api" \
    IntegrationEndpoints__OnlineBillingSystemApiUrl="https://dev-obs.ismschool.local/api" \
    HealthChecks__Enabled="true" \
    HealthChecks__DetailedErrors="false" \
    SponsorAuth__RequireEmailConfirmation="true" \
    SponsorAuth__PasswordRequirements__RequireDigit="true" \
    SponsorAuth__PasswordRequirements__RequireNonAlphanumeric="true" \
    SponsorAuth__PasswordRequirements__RequireUppercase="true" \
    SponsorAuth__PasswordRequirements__RequiredLength="8" \
    SponsorAuth__LockoutSettings__MaxFailedAccessAttempts="5" \
    SponsorAuth__LockoutSettings__LockoutDurationMinutes="30" \
    Authentication__Google__ClientId="$GOOGLE_CLIENT_ID" \
    Authentication__Google__ClientSecret="$GOOGLE_CLIENT_SECRET" \
    ApplicationInsights__ConnectionString="" \
    AllowedHosts="*.ismmanila.org"

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Configuration complete!"
    echo ""
    echo "📋 Next steps:"
    echo "  1. The app will automatically restart"
    echo "  2. Check logs: az webapp log tail --resource-group $RESOURCE_GROUP --name $APP_NAME"
    echo "  3. Visit: https://$APP_NAME.azurewebsites.net"
    echo "  4. Check health: https://$APP_NAME.azurewebsites.net/health"
    echo ""
else
    echo ""
    echo "❌ Configuration failed. Check your Azure CLI login and permissions."
    echo ""
fi
