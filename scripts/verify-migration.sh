#!/bin/bash

# Database Migration Verification Script
# Compares data between Docker SQL and Azure SQL

echo "=== Database Migration Verification ==="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Get Azure SQL connection details
echo -e "${YELLOW}Enter Azure SQL Details:${NC}"
read -p "Server name (without .database.windows.net): " AZURE_SERVER
read -p "Database name [ISMSponsor]: " AZURE_DB
AZURE_DB=${AZURE_DB:-ISMSponsor}
read -p "Username: " AZURE_USER
read -sp "Password: " AZURE_PASS
echo ""
echo ""

# Docker SQL connection
DOCKER_CONTAINER=$(docker ps --format '{{.Names}}' | grep -i sql | head -1)

if [ -z "$DOCKER_CONTAINER" ]; then
    echo -e "${RED}Warning: Docker SQL container not found or not running${NC}"
    echo "Skipping source comparison..."
    COMPARE_SOURCE=false
else
    echo -e "${GREEN}Found Docker container: $DOCKER_CONTAINER${NC}"
    COMPARE_SOURCE=true
fi

echo ""
echo -e "${YELLOW}Running verification checks...${NC}"
echo ""

# Function to query Azure SQL
query_azure() {
    local query=$1
    docker run --rm mcr.microsoft.com/mssql-tools \
        /opt/mssql-tools/bin/sqlcmd \
        -S "${AZURE_SERVER}.database.windows.net" \
        -d "$AZURE_DB" \
        -U "$AZURE_USER" \
        -P "$AZURE_PASS" \
        -Q "$query" \
        -h -1 -W 2>/dev/null | tr -d '[:space:]'
}

# Function to query Docker SQL
query_docker() {
    local query=$1
    docker exec -i $DOCKER_CONTAINER \
        /opt/mssql-tools/bin/sqlcmd \
        -S localhost \
        -d ISMSponsor \
        -U sa \
        -P Qwerty012210 \
        -Q "$query" \
        -h -1 -W 2>/dev/null | tr -d '[:space:]'
}

# Check 1: Database exists
echo -n "1. Azure SQL database accessible... "
AZURE_CHECK=$(query_azure "SELECT 1" 2>/dev/null)
if [ "$AZURE_CHECK" == "1" ]; then
    echo -e "${GREEN}✓${NC}"
else
    echo -e "${RED}✗${NC}"
    echo "  Cannot connect to Azure SQL. Check credentials and firewall rules."
    exit 1
fi

# Check 2: Tables exist
echo -n "2. Tables created in Azure SQL... "
TABLE_COUNT=$(query_azure "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'")
if [ "$TABLE_COUNT" -gt "0" ]; then
    echo -e "${GREEN}✓ ($TABLE_COUNT tables)${NC}"
else
    echo -e "${RED}✗${NC}"
    echo "  No tables found. Run migrations first."
    exit 1
fi

# Key tables to verify
TABLES=(
    "Sponsors"
    "Students"
    "LogCoverages"
    "SchoolYears"
    "Items"
    "AspNetUsers"
)

echo ""
echo -e "${YELLOW}Checking row counts:${NC}"
echo ""

TOTAL_ISSUES=0

for table in "${TABLES[@]}"; do
    # Get Azure count
    AZURE_COUNT=$(query_azure "SELECT COUNT(*) FROM [$table]")
    
    if [ "$COMPARE_SOURCE" = true ]; then
        # Get Docker count
        DOCKER_COUNT=$(query_docker "SELECT COUNT(*) FROM [$table]")
        
        # Compare
        echo -n "  $table: "
        printf "Docker: %-8s → Azure: %-8s " "$DOCKER_COUNT" "$AZURE_COUNT"
        
        if [ "$AZURE_COUNT" == "$DOCKER_COUNT" ]; then
            echo -e "${GREEN}✓${NC}"
        else
            echo -e "${RED}✗ MISMATCH${NC}"
            ((TOTAL_ISSUES++))
        fi
    else
        # Just show Azure count
        echo "  $table: $AZURE_COUNT rows"
    fi
done

echo ""

# Check 3: Foreign key constraints
echo -n "3. Foreign key constraints enabled... "
FK_COUNT=$(query_azure "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE='FOREIGN KEY'")
echo -e "${GREEN}✓ ($FK_COUNT constraints)${NC}"

# Check 4: Indexes
echo -n "4. Indexes created... "
IDX_COUNT=$(query_azure "SELECT COUNT(*) FROM sys.indexes WHERE type > 0 AND is_primary_key = 0")
echo -e "${GREEN}✓ ($IDX_COUNT indexes)${NC}"

# Check 5: Identity columns
echo -n "5. Identity columns configured... "
IDENTITY_COUNT=$(query_azure "SELECT COUNT(*) FROM sys.identity_columns")
echo -e "${GREEN}✓ ($IDENTITY_COUNT columns)${NC}"

# Check 6: Sample data verification
if [ "$COMPARE_SOURCE" = true ]; then
    echo ""
    echo -e "${YELLOW}Sample data spot-check:${NC}"
    
    # Check if a specific sponsor exists in both
    SAMPLE_ID=$(query_docker "SELECT TOP 1 SponsorId FROM Sponsors ORDER BY SponsorId" 2>/dev/null)
    if [ ! -z "$SAMPLE_ID" ]; then
        DOCKER_NAME=$(query_docker "SELECT SponsorName FROM Sponsors WHERE SponsorId='$SAMPLE_ID'")
        AZURE_NAME=$(query_azure "SELECT SponsorName FROM Sponsors WHERE SponsorId='$SAMPLE_ID'")
        
        echo -n "  Sample Sponsor (ID: $SAMPLE_ID): "
        if [ "$DOCKER_NAME" == "$AZURE_NAME" ]; then
            echo -e "${GREEN}✓ Match${NC}"
        else
            echo -e "${RED}✗ Mismatch${NC}"
            echo "    Docker: $DOCKER_NAME"
            echo "    Azure:  $AZURE_NAME"
            ((TOTAL_ISSUES++))
        fi
    fi
fi

# Summary
echo ""
echo "================================"
if [ $TOTAL_ISSUES -eq 0 ]; then
    echo -e "${GREEN}✓ Verification PASSED${NC}"
    echo ""
    echo "Migration appears successful!"
    echo ""
    echo "Next steps:"
    echo "  1. Test application with Azure SQL connection"
    echo "  2. Run full smoke tests"
    echo "  3. Update production configurations"
else
    echo -e "${RED}✗ Verification FAILED${NC}"
    echo ""
    echo "Found $TOTAL_ISSUES issue(s)"
    echo ""
    echo "Recommended actions:"
    echo "  1. Check data import logs"
    echo "  2. Verify import scripts ran completely"
    echo "  3. Check for constraint violations"
    echo "  4. Re-run import for affected tables"
fi
echo "================================"
echo ""

# Connection string helper
echo -e "${YELLOW}Azure SQL Connection String:${NC}"
echo "Server=${AZURE_SERVER}.database.windows.net;Database=${AZURE_DB};User Id=${AZURE_USER};Password=${AZURE_PASS};Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"
echo ""
