#!/bin/bash

# Export Data from Docker SQL Server
# This script exports data from the local Docker SQL Server instance

set -e

echo "=== Export Data from Docker SQL Server ==="
echo ""

# Configuration
DOCKER_SERVER="localhost,1433"
DATABASE="ISMSponsor"
USER="sa"
PASSWORD="Qwerty012210"
EXPORT_DIR="./data-export"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check if Docker is running
echo -e "${YELLOW}Checking Docker SQL Server...${NC}"
if ! docker ps | grep -q sql; then
    echo -e "${RED}Error: SQL Server container is not running${NC}"
    echo "Start it with: docker start <container-name>"
    exit 1
fi

# Create export directory
mkdir -p "$EXPORT_DIR"
echo "  ✓ Export directory created: $EXPORT_DIR"
echo ""

# List of tables to export (in dependency order)
TABLES=(
    "AspNetRoles"
    "AspNetUsers"
    "AspNetUserRoles"
    "AspNetUserClaims"
    "AspNetUserLogins"
    "AspNetUserTokens"
    "AspNetRoleClaims"
    "SchoolYears"
    "Sponsors"
    "SponsorAddresses"
    "SponsorContacts"
    "Students"
    "ItemCategories"
    "Items"
    "LoGCoverageRules"
    "LogCoverages"
    "ChangeRequests"
    "ActivityLogs"
    "UserPreferences"
    "CoverageEvaluationAudits"
    "SponsorChangeRequests"
    "SponsorDuplicateCandidates"
    "MergeOperations"
    "SyncLogs"
    "UserFeedback"
)

# Method 1: Using docker exec with sqlcmd
echo -e "${YELLOW}Method: Using docker exec with sqlcmd${NC}"
echo "Exporting tables..."
echo ""

# Get container name
CONTAINER_NAME=$(docker ps --format '{{.Names}}' | grep sql | head -1)
echo "Using container: $CONTAINER_NAME"
echo ""

# Function to export table
export_table() {
    local table=$1
    local file="$EXPORT_DIR/${table}.sql"
    
    echo -n "  Exporting $table... "
    
    # Export as INSERT statements
    docker exec -i $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd \
        -S localhost -U $USER -P $PASSWORD -d $DATABASE \
        -Q "SET NOCOUNT ON; SELECT * FROM [$table]" \
        -s "," -w 8000 -W \
        > "$EXPORT_DIR/${table}.csv" 2>/dev/null
    
    if [ $? -eq 0 ]; then
        ROW_COUNT=$(wc -l < "$EXPORT_DIR/${table}.csv")
        echo "✓ ($ROW_COUNT rows)"
    else
        echo "⚠ Failed or empty"
    fi
}

# Export each table
for table in "${TABLES[@]}"; do
    export_table "$table"
done

echo ""
echo -e "${GREEN}Export Summary:${NC}"
echo "  Location: $EXPORT_DIR"
echo "  Tables exported: ${#TABLES[@]}"

# Show file sizes
echo ""
echo "File Sizes:"
du -h "$EXPORT_DIR"/*.csv 2>/dev/null | awk '{print "  " $2 ": " $1}'

# Create a backup using sqlcmd BCP alternative
echo ""
echo -e "${YELLOW}Creating full database script...${NC}"

docker exec -i $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U $USER -P $PASSWORD -d $DATABASE \
    -Q "EXEC sp_MSforeachtable 'SELECT ''DROP TABLE IF EXISTS ['' + OBJECT_SCHEMA_NAME(object_id) + ''].['' + OBJECT_NAME(object_id) + '']''" \
    > "$EXPORT_DIR/drop-tables.sql"

echo "  ✓ Schema script created"

echo ""
echo -e "${GREEN}=== Export Complete ===${NC}"
echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "  1. Verify exported files in: $EXPORT_DIR"
echo "  2. Apply migrations to Azure SQL: dotnet ef database update"
echo "  3. Import data: ./scripts/import-to-azure.sh"
echo ""
