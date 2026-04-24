# Demo Data Setup Guide

## ISM Sponsor Management System - Comprehensive Demo Data

This guide explains how to populate the database with comprehensive demo data for UAT testing and demonstrations.

---

## 📊 What Demo Data Gets Created

### **Medium Demo Data** (Recommended for UAT)

| Entity | Count | Details |
|--------|-------|---------|
| **Sponsors** | 10 | DEMO-SP001 through DEMO-SP010 |
| **Students** | 100 | DEMO-ST001 through DEMO-ST100 (10 per sponsor) |
| **Letters of Guarantee** | 100 | One per student, with coverage rules |
| **Coverage Rules** | 200-250 | 2-3 rules per LoG (tuition, books, uniforms) |
| **Users** | 20 | 5 per role (Admin, Admissions, Cashier, Sponsor) |
| **Items** | 8 | Essential fees: tuition, books, uniforms, transport, meals |
| **School Years** | 3 | 2024-2025, 2025-2026 (active), 2026-2027 |

### **User Accounts Created**

All demo users have the password: **`DemoPass123!`**

- **Admins:** demo.admin1 through demo.admin5@ismanila.org
- **Admissions:** demo.admissions1 through demo.admissions5@ismanila.org  
- **Cashiers:** demo.cashier1 through demo.cashier5@ismanila.org
- **Sponsors:** demo.sponsor1 through demo.sponsor5@ismanila.org

---

## 🖥️ Local Environment (In-Memory Database)

The local development environment automatically seeds demo data on startup.

### **Automatic Seeding (Enabled by Default)**

1. **Verify configuration** - Check [appsettings.Development.json](appsettings.Development.json):
   ```json
   "Database": {
     "RunMigrationsOnStartup": true,
     "SeedDemoData": true
   }
   ```

2. **Start the application:**
   ```bash
   dotnet run
   ```

3. **What happens automatically:**
   - ✅ In-memory database created
   - ✅ Tables created from migrations
   - ✅ 10 sponsors created
   - ✅ 100 students created (10 per sponsor)
   - ✅ 100 LoGs with coverage rules created
   - ✅ 20 demo users created (5 per role)
   - ✅ Essential items created

4. **Verify data:** Open browser to http://localhost:5000
   - Login with any demo user (password: `DemoPass123!`)
   - Navigate to respective pages to see data

### **Disable Auto-Seeding (If Needed)**

1. Edit [appsettings.Development.json](appsettings.Development.json):
   ```json
   "Database": {
     "RunMigrationsOnStartup": true,
     "SeedDemoData": false
   }
   ```

2. Restart application

---

## ☁️ Azure Environment (Azure SQL Database)

Azure requires **manual execution** of SQL scripts because:
- Persistent database (doesn't reset on restart)
- Multiple instances might run concurrently
- Production-safe seeding approach

### **Option 1: Azure Portal Query Editor** (Recommended)

**Step 1:** Clean existing demo data (optional but recommended)
1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to your SQL Database → **Query editor (preview)**
3. Login with SQL authentication
4. Open [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql)
5. Copy entire contents and paste into Query Editor
6. Click **Run**
7. Verify output shows "All demo data successfully removed!"

**Step 2:** Seed medium demo data
1. In the same Query Editor
2. Open [scripts/seed_medium_demo_data.sql](scripts/seed_medium_demo_data.sql)
3. Copy entire contents and paste
4. Click **Run**
5. ⏱️ **Wait ~10-15 seconds** for completion

**Step 3:** Verify creation
Expected output:
```
Medium Demo Data Setup Complete!
===============================================

Sponsors: 10
Students: 100
Letters of Guarantee: 100
Coverage Rules: 200+
Demo Users: 20
Active Items: 8

✅ Demo data ready for UAT testing!
```

⚠️ **User Passwords:** Azure SQL script creates user records **without passwords**. You must set passwords via:
- Admin UI: Admin → Users → Reset Password for each demo user
- Or run password hash script (advanced - requires C# code execution)

**Password Workaround:** Use the Admin UI to set password `DemoPass123!` for all demo users.

---

### **Option 2: SQL Server Management Studio (SSMS)**

**Prerequisites:**
- SSMS installed on Windows
- Azure SQL firewall rule for your IP

**Steps:**
1. Open SSMS
2. Connect to Azure SQL:
   - **Server:** yourserver.database.windows.net
   - **Authentication:** SQL Server Authentication
   - **Login/Password:** Your admin credentials
3. Open [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql) → Execute (F5)
4. Open [scripts/seed_medium_demo_data.sql](scripts/seed_medium_demo_data.sql) → Execute (F5)
5. Review messages tab for confirmation

---

### **Option 3: Azure CLI + sqlcmd**

```bash
# Ensure logged in
az login

# Clean existing demo data
sqlcmd -S yourserver.database.windows.net \
  -d ISMSponsorDB \
  -U youradmin \
  -P 'yourpassword' \
  -i scripts/clean_demo_data.sql

# Seed medium demo data
sqlcmd -S yourserver.database.windows.net \
  -d ISMSponsorDB \
  -U youradmin \
  -P 'yourpassword' \
  -i scripts/seed_medium_demo_data.sql
```

---

## 🔐 Setting Demo User Passwords (Azure)

Since SQL scripts cannot hash passwords like Identity framework, you need to set passwords manually:

### **Method 1: Admin UI (Recommended)**

1. Login to Azure app as Admin (use non-demo admin account)
2. Navigate to **Admin → Users**
3. For each demo user (demo.admin1, demo.admissions1, etc.):
   - Click "Edit" or "Reset Password"
   - Set password to: `DemoPass123!`
   - Save

Repeat for all 20 demo users.

### **Method 2: Bulk Password Script** (Advanced)

Create a one-time startup task in your Azure App Service that sets demo passwords. This requires code changes and redeployment.

---

## ✅ Verification Steps

### **Local Environment**

1. Restart application: `dotnet run`
2. Check startup logs for:
   ```
   ========================================
   Starting MEDIUM demo data seed...
   ========================================
   📅 Setting up school years...
   ...
   ✅ Medium demo data seed completed!
   ```
3. Open browser: http://localhost:5000
4. Login with: `demo.admin1@ismanila.org` / `DemoPass123!`
5. Navigate to:
   - **Admin → Sponsors** - Should see 10 DEMO-SP sponsors
   - **Admin → Students** - Should see 100 DEMO-ST students
   - **Admissions → LoGs** - Should see 100 Letters of Guarantee
   - **Admin → Users** - Should see 20 demo users

### **Azure Environment**

1. Open: https://ismsponsor.azurewebsites.net
2. Login with demo account (ensure password set via Admin UI)
3. Verify same navigation checks as local
4. Check database directly:
   ```sql
   SELECT 
       'Sponsors' AS Type, COUNT(*) AS Count 
   FROM Sponsors WHERE SponsorId LIKE 'DEMO-%'
   UNION ALL
   SELECT 'Students', COUNT(*) 
   FROM Students WHERE StudentId LIKE 'DEMO-%'
   UNION ALL
   SELECT 'LoGs', COUNT(*) 
   FROM LogCoverages WHERE SponsorId LIKE 'DEMO-%';
   ```

Expected results: 10, 100, 100

---

## 🔄 Refreshing Demo Data

### **Local (Easy)**
Just restart the application - in-memory database auto-resets and reseeds.

```bash
# Press Ctrl+C to stop
# Then restart:
dotnet run
```

### **Azure (Manual)**

1. Run cleanup script: [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql)
2. Run seed script: [scripts/seed_medium_demo_data.sql](scripts/seed_medium_demo_data.sql)
3. Reset demo user passwords via Admin UI

---

## 🛠️ Customizing Demo Data

### **Change Quantities**

Edit [Data/MediumDemoDataSeeder.cs](Data/MediumDemoDataSeeder.cs):

```csharp
// Line ~221: Change number of sponsors
for (int i = 0; i < sponsorData.Length; i++)  // Currently 10

// Line ~298: Change number of students
for (int i = 1; i <= 100; i++)  // Currently 100

// Line ~178: Change users per role
for (int i = 1; i <= 5; i++)  // Currently 5 per role
```

Recompile and run locally to test.

### **Change Items**

Edit [Data/MediumDemoDataSeeder.cs](Data/MediumDemoDataSeeder.cs) line ~145:

```csharp
var items = new[]
{
    new Item { ItemId = "CUSTOM-01", ItemName = "Custom Item", ... },
    // Add more items here
};
```

### **For Azure (SQL Scripts)**

Edit [scripts/seed_medium_demo_data.sql](scripts/seed_medium_demo_data.sql):

```sql
-- Change sponsor count: Line ~140
WHILE @i <= 10  -- Change to desired count

-- Change student count: Line ~220
WHILE @i <= 100  -- Change to desired count

-- Add custom items: Line ~90
INSERT INTO Items (Code, Name, Category, DefaultAmount, IsActive)
VALUES ('YOUR-CODE', 'Your Item', 'CATEGORY', 1500.00, 1);
```

---

## 📁 Files Reference

### **Code Files**
- [Data/MediumDemoDataSeeder.cs](Data/MediumDemoDataSeeder.cs) - C# seeder for local
- [Program.cs](Program.cs) - Startup integration (line 261, line 433)
- [appsettings.Development.json](appsettings.Development.json) - Local config
- [appsettings.Pilot.json](appsettings.Pilot.json) - Azure config

### **SQL Scripts**
- [scripts/seed_medium_demo_data.sql](scripts/seed_medium_demo_data.sql) - Azure seeding script
- [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql) - Cleanup script

### **Documentation**
- [docs/DATABASE_CLEANUP_GUIDE.md](docs/DATABASE_CLEANUP_GUIDE.md) - Database reset guide
- [docs/DEMO_DATA_STATUS_SUMMARY.md](docs/DEMO_DATA_STATUS_SUMMARY.md) - Historical status

---

## 🚨 Troubleshooting

### **Issue: Demo data not appearing (Local)**

**Check 1:** Verify SeedDemoData flag
```json
// appsettings.Development.json
"Database": { "SeedDemoData": true }
```

**Check 2:** Check startup logs
```bash
dotnet run
# Look for:
# "Starting MEDIUM demo data seed..."
```

**Check 3:** Restart application
```bash
# Ctrl+C to stop, then:
dotnet run
```

---

### **Issue: Duplicate key errors (Local)**

**Cause:** In-memory DB already has demo data from previous run  
**Solution:** Restart application to clear in-memory DB

---

### **Issue: SQL script fails on Azure**

**Error:** "Cannot insert duplicate key..."  
**Solution:** Run [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql) first

**Error:** "Login failed..."  
**Solution:** 
1. Check Azure SQL firewall rules
2. Verify SQL admin credentials
3. Add your IP to firewall allowlist

---

### **Issue: Demo users cannot login (Azure)**

**Cause:** SQL script creates users without password hashes  
**Solution:** Set passwords via Admin UI:
1. Login as non-demo admin
2. Admin → Users → Select demo user → Reset Password
3. Set to `DemoPass123!`

---

### **Issue: Build errors after adding MediumDemoDataSeeder**

**Solution:** Check compilation errors
```bash
dotnet build
# Fix any missing using statements or typos
```

Common fixes:
- Ensure `using ISMSponsor.Models.Domain;` is present
- Verify DbContext has all required DbSets
- Check model property names match database schema

---

## 📊 Quick Reference

| Task | Local | Azure |
|------|-------|-------|
| **Clean database** | Restart app | Run `clean_demo_data.sql` |
| **Seed demo data** | Automatic on startup | Run `seed_medium_demo_data.sql` |
| **Verify data** | http://localhost:5000 | https://ismsponsor.azurewebsites.net |
| **Time to complete** | 5-10 seconds | 10-15 seconds |
| **Enable seeding** | `SeedDemoData: true` | Run SQL script manually |
| **Disable seeding** | `SeedDemoData: false` | Don't run script |
| **Reset passwords** | Automatic | Manual via Admin UI |

---

## 🎯 UAT Testing Workflow

1. **Clean database** (both local and Azure)
2. **Seed medium demo data**
3. **Set demo user passwords** (Azure only)
4. **Verify data appears** in UI
5. **Execute UAT scripts:**
   - [tests/UAT_Admin_Role.md](tests/UAT_Admin_Role.md)
   - [tests/UAT_Admissions_Role.md](tests/UAT_Admissions_Role.md)
   - [tests/UAT_Cashier_Role.md](tests/UAT_Cashier_Role.md)
   - [tests/UAT_Sponsor_Role.md](tests/UAT_Sponsor_Role.md)

---

**Last Updated:** April 24, 2026  
**Status:** ✅ Ready for UAT testing  
**Demo Data Size:** Medium (10 sponsors, 100 students, 20 users)
