# Sample Test Data Documentation
## ISM Sponsor Management System

**Version:** 1.0  
**Date:** March 2026  
**Purpose:** Reference for demo and UAT test data

---

## Overview

This document describes the demo/test data seeded by `DemoDataSeeder.cs` for UAT, training, and demonstrations. All data is de-identified and safe for pilot use.

---

## Demo User Accounts

### All Demo Users

| Email | Password | Role | Purpose |
|-------|----------|------|---------|
| demo.admin@ism.edu.ph | Demo@2026! | Admin | Full system testing, approval workflows, system configuration |
| demo.admissions@ism.edu.ph | Demo@2026! | Admissions | Sponsor/LoG creation, change request submission, admissions workflows |
| demo.cashier@ism.edu.ph | Demo@2026! | Cashier | Read-only access, reconciliation reports, coverage evaluation |
| demo.sponsor@ism.edu.ph | Demo@2026! | Sponsor | Placeholder for future sponsor self-service (currently read-only) |

**Note:** All users have the same password for demo convenience. In production, use strong unique passwords.

---

## Demo Sponsors

### DEMO-SP001: Global Tech Corporation

- **Sponsor ID:** DEMO-SP001
- **Sponsor Name:** Global Tech Corporation
- **Legal Name:** Global Tech Corporation Philippines Inc.
- **Contact Person:** Maria Santos
- **Contact Email:** maria.santos@globaltech.com
- **Contact Phone:** +63 2 8123 4567
- **Address:** 25th Floor, Corporate Tower, Bonifacio Global City, Taguig, Metro Manila 1634, Philippines
- **Status:** Active (IsActive = true)
- **Created:** 6 months ago
- **Sync Status:** Synced to PowerSchool (PS001) and NetSuite (NS001)
- **Students:** 2 (DEMO-ST001, DEMO-ST002)
- **Active LoGs:** 1 (DEMO-LOG001)
- **Use Cases:** Primary test sponsor for happy path scenarios, coverage evaluation demos

---

### DEMO-SP002: Asian Development Bank

- **Sponsor ID:** DEMO-SP002
- **Sponsor Name:** Asian Development Bank
- **Legal Name:** Asian Development Bank
- **Contact Person:** John Lee
- **Contact Email:** j.lee@adb.org
- **Contact Phone:** +63 2 8632 4444
- **Address:** 6 ADB Avenue, Mandaluyong City, Metro Manila 1550, Philippines
- **Status:** Active
- **Created:** 5 months ago
- **Sync Status:** Synced to PowerSchool (PS002) only
- **Students:** 2 (DEMO-ST003, DEMO-ST004)
- **Active LoGs:** 1 (DEMO-LOG002), 1 inactive LoG (DEMO-LOG005)
- **Use Cases:** Change request testing (approved request exists), multi-LoG scenario

---

### DEMO-SP003: Embassy of Canada

- **Sponsor ID:** DEMO-SP003
- **Sponsor Name:** Embassy of Canada
- **Legal Name:** Embassy of Canada to the Philippines
- **Contact Person:** Robert Johnson
- **Contact Email:** robert.johnson@canada.ca
- **Contact Phone:** +63 2 8857 9000
- **Address:** Tower 2, RCBC Plaza, 6819 Ayala Avenue, Makati, Metro Manila 1200, Philippines
- **Status:** Active
- **Created:** 4 months ago
- **Sync Status:** Synced to NetSuite (NS003) only
- **Students:** 1 (DEMO-ST005)
- **Active LoGs:** 1 (DEMO-LOG003 with NotCovered rules)
- **Use Cases:** NotCovered coverage type testing, rejected change request scenario

---

### DEMO-SP004: Manila Consulting Group

- **Sponsor ID:** DEMO-SP004
- **Sponsor Name:** Manila Consulting Group
- **Legal Name:** Manila Consulting Group Inc.
- **Contact Person:** Ana Reyes
- **Contact Email:** ana.reyes@manilacons.com
- **Contact Phone:** +63 2 8812 3456
- **Address:** 15th Floor, Zuellig Building, Makati Avenue, Makati, Metro Manila 1227, Philippines
- **Status:** Active
- **Created:** 3 months ago
- **Sync Status:** Not synced
- **Students:** 2 (DEMO-ST006, DEMO-ST007)
- **Active LoGs:** 0 (1 pending LoG: DEMO-LOG004)
- **Use Cases:** Pending LoG activation scenario, no sync testing

---

### DEMO-SP005: Pacific Resources Ltd

- **Sponsor ID:** DEMO-SP005
- **Sponsor Name:** Pacific Resources Ltd
- **Legal Name:** Pacific Resources Limited Philippines
- **Contact Person:** Michael Chen
- **Contact Email:** m.chen@pacificres.com
- **Contact Phone:** +63 2 8845 6789
- **Address:** Pacific Star Building, Makati Avenue corner Gil Puyat, Makati, Metro Manila 1200, Philippines
- **Status:** Inactive (IsActive = false)
- **Created:** 12 months ago
- **Deactivated:** 1 month ago
- **Sync Status:** Not synced
- **Students:** 1 (DEMO-ST008, also inactive)
- **Active LoGs:** 0
- **Use Cases:** Inactive sponsor testing, deactivation workflow

---

### DEMO-SP006: Global Tech Corp (Duplicate)

- **Sponsor ID:** DEMO-SP006
- **Sponsor Name:** Global Tech Corp
- **Legal Name:** Global Tech Corporation (duplicate)
- **Contact Person:** Maria Santos
- **Contact Email:** maria@globaltech.com
- **Contact Phone:** +63 2 8123 4500
- **Address:** BGC Corporate Tower, Taguig, Metro Manila 1634, Philippines
- **Status:** Active
- **Created:** 1 month ago
- **Sync Status:** Not synced
- **Students:** 0
- **Active LoGs:** 0
- **Use Cases:** Duplicate detection and merge testing (similar to DEMO-SP001)

---

## Demo Students

| Student ID | First Name | Last Name | Grade Level | Sponsor | School Year | Status |
|------------|------------|-----------|-------------|---------|-------------|--------|
| DEMO-ST001 | Emma | Wilson | Grade 5 | DEMO-SP001 | 2025-2026 | Active |
| DEMO-ST002 | Liam | Anderson | Grade 8 | DEMO-SP001 | 2025-2026 | Active |
| DEMO-ST003 | Sophia | Lee | Grade 3 | DEMO-SP002 | 2025-2026 | Active |
| DEMO-ST004 | Noah | Kim | Grade 11 | DEMO-SP002 | 2025-2026 | Active |
| DEMO-ST005 | Olivia | Johnson | Grade 6 | DEMO-SP003 | 2025-2026 | Active |
| DEMO-ST006 | Ethan | Martinez | Grade 9 | DEMO-SP004 | 2025-2026 | Active |
| DEMO-ST007 | Ava | Garcia | Grade 4 | DEMO-SP004 | 2025-2026 | Active |
| DEMO-ST008 | Mason | Chen | Grade 10 | DEMO-SP005 | 2025-2026 | Inactive |

**Use Cases:**
- DEMO-ST001/ST002: Coverage evaluation testing with DEMO-LOG001
- DEMO-ST003/ST004: Multi-student scenarios, different grade levels
- DEMO-ST008: Inactive student testing, withdrawal scenarios

---

## Demo Letters of Guarantee (LoGs)

### DEMO-LOG001 (Active, Full Coverage)

- **LoG ID:** DEMO-LOG001
- **Sponsor:** DEMO-SP001 (Global Tech Corporation)
- **School Year:** 2025-2026
- **Status:** Active (IsActive = true)
- **Created:** 5 months ago
- **Activated:** 5 months ago

**Coverage Rules:**

| Item Code | Item Name | Coverage Type | Sponsor % | Parent % |
|-----------|-----------|---------------|-----------|----------|
| TUITION-ES | Elementary School Tuition | Covered | 100% | 0% |
| TUITION-MS | Middle School Tuition | Covered | 100% | 0% |
| BUS-SERVICE | Bus Transportation | Split | 70% | 30% |
| LUNCH-PLAN | Lunch Plan | Split | 50% | 50% |

**Use Cases:** Covered and Split coverage type testing, typical LoG scenario

---

### DEMO-LOG002 (Active, Selective Coverage)

- **LoG ID:** DEMO-LOG002
- **Sponsor:** DEMO-SP002 (Asian Development Bank)
- **School Year:** 2025-2026
- **Status:** Active
- **Created:** 4 months ago
- **Activated:** 4 months ago

**Coverage Rules:**

| Item Code | Item Name | Coverage Type | Sponsor % | Parent % |
|-----------|-----------|---------------|-----------|----------|
| TUITION-ES | Elementary School Tuition | Covered | 100% | 0% |
| TUITION-HS | High School Tuition | Covered | 100% | 0% |
| TECHNOLOGY | Technology Fee | Covered | 100% | 0% |
| TEXTBOOK | Textbook Fee | Split | 80% | 20% |

**Use Cases:** Multiple covered items, different split percentage

---

### DEMO-LOG003 (Active, with NotCovered Items)

- **LoG ID:** DEMO-LOG003
- **Sponsor:** DEMO-SP003 (Embassy of Canada)
- **School Year:** 2025-2026
- **Status:** Active
- **Created:** 3 months ago
- **Activated:** 3 months ago

**Coverage Rules:**

| Item Code | Item Name | Coverage Type | Sponsor % | Parent % |
|-----------|-----------|---------------|-----------|----------|
| TUITION-MS | Middle School Tuition | Covered | 100% | 0% |
| BUS-SERVICE | Bus Transportation | NotCovered | 0% | 100% |
| UNIFORM | School Uniform | NotCovered | 0% | 100% |

**Use Cases:** NotCovered coverage type testing, selective coverage

---

### DEMO-LOG004 (UnderReview, Pending Activation)

- **LoG ID:** DEMO-LOG004
- **Sponsor:** DEMO-SP004 (Manila Consulting Group)
- **School Year:** 2025-2026
- **Status:** UnderReview (IsActive = false)
- **Created:** 7 days ago
- **Activated:** Not yet activated

**Coverage Rules:** None added yet

**Use Cases:** LoG activation workflow testing, pending LoG scenario

---

### DEMO-LOG005 (Inactive, Previous School Year)

- **LoG ID:** DEMO-LOG005
- **Sponsor:** DEMO-SP002 (Asian Development Bank)
- **School Year:** 2024-2025
- **Status:** Inactive (IsActive = false)
- **Created:** 12 months ago
- **Activated:** 12 months ago
- **Deactivated:** 2 months ago

**Use Cases:** Inactive LoG testing, historical LoG queries

---

## Demo Change Requests

### Request 1: Pending (demo.admissions submitted)

- **Change Request ID:** [Auto-generated GUID]
- **Sponsor:** DEMO-SP001 (Global Tech Corporation)
- **Field:** ContactEmail
- **Old Value:** maria.santos@globaltech.com
- **New Value:** m.santos@globaltech.ph
- **Justification:** "Updated corporate email domain"
- **Status:** pending
- **Requested By:** demo.admissions@ism.edu.ph
- **Requested On:** 3 days ago
- **Use Cases:** Pending request approval testing, Admin review workflow

---

### Request 2: Approved and Applied

- **Change Request ID:** [Auto-generated GUID]
- **Sponsor:** DEMO-SP002 (Asian Development Bank)
- **Field:** ContactPhone
- **Old Value:** +63 2 8632 4444
- **New Value:** +63 2 8632 5000
- **Justification:** "New direct line for education liaison"
- **Status:** approved (and applied)
- **Requested By:** demo.admissions@ism.edu.ph
- **Requested On:** 10 days ago
- **Reviewed By:** demo.admin@ism.edu.ph
- **Reviewed On:** 9 days ago
- **Review Comments:** "Approved. Contact confirmed via email."
- **Applied On:** 8 days ago
- **Use Cases:** Successful approval workflow, applied change verification

---

### Request 3: Rejected

- **Change Request ID:** [Auto-generated GUID]
- **Sponsor:** DEMO-SP003 (Embassy of Canada)
- **Field:** AddressLine1
- **Old Value:** Tower 2, RCBC Plaza
- **New Value:** Tower 3, RCBC Plaza
- **Justification:** "Embassy moved to adjacent tower"
- **Status:** rejected
- **Requested By:** demo.admissions@ism.edu.ph
- **Requested On:** 15 days ago
- **Reviewed By:** demo.admin@ism.edu.ph
- **Reviewed On:** 14 days ago
- **Review Comments:** "Rejected. Official embassy records still show Tower 2. Please provide official notice."
- **Use Cases:** Rejection workflow testing, requester notification

---

### Request 4: Recent Pending

- **Change Request ID:** [Auto-generated GUID]
- **Sponsor:** DEMO-SP004 (Manila Consulting Group)
- **Field:** ContactPerson
- **Old Value:** Ana Reyes
- **New Value:** Carlos Mendoza
- **Justification:** "Change in HR contact person"
- **Status:** pending
- **Requested By:** demo.admissions@ism.edu.ph
- **Requested On:** 1 day ago
- **Use Cases:** Recent pending request testing, dashboard widget verification

---

## Demo Sync Logs

**9 Sync Log Entries:**

- 5 successful syncs (PowerSchool and NetSuite)
- 2 failed syncs (PowerSchool connection timeout, NetSuite validation error)
- 2 recent syncs (within last 10 days)

**Use Cases:** Sync status reporting, sync failure investigation, operations dashboard metrics

---

## Demo Audit Logs

**9 Audit Log Entries:**

- Sponsor creation logs
- LoG creation and activation logs
- Change request submission and approval logs

**Use Cases:** Audit retrieval testing, filtering by module/user/date, compliance verification

---

## Demo Coverage Evaluation Audits

**8 Coverage Evaluation Audit Entries:**

- Covered decisions (100% sponsor)
- Split decisions (70/30, 80/20)
- NotCovered decisions (100% parent)
- 1 failed evaluation (no active LoG found)

**Use Cases:** Coverage evaluation verification, reconciliation reporting, decision auditing

---

## Demo Items (Fee Codes)

| Item Code | Item Name | Category | Status |
|-----------|-----------|----------|--------|
| TUITION-ES | Elementary School Tuition | Tuition | Active |
| TUITION-MS | Middle School Tuition | Tuition | Active |
| TUITION-HS | High School Tuition | Tuition | Active |
| BUS-SERVICE | Bus Transportation | Transportation | Active |
| LUNCH-PLAN | Lunch Plan | Food | Active |
| FIELD-TRIP | Field Trip Fee | Activity | Active |
| TECHNOLOGY | Technology Fee | Fee | Active |
| UNIFORM | School Uniform | Material | Active |
| TEXTBOOK | Textbook Fee | Material | Active |
| LAB-FEE | Laboratory Fee | Fee | Active |

**Use Cases:** Coverage rule creation, item selection, fee code lookups

---

## Demo School Years

| School Year ID | Start Date | End Date | Status |
|----------------|------------|----------|--------|
| 2024-2025 | Aug 1, 2024 | Jun 30, 2025 | Inactive (past) |
| 2025-2026 | Aug 1, 2025 | Jun 30, 2026 | Active (current) |
| 2026-2027 | Aug 1, 2026 | Jun 30, 2027 | Inactive (future) |

**Use Cases:** School year selector testing, LoG filtering by school year

---

## How to Re-Seed Demo Data

If demo data needs to be refreshed or re-seeded:

1. **Clear existing demo data** (optional, if starting fresh):
   ```sql
   DELETE FROM Students WHERE StudentId LIKE 'DEMO-%';
   DELETE FROM LogCoverages WHERE LogId LIKE 'DEMO-%';
   DELETE FROM Sponsors WHERE SponsorId LIKE 'DEMO-%';
   DELETE FROM AspNetUsers WHERE Email LIKE 'demo.%@ism.edu.ph';
   -- Clear related tables (ChangeRequests, SyncLogs, ActivityLogs, etc.)
   ```

2. **Run DemoDataSeeder**:
   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
       await seeder.SeedDemoDataAsync();
   }
   ```

3. **Verify demo data seeded**:
   - Log in as demo.admin@ism.edu.ph
   - Check Sponsors list shows 6 demo sponsors
   - Check Students list shows 8 demo students
   - Check LoGs list shows 5 demo LoGs

---

## Important Notes

- **Demo data prefix:** All demo entities use "DEMO-" prefix for easy identification
- **Demo user passwords:** All demo users have the same password (`Demo@2026!`) for convenience
- **De-identified data:** All sponsor names, contacts, addresses are fictional or use public information (e.g., embassies)
- **Safe for demos:** Demo data is safe to show in presentations, training, and UAT
- **Re-seeding:** Demo data seeder checks if data already exists to avoid duplicates
- **Production:** Do NOT run DemoDataSeeder in production environment

---

**End of Sample Test Data Documentation**
