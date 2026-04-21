#!/bin/bash

# Azure SQL Database Setup Script
# This script creates and configures Azure SQL Database for ISM Sponsor

set -e  # Exit on error

echo "=== Azure SQL Database Setup for ISM Sponsor ==="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
RESOURCE_GROUP="ISM-Sponsor-RG"
LOCATION="eastus"
SQL_SERVER_NAME="ism-sponsor-sql"  # Change if needed (must be globally unique)
DATABASE_NAME="ISMSponsor"
KEY_VAULT_NAME="ism-sponsor-kv"

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}Error: Azure CLI is not installed${NC}"
    echo "Install it from: https://docs.microsoft.com/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in
echo -e "${YELLOW}Checking Azure login status...${NC}"
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}Please login to Azure...${NC}"
    az login
fi

# Get subscription info
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
echo -e "${GREEN}Using subscription: ${SUBSCRIPTION_NAME}${NC}"
echo ""

# Collect inputs
echo -e "${YELLOW}Please provide the following information:${NC}"
echo ""

read -p "SQL Admin Username [ismsponadmin]: " ADMIN_USER
ADMIN_USER=${ADMIN_USER:-ismsponadmin}

read -sp "SQL Admin Password (min 8 chars, mixed case, numbers, symbols): " ADMIN_PASSWORD
echo ""

if [ -z "$ADMIN_PASSWORD" ]; then
    echo -e "${RED}Password cannot be empty${NC}"
    exit 1
fi

read -p "Database Tier (Basic/S0/S2/S3) [S2]: " DB_TIER
DB_TIER=${DB_TIER:-S2}

echo ""
echo -e "${YELLOW}Configuration Summary:${NC}"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  SQL Server: $SQL_SERVER_NAME"
echo "  Database: $DATABASE_NAME"
echo "  Admin User: $ADMIN_USER"
echo "  Database Tier: $DB_TIER"
echo ""

read -p "Proceed with setup? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
    echo "Setup cancelled"
    exit 0
fi

echo ""
echo -e "${GREEN}Starting setup...${NC}"
echo ""

# Step 1: Create Resource Group (if not exists)
echo -e "${YELLOW}[1/7] Creating resource group...${NC}"
if az group show --name $RESOURCE_GROUP &> /dev/null; then
    echo "  Resource group already exists"
else
    az group create \
        --name $RESOURCE_GROUP \
        --location $LOCATION \
        --output none
    echo "  ✓ Resource group created"
fi

# Step 2: Create SQL Server
echo -e "${YELLOW}[2/7] Creating SQL Server...${NC}"
if az sql server show --name $SQL_SERVER_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo "  SQL Server already exists"
else
    az sql server create \
        --name $SQL_SERVER_NAME \
        --resource-group $RESOURCE_GROUP \
        --location $LOCATION \
        --admin-user $ADMIN_USER \
        --admin-password "$ADMIN_PASSWORD" \
        --output none
    echo "  ✓ SQL Server created"
fi

# Step 3: Configure Firewall
echo -e "${YELLOW}[3/7] Configuring firewall rules...${NC}"

# Allow Azure services
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0 \
    --output none 2>/dev/null || echo "  Azure services rule already exists"

# Allow current IP
MY_IP=$(curl -s https://api.ipify.org)
echo "  Your IP: $MY_IP"
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name AllowMyIP \
    --start-ip-address $MY_IP \
    --end-ip-address $MY_IP \
    --output none 2>/dev/null || echo "  IP rule already exists"

echo "  ✓ Firewall configured"

# Step 4: Create Database
echo -e "${YELLOW}[4/7] Creating database...${NC}"
if az sql db show --name $DATABASE_NAME --server $SQL_SERVER_NAME --resource-group $RESOURCE_GROUP &> /dev/null; then
    echo "  Database already exists"
else
    az sql db create \
        --resource-group $RESOURCE_GROUP \
        --server $SQL_SERVER_NAME \
        --name $DATABASE_NAME \
        --service-objective $DB_TIER \
        --backup-storage-redundancy Local \
        --output none
    echo "  ✓ Database created"
fi

# Step 5: Create Key Vault
echo -e "${YELLOW}[5/7] Creating Key Vault...${NC}"
if az keyvault show --name $KEY_VAULT_NAME &> /dev/null; then
    echo "  Key Vault already exists"
else
    az keyvault create \
        --name $KEY_VAULT_NAME \
        --resource-group $RESOURCE_GROUP \
        --location $LOCATION \
        --output none
    echo "  ✓ Key Vault created"
fi

# Step 6: Store Connection String
echo -e "${YELLOW}[6/7] Storing connection string in Key Vault...${NC}"
CONNECTION_STRING="Server=${SQL_SERVER_NAME}.database.windows.net;Database=${DATABASE_NAME};User Id=${ADMIN_USER};Password=${ADMIN_PASSWORD};Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"

az keyvault secret set \
    --vault-name $KEY_VAULT_NAME \
    --name "DatabaseConnectionString" \
    --value "$CONNECTION_STRING" \
    --output none

echo "  ✓ Connection string stored"

# Step 7: Enable Auditing
echo -e "${YELLOW}[7/7] Enabling auditing...${NC}"
AUDIT_STORAGE="ismsponsoraudit$RANDOM"
az storage account create \
    --name $AUDIT_STORAGE \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku Standard_LRS \
    --output none 2>/dev/null || true

az sql server audit-policy update \
    --resource-group $RESOURCE_GROUP \
    --name $SQL_SERVER_NAME \
    --state Enabled \
    --storage-account $AUDIT_STORAGE \
    --output none 2>/dev/null || echo "  Auditing already enabled"

echo "  ✓ Auditing configured"

echo ""
echo -e "${GREEN}=== Setup Complete! ===${NC}"
echo ""
echo "Connection Details:"
echo "  Server: ${SQL_SERVER_NAME}.database.windows.net"
echo "  Database: ${DATABASE_NAME}"
echo "  Username: ${ADMIN_USER}"
echo ""
echo "Connection String (saved in Key Vault):"
echo "  $CONNECTION_STRING"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "  1. Run migration script: ./scripts/migrate-to-azure.sh"
echo "  2. Or apply EF migrations manually: dotnet ef database update"
echo "  3. Update appsettings.Development.json with the connection string"
echo ""
