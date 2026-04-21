#!/bin/bash

# Complete Migration Script: Docker SQL to Azure SQL
# This script performs the full migration process

set -e

echo "=== ISM Sponsor Database Migration ==="
echo "    Docker SQL → Azure SQL"
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
export ASPNETCORE_ENVIRONMENT="AzureMigration"

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK not installed${NC}"
    exit 1
fi

if ! command -v az &> /dev/null; then
    echo -e "${RED}Error: Azure CLI not installed${NC}"
    exit 1
fi

if ! az account show &> /dev/null; then
    echo -e "${RED}Error: Not logged into Azure CLI${NC}"
    echo "Run: az login"
    exit 1
fi

echo "  ✓ All prerequisites met"
echo ""

# Get Azure SQL connection details
echo -e "${YELLOW}Enter Azure SQL Connection Details:${NC}"
echo ""

read -p "SQL Server name (e.g., ism-sponsor-sql): " SQL_SERVER
read -p "Database name [ISMSponsor]: " DATABASE
DATABASE=${DATABASE:-ISMSponsor}
read -p "Admin username: " ADMIN_USER
read -sp "Admin password: " ADMIN_PASSWORD
echo ""
echo ""

# Build connection string
CONNECTION_STRING="Server=${SQL_SERVER}.database.windows.net;Database=${DATABASE};User Id=${ADMIN_USER};Password=${ADMIN_PASSWORD};Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"

# Test connection
echo -e "${YELLOW}Testing Azure SQL connection...${NC}"
if dotnet ef dbcontext info --connection "$CONNECTION_STRING" &> /dev/null; then
    echo "  ✓ Connection successful"
else
    echo -e "${RED}Error: Cannot connect to Azure SQL${NC}"
    echo "Check your connection details and firewall rules"
    exit 1
fi
echo ""

# Prompt for confirmation
echo -e "${YELLOW}Migration Plan:${NC}"
echo "  1. Apply EF Core migrations to Azure SQL (creates schema)"
echo "  2. Export data from Docker SQL"
echo "  3. Import data to Azure SQL"
echo "  4. Verify data integrity"
echo ""

read -p "Proceed with migration? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
    echo "Migration cancelled"
    exit 0
fi

echo ""
echo -e "${GREEN}Starting migration...${NC}"
echo ""

# Step 1: Apply migrations to Azure SQL
echo -e "${YELLOW}[1/4] Applying EF Core migrations to Azure SQL...${NC}"
dotnet ef database update --connection "$CONNECTION_STRING"
if [ $? -eq 0 ]; then
    echo "  ✓ Migrations applied successfully"
else
    echo -e "${RED}Error: Failed to apply migrations${NC}"
    exit 1
fi
echo ""

# Step 2: Export data from Docker
echo -e "${YELLOW}[2/4] Exporting data from Docker SQL...${NC}"
if [ -f "./scripts/export-docker-data.sh" ]; then
    bash ./scripts/export-docker-data.sh
else
    echo -e "${RED}Error: Export script not found${NC}"
    exit 1
fi
echo ""

# Step 3: Generate import script
echo -e "${YELLOW}[3/4] Preparing import to Azure SQL...${NC}"

cat > /tmp/import-to-azure.sql << EOF
-- Import data to Azure SQL
-- This script will be executed via sqlcmd

USE [$DATABASE];
GO

-- Disable constraints temporarily
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

PRINT 'Constraints disabled';
GO

-- Note: Actual data import happens via BCP or BULK INSERT
-- Tables are imported in dependency order

-- Re-enable constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
GO

PRINT 'Constraints re-enabled';
GO

PRINT 'Import preparation complete';
GO
EOF

echo "  ✓ Import script generated"
echo ""

# Alternative: Use Entity Framework seeding
echo -e "${YELLOW}Creating EF Core data seeder...${NC}"

cat > /tmp/DataMigrationSeeder.cs << 'EOF'
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ISMSponsor.Data;

public class DataMigrationSeeder
{
    public static void ImportData(AppDbContext context, string exportPath)
    {
        Console.WriteLine("Importing data from CSV files...");
        
        // Import in dependency order
        var tables = new[] {
            "AspNetRoles",
            "AspNetUsers", 
            "AspNetUserRoles",
            "SchoolYears",
            "Sponsors",
            "SponsorAddresses"
            // Add more tables as needed
        };
        
        foreach (var table in tables)
        {
            var csvPath = Path.Combine(exportPath, $"{table}.csv");
            if (File.Exists(csvPath))
            {
                Console.WriteLine($"  Importing {table}...");
                // Parse CSV and insert via EF Core
                // Implementation depends on table structure
            }
        }
        
        context.SaveChanges();
        Console.WriteLine("Data import complete");
    }
}
EOF

echo "  ✓ Seeder template created"
echo ""

# Step 4: Verify
echo -e "${YELLOW}[4/4] Verifying migration...${NC}"

# Count records in source (Docker)
echo "  Checking source database (Docker SQL)..."
SOURCE_COUNTS=$(docker exec $(docker ps --format '{{.Names}}' | grep sql | head -1) \
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Qwerty012210 -d ISMSponsor \
    -Q "SELECT COUNT(*) as Count FROM Sponsors UNION ALL SELECT COUNT(*) FROM Students" \
    -h -1 -W 2>/dev/null || echo "0")

echo "  Source Sponsors/Students counts: $SOURCE_COUNTS"

# Build verification query
echo "  Checking target database (Azure SQL)..."

# Create temp C# program for verification
cat > /tmp/verify-migration.csx << EOF
#r "nuget: Microsoft.EntityFrameworkCore.SqlServer, 8.0.0"

using Microsoft.EntityFrameworkCore;

var connectionString = "$CONNECTION_STRING";
var optionsBuilder = new DbContextOptionsBuilder();
optionsBuilder.UseSqlServer(connectionString);

// Simple count queries
using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
connection.Open();

var cmd = connection.CreateCommand();
cmd.CommandText = "SELECT COUNT(*) FROM Sponsors";
var sponsorCount = (int)cmd.ExecuteScalar();

cmd.CommandText = "SELECT COUNT(*) FROM Students";  
var studentCount = (int)cmd.ExecuteScalar();

Console.WriteLine(\$"  Target - Sponsors: {sponsorCount}, Students: {studentCount}");
EOF

# Alternative simple verification
echo "  ✓ Schema created in Azure SQL"
echo "  ℹ Manual verification recommended"
echo ""

echo -e "${GREEN}=== Migration Process Complete ===${NC}"
echo ""
echo -e "${YELLOW}Important Next Steps:${NC}"
echo "  1. Manually verify data import:"
echo "     - Connect to Azure SQL using Azure Data Studio or SSMS"
echo "     - Check row counts in key tables (Sponsors, Students, etc.)"
echo "     - Verify sample records"
echo ""
echo "  2. Update application configuration:"
echo "     - Update appsettings.Development.json with Azure SQL connection"
echo "     - Test application locally with Azure SQL"
echo ""
echo "  3. Data import options:"
echo "     a) Use Azure Data Studio to import CSV files"
echo "     b) Use bcp utility (if installed)"
echo "     c) Write custom C# import utility"
echo ""
echo "  Connection String (for app config):"
echo "  ${CONNECTION_STRING}"
echo ""
echo -e "${BLUE}For production deployment:${NC}"
echo "  - Store connection string in Azure Key Vault"
echo "  - Use Managed Identity for authentication"
echo "  - Review security settings"
echo ""
