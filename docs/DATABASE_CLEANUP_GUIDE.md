# Database Cleanup Guide

## ISM Sponsor Demo Data Management

This guide explains how to clean/reset the database for both local and Azure environments.

---

## 🖥️ Local Environment (In-Memory Database)

The local development environment uses an **in-memory database** that automatically resets on application restart.

### Clean Local Database

Simply restart the application:

```bash
# Stop the running application (Ctrl+C in terminal)
# Then restart:
dotnet run
```

**What happens:**
- ✅ In-memory database is completely cleared
- ✅ All tables recreated from migrations
- ✅ Demo data reseeded automatically (if `SeedDemoData: true` in appsettings.Development.json)
- ⏱️ Takes: ~5-10 seconds

**Verify Clean State:**
```bash
# Check home page loads (should show 0 or fresh data)
curl http://localhost:5000

# Or open browser:
open http://localhost:5000
```

---

## ☁️ Azure Environment (Azure SQL Database)

Azure uses **persistent Azure SQL Database** - requires SQL script execution to clean data.

### Option 1: Azure Portal Query Editor (Recommended)

**Step 1:** Navigate to Azure SQL Database
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **Resource Groups** → Your resource group → **SQL Database**
3. Click on your ISM Sponsor database

**Step 2:** Open Query Editor
1. In left sidebar, click **Query editor (preview)**
2. Login using:
   - **Authentication type:** SQL server authentication
   - **Login:** Your admin username
   - **Password:** Your admin password

**Step 3:** Execute Cleanup Script
1. Open file: `/scripts/clean_demo_data.sql`
2. Copy entire contents (Ctrl+A, Ctrl+C)
3. Paste into Query Editor
4. Click **Run**

**Expected Output:**
```
Starting demo data cleanup...

=== Current Demo Data ===
Found:
  - Sponsors: 6
  - Students: 200
  - LoGs: 200
  - Coverage Rules: 600

Deleting LoG coverage rules...
  ✓ Deleted 600 coverage rules
Deleting Letters of Guarantee...
  ✓ Deleted 200 LoGs
Deleting students...
  ✓ Deleted 200 students
...

✅ All demo data successfully removed!
Database is clean and ready for fresh demo data.
```

⏱️ **Execution Time:** 2-5 seconds

---

### Option 2: SQL Server Management Studio (SSMS)

**Prerequisites:**
- SSMS installed on Windows machine
- Azure SQL firewall rule allowing your IP

**Steps:**
1. Open SSMS
2. Connect to Azure SQL:
   - **Server name:** `yourserver.database.windows.net`
   - **Authentication:** SQL Server Authentication
   - **Login / Password:** Your credentials
3. Open `/scripts/clean_demo_data.sql`
4. Click **Execute** (F5)

---

### Option 3: Command Line (Azure CLI + sqlcmd)

```bash
# Ensure you're logged in to Azure
az login

# Execute cleanup script
sqlcmd -S yourserver.database.windows.net \
  -d ISMSponsorDB \
  -U youradmin \
  -P 'yourpassword' \
  -i scripts/clean_demo_data.sql
```

---

## 🔍 What Gets Deleted

The cleanup script removes **ONLY demo/test data**:

### ✅ Removed:
- Students with ID prefix `DEMO-*`
- Sponsors with ID prefix `DEMO-*`
- All Letters of Guarantee linked to demo sponsors
- All LoG Coverage Rules linked to demo LoGs
- Demo user accounts (email: `demo.*@ismanila.org`)
- Sponsor change requests for demo sponsors

### ✅ Preserved:
- School years (2024-2025, 2025-2026, etc.)
- Items and categories (tuition, uniforms, etc.)
- AspNet roles (Admin, Admissions, Cashier, Sponsor)
- Regular production data (non-DEMO prefixed)

---

## ✅ Verify Cleanup Success

### Azure Portal Check:
After running cleanup script, execute verification query:

```sql
-- Count remaining demo data
SELECT 
    'Sponsors' AS EntityType,
    COUNT(*) AS RemainingCount
FROM Sponsors
WHERE SponsorId LIKE 'DEMO-%'

UNION ALL

SELECT 
    'Students',
    COUNT(*)
FROM Students
WHERE StudentId LIKE 'DEMO-%'

UNION ALL

SELECT 
    'LoGs',
    COUNT(*)
FROM LogCoverages
WHERE SponsorId LIKE 'DEMO-%';
```

**Expected Result:** All counts should be **0**

### Application Check:
1. Navigate to: https://ismsponsor.azurewebsites.net
2. Login as Admin
3. Go to **Admin → Sponsors** page
4. Should see NO sponsors with "DEMO-" prefix

---

## 📊 After Cleanup - Next Steps

Once database is clean, you can:

1. **Reseed Fresh Demo Data:**
   - Execute `/scripts/seed_200_students.sql` (for 200 students)
   - Or restart local app (auto-seeds demo data)

2. **Manual Data Entry:**
   - Use Admin UI to create sponsors, students, LoGs
   - Create test users via Admin → Users page

3. **Import from CSV:**
   - Use bulk import features (if implemented)

---

## ⚠️ Safety Notes

### Local Environment:
- ✅ Safe to reset anytime (data is temporary)
- ✅ No risk of production data loss
- ✅ Quick recovery (auto-reseed on startup)

### Azure Environment:
- ⚠️ **BACKUP FIRST** if you have important test data
- ⚠️ Script targets DEMO-* prefixes only
- ⚠️ Verify you're connected to correct database
- ⚠️ Cannot undo deletion without backup

### Production Safety:
The cleanup script is designed to be **production-safe**:
- Only deletes records with `DEMO-*` prefix
- Does NOT use `TRUNCATE` (no full table clears)
- Does NOT delete system tables
- Does NOT affect real user data

---

## 🆘 Troubleshooting

### Issue: "Cannot delete referenced data"
**Cause:** Foreign key constraints preventing deletion  
**Solution:** Script handles this by deleting in correct order (rules → LoGs → students → sponsors)

### Issue: "Login failed" in Azure Query Editor
**Cause:** Incorrect credentials or firewall block  
**Solution:** 
1. Verify SQL admin credentials in Azure Portal
2. Add your IP to firewall: SQL Server → Firewalls and virtual networks → Add client IP

### Issue: Script timeout
**Cause:** Large amount of demo data  
**Solution:** Execute in smaller batches or increase query timeout in SSMS

### Issue: Demo data still appears after cleanup
**Cause:** Azure app cache  
**Solution:** Restart Azure App Service:
```bash
az webapp restart --name ismsponsor --resource-group YourResourceGroup
```

---

## 📝 Quick Reference

| Environment | Method | Time | Command/Tool |
|-------------|--------|------|--------------|
| **Local** | Restart app | 5-10s | `dotnet run` |
| **Azure** | SQL script | 2-5s | Azure Portal Query Editor |
| **Azure** | SQL script | 2-5s | SSMS |
| **Azure** | sqlcmd | 5-10s | `sqlcmd -S ... -i clean_demo_data.sql` |

---

## Files Reference

- **Cleanup Script:** [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql)
- **Bulk Insert Script:** [scripts/seed_200_students.sql](scripts/seed_200_students.sql)
- **Demo Seeder Code:** [Data/DemoDataSeeder.cs](Data/DemoDataSeeder.cs)

---

**Last Updated:** April 24, 2026  
**Status:** ✅ Ready for use in local and Azure environments
