# Demo Data Creation Summary

## ✅ Successfully Created - April 24, 2026

Comprehensive demo data setup for **both local and Azure environments** is now ready!

---

## 📦 What Was Created

### **1. Code Files**

#### **MediumDemoDataSeeder.cs**
- **Location:** [Data/MediumDemoDataSeeder.cs](Data/MediumDemoDataSeeder.cs)
- **Purpose:** C# class that seeds demo data for local in-memory database
- **Creates:**
  - 10 sponsors (DEMO-SP001 to DEMO-SP010)
  - 100 students (DEMO-ST001 to DEMO-ST100, 10 per sponsor)
  - 100 Letters of Guarantee with 200+ coverage rules
  - 20 demo users (5 per role: Admin, Admissions, Cashier, Sponsor)
  - 8 essential items (tuition, books, uniforms, etc.)
  - 3 school years (2024-2025, 2025-2026 active, 2026-2027)
- **Status:** ✅ Compiles successfully, integrated into Program.cs

#### **Program.cs Updates**
- **Lines Modified:** 261, 433-450
- **Changes:**
  - Registered `MediumDemoDataSeeder` in DI container
  - Added logic to check `Database:SeedDemoData` configuration
  - Automatically calls seeder on startup when enabled
- **Status:** ✅ Build verified successful

---

### **2. SQL Scripts**

#### **seed_medium_demo_data.sql**
- **Location:** [scripts/seed_medium_demo_data.sql](scripts/seed_medium_demo_data.sql)
- **Purpose:** Bulk insert script for Azure SQL Database
- **Size:** ~640 lines of T-SQL
- **Creates:** Same entities as C# seeder (10 sponsors, 100 students, etc.)
- **Execution Time:** ~10-15 seconds
- **Status:** ✅ Ready for Azure Portal Query Editor

#### **clean_demo_data.sql**
- **Location:** [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql)
- **Purpose:** Safely removes all demo data (DEMO-* prefixed)  
- **Safety:** Only deletes demo records, preserves production data
- **Execution Time:** ~2-5 seconds
- **Status:** ✅ Ready to use

---

### **3. Documentation**

#### **DEMO_DATA_SETUP_GUIDE.md**
- **Location:** [docs/DEMO_DATA_SETUP_GUIDE.md](docs/DEMO_DATA_SETUP_GUIDE.md)
- **Contents:**
  - Complete setup instructions (local + Azure)
  - User account details (20 demo users)
  - Verification steps
  - Troubleshooting guide
  - Customization options
- **Status:** ✅ Ready for UAT team reference

#### **DATABASE_CLEANUP_GUIDE.md**
- **Location:** [docs/DATABASE_CLEANUP_GUIDE.md](docs/DATABASE_CLEANUP_GUIDE.md)  
- **Purpose:** How to reset/clean database for fresh demos
- **Status:** ✅ Available

---

## 🚀 How to Use

### **Local Environment (In-Memory DB)**

1. **Verify configuration** in [appsettings.Development.json](appsettings.Development.json):
   ```json
   "Database": {
     "RunMigrationsOnStartup": true,
     "SeedDemoData": true
   }
   ```

2. **Start application:**
   ```bash
   dotnet run
   ```

3. **Verify demo data** - Check startup logs for:
   ```
   ========================================
   Starting MEDIUM demo data seed...
   ========================================
   ...
   ✅ Medium demo data seed completed!
   ========================================
   ```

4. **Login and verify:**
   - URL: http://localhost:5000
   - User: `demo.admin1@ismanila.org`
   - Password: `DemoPass123!`
   - Navigate to Admin → Sponsors (should see 10 DEMO-SP sponsors)

---

### **Azure Environment (Azure SQL)**

1. **Navigate to Azure Portal** → SQL Database → **Query editor (preview)**

2. **(Optional) Clean existing demo data:**
   - Copy contents of [scripts/clean_demo_data.sql](scripts/clean_demo_data.sql)
   - Paste into Query Editor → Run
   - Verify "All demo data successfully removed!"

3. **Seed demo data:**
   - Copy contents of [scripts/seed_medium_demo_data.sql](scripts/seed_medium_demo_data.sql)
   - Paste into Query Editor → Run  
   - Wait ~10-15 seconds

4. **⚠️ IMPORTANT - Set User Passwords:**
   SQL script creates user records **without passwords**. Manual setup required:
   - Login to https://ismsponsor.azurewebsites.net as non-demo admin
   - Go to Admin → Users
   - For each demo user (demo.admin1, demo.admissions1, etc.):
     - Click Edit/Reset Password
     - Set password to: `DemoPass123!`
   - Repeat for all 20 users

5. **Verify:**
   - Login with demo user
   - Check Admin → Sponsors (should see 10 DEMO-SP sponsors)

---

## 👥 Demo User Accounts

All users have the same password: **`DemoPass123!`**

### **Admins (5)**
- demo.admin1@ismanila.org
- demo.admin2@ismanila.org
- demo.admin3@ismanila.org
- demo.admin4@ismanila.org
- demo.admin5@ismanila.org

### **Admissions Staff (5)**
- demo.admissions1@ismanila.org
- demo.admissions2@ismanila.org
- demo.admissions3@ismanila.org
- demo.admissions4@ismanila.org
- demo.admissions5@ismanila.org

### **Cashiers (5)**
- demo.cashier1@ismanila.org
- demo.cashier2@ismanila.org
- demo.cashier3@ismanila.org
- demo.cashier4@ismanila.org
- demo.cashier5@ismanila.org

### **Sponsors (5)**
- demo.sponsor1@ismanila.org
- demo.sponsor2@ismanila.org
- demo.sponsor3@ismanila.org
- demo.sponsor4@ismanila.org
- demo.sponsor5@ismanila.org

---

## 📊 Demo Data Structure

| Entity | Count | ID Pattern | Notes |
|--------|-------|------------|-------|
| **Sponsors** | 10 | DEMO-SP001 to DEMO-SP010 | Tech Innovations, Global Finance, etc. |
| **Students** | 100 | DEMO-ST001 to DEMO-ST100 | 10 students per sponsor |
| **Letters of Guarantee** | 100 | Auto-generated IDs | One per student |
| **Coverage Rules** | 200+ | Auto-generated IDs | 2-3 rules per LoG |
| **Items** | 8 | TUITION-ELEM, BOOKS, etc. | Essential school fees |
| **School Years** | 3 | 2024-2025, 2025-2026, 2026-2027 | 2025-2026 is active |

---

## ✅ Build Status

```
Build succeeded with 2 warning(s)
Status: ✅ Ready to use
Warnings: Minor null reference warnings (non-blocking)
```

---

## 📝 Configuration Files

### **appsettings.Development.json**
```json
"Database": {
  "RunMigrationsOnStartup": true,
  "SeedDemoData": true  ← Enables auto-seeding
}
```

### **appsettings.Pilot.json** (Azure)
```json
"Database": {
  "RunMigrationsOnStartup": false,
  "SeedDemoData": false  ← Manual SQL script execution
}
```

---

## 🔄 Refreshing Demo Data

### **Local**
```bash
# Just restart the application
# Ctrl+C to stop, then:
dotnet run
```

### **Azure**
```sql
-- 1. Run cleanup script
/scripts/clean_demo_data.sql

-- 2. Run seed script
/scripts/seed_medium_demo_data.sql

-- 3. Reset user passwords via Admin UI
```

---

## 🎯 Next Steps for UAT

1. ✅ **Demo data created** (this step complete)
2. ⏭️ **Test local environment** - Verify all data appears
3. ⏭️ **Deploy to Azure** - Run SQL scripts on Azure SQL
4. ⏭️ **Set Azure user passwords** - via Admin UI
5. ⏭️ **Execute UAT scripts:**
   - [tests/UAT_Admin_Role.md](tests/UAT_Admin_Role.md)
   - [tests/UAT_Admissions_Role.md](tests/UAT_Admissions_Role.md)
   - [tests/UAT_Cashier_Role.md](tests/UAT_Cashier_Role.md)
   - [tests/UAT_Sponsor_Role.md](tests/UAT_Sponsor_Role.md)

---

## 📚 Reference Documentation

- **Setup Guide:** [docs/DEMO_DATA_SETUP_GUIDE.md](docs/DEMO_DATA_SETUP_GUIDE.md)
- **Cleanup Guide:** [docs/DATABASE_CLEANUP_GUIDE.md](docs/DATABASE_CLEANUP_GUIDE.md)
- **UAT Scripts:** `tests/UAT_*.md` files

---

## 🆘 Quick Troubleshooting

| Issue | Solution |
|-------|----------|
| Demo data not appearing (local) | Verify `SeedDemoData: true`, restart app |
| Build errors | Run `dotnet build ISMSponsor.csproj` |
| Azure users can't login | Set passwords via Admin UI |
| Duplicate key errors | Run `clean_demo_data.sql` first |
| SQL script timeout | Execute in smaller parts or via SSMS |

---

**Status:** ✅ **READY FOR DEPLOYMENT AND UAT**  
**Last Updated:** April 24, 2026 16:45  
**Build:** Successful with 2 warnings (non-blocking)
