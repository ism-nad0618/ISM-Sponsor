# Pilot Readiness Checklist
## ISM Sponsor Management System

**Version:** 1.0  
**Date:** March 2026  
**Purpose:** Final verification before pilot deployment

---

## Checklist Overview

This checklist ensures all requirements for pilot deployment are met. Complete all items before deploying to staging/pilot environment.

**Completion Target:** 100% (all items checked)  
**Critical Items:** Items marked with ⚠️ are blocking for pilot deployment

---

## 1. Technical Infrastructure

### 1.1 Application Build and Deployment

- [ ] ⚠️ **Application builds successfully with 0 errors**
  - Build command: `dotnet build ISMSponsor.csproj`
  - Expected: Build succeeded, 0 Warning(s), 0 Error(s)
  - Verified by: __________ Date: __________

- [ ] ⚠️ **Application publishes successfully**
  - Publish command: `dotnet publish -c Release -o ./publish`
  - Expected: Publish succeeded
  - Verified by: __________ Date: __________

- [ ] ⚠️ **All database migrations applied successfully**
  - Migration command: `dotnet ef database update`
  - Expected: All migrations applied, no errors
  - Verified by: __________ Date: __________

- [ ] **Application starts without errors**
  - Start command: `dotnet run` or deploy to app service
  - Expected: Application listening on configured port, no startup errors
  - Verified by: __________ Date: __________

---

### 1.2 Environment Configuration

- [ ] ⚠️ **Production connection string configured**
  - Location: `appsettings.Production.json` or Azure App Service configuration
  - Connection string points to production/staging SQL Server
  - Verified by: __________ Date: __________

- [ ] ⚠️ **Database accessible from application server**
  - Test: Application can connect and query database
  - Expected: No connection errors in logs
  - Verified by: __________ Date: __________

- [ ] **HTTPS configured and working**
  - Test: Navigate to https://[app-url]
  - Expected: Valid SSL certificate, no browser warnings
  - Verified by: __________ Date: __________

- [ ] **Environment variables set correctly**
  - Check: `ASPNETCORE_ENVIRONMENT = Production`
  - Check: Other required environment variables
  - Verified by: __________ Date: __________

---

### 1.3 Database and Data

- [ ] ⚠️ **Initial seed data loaded (roles, admin user, school years, items)**
  - Verified: Admin account exists (admin@ism.edu.ph)
  - Verified: Roles exist (Admin, Admissions, Cashier, Sponsor)
  - Verified: School year 2025-2026 exists and is active
  - Verified: Sample items (fee codes) exist
  - Verified by: __________ Date: __________

- [ ] **Demo data seeded for UAT (if staging/UAT environment)**
  - Run: `DemoDataSeeder.SeedDemoDataAsync()`
  - Verified: 6 demo sponsors exist
  - Verified: 8 demo students exist
  - Verified: 5 demo LoGs exist
  - Verified: Demo users exist (demo.admin, demo.admissions, demo.cashier, demo.sponsor)
  - Verified by: __________ Date: __________

- [ ] **Database backup configured**
  - Backup schedule: Daily full, hourly incremental (or as configured)
  - Backup location: [Specify location]
  - Verified by: __________ Date: __________

---

## 2. Authentication and Security

### 2.1 Authentication Working

- [ ] ⚠️ **Admin login working**
  - Test: Log in as admin@ism.edu.ph (or demo.admin@ism.edu.ph)
  - Expected: Successful login, redirected to dashboard
  - Verified by: __________ Date: __________

- [ ] ⚠️ **Role-based access control (RBAC) enforced**
  - Test: Log in as demo.admissions@ism.edu.ph
  - Test: Attempt to access `/Settings/Users` (Admin-only page)
  - Expected: Access denied or 403 Forbidden
  - Verified by: __________ Date: __________

- [ ] **Account lockout working**
  - Test: Attempt 5 failed logins
  - Expected: Account locked, cannot log in until unlocked
  - Verified by: __________ Date: __________

- [ ] **Password complexity enforced**
  - Test: Attempt to set weak password (e.g., "password")
  - Expected: Validation error, password rejected
  - Verified by: __________ Date: __________

---

### 2.2 Security Controls

- [ ] ⚠️ **No security vulnerabilities in dependencies**
  - Check: Run `dotnet list package --vulnerable`
  - Expected: No known vulnerabilities
  - Verified by: __________ Date: __________

- [ ] **Sensitive configuration not exposed in client-side code**
  - Check: View page source, check for connection strings, API keys
  - Expected: No sensitive data visible
  - Verified by: __________ Date: __________

- [ ] **Error pages do not expose stack traces**
  - Test: Navigate to invalid URL (e.g., `/InvalidPage`)
  - Expected: Generic error page, no stack trace
  - Verified by: __________ Date: __________

---

## 3. Core Functionality

### 3.1 Sponsor Management

- [ ] ⚠️ **Sponsor CRUD working**
  - Test: Create new sponsor
  - Test: View sponsor detail
  - Test: Edit sponsor
  - Test: Deactivate sponsor
  - Expected: All operations succeed without errors
  - Verified by: __________ Date: __________

- [ ] ⚠️ **Sponsor validation working**
  - Test: Create sponsor with invalid email
  - Test: Create sponsor with missing required fields
  - Expected: Validation errors displayed
  - Verified by: __________ Date: __________

- [ ] **Sponsor search and filter working**
  - Test: Search by sponsor name
  - Test: Filter by Active/Inactive
  - Expected: Results filtered correctly
  - Verified by: __________ Date: __________

---

### 3.2 Letter of Guarantee (LoG) Workflow

- [ ] ⚠️ **LoG creation working**
  - Test: Create new LoG for demo sponsor
  - Test: Add coverage rules (Covered, Split, NotCovered)
  - Expected: LoG created successfully with rules
  - Verified by: __________ Date: __________

- [ ] ⚠️ **LoG activation working (Admin only)**
  - Test: Log in as Admin
  - Test: Activate LoG in "UnderReview" status
  - Expected: LoG status changes to "Active"
  - Verified by: __________ Date: __________

- [ ] **LoG list and filtering working**
  - Test: Filter LoGs by school year
  - Test: Filter LoGs by sponsor
  - Test: Filter LoGs by status
  - Expected: Results filtered correctly
  - Verified by: __________ Date: __________

---

### 3.3 Coverage Evaluation

- [ ] ⚠️ **Coverage evaluation API working**
  - Test: Call `/api/coverage/evaluate` with valid parameters
  - Expected: Response within 2 seconds with correct decision
  - Verified by: __________ Date: __________

- [ ] ⚠️ **Coverage evaluation handles all scenarios**
  - Test: Covered item (100% sponsor)
  - Test: Split item (X% sponsor, Y% parent)
  - Test: NotCovered item (100% parent)
  - Test: Item not in LoG (defaults to NotCovered)
  - Expected: Correct decision and amounts for each scenario
  - Verified by: __________ Date: __________

- [ ] **Coverage evaluation audit logged**
  - Test: Run evaluation
  - Test: Check `CoverageEvaluationAudit` table
  - Expected: Evaluation logged with all details
  - Verified by: __________ Date: __________

---

### 3.4 Change Request and Approval Workflow

- [ ] ⚠️ **Change request submission working**
  - Test: Log in as Admissions
  - Test: Submit change request for sponsor field
  - Expected: Request submitted, appears in pending list
  - Verified by: __________ Date: __________

- [ ] ⚠️ **Change request approval working**
  - Test: Log in as Admin
  - Test: Approve pending change request
  - Expected: Request approved, sponsor record updated automatically
  - Verified by: __________ Date: __________

- [ ] **Change request rejection working**
  - Test: Log in as Admin
  - Test: Reject pending change request with comments
  - Expected: Request rejected, requester can view comments
  - Verified by: __________ Date: __________

---

### 3.5 Duplicate Detection and Merge

- [ ] **Duplicate detection working**
  - Test: Run duplicate detection
  - Expected: Similar sponsors identified (e.g., DEMO-SP001 and DEMO-SP006)
  - Verified by: __________ Date: __________

- [ ] **Sponsor merge working**
  - Test: Merge DEMO-SP006 into DEMO-SP001
  - Expected: Students and LoGs reassigned, DEMO-SP006 marked as merged
  - Verified by: __________ Date: __________

---

## 4. Reporting and Monitoring

### 4.1 Reports

- [ ] **Admin reports working**
  - Test: Generate Sponsor Master Report
  - Test: Generate LoG Activity Report
  - Test: Generate Coverage Decisions Report
  - Test: Generate Sync Status Report
  - Test: Generate Audit Activity Report
  - Expected: All reports generate without errors
  - Verified by: __________ Date: __________

- [ ] **Report filtering working**
  - Test: Filter reports by date range
  - Test: Filter reports by sponsor, school year, status
  - Expected: Results filtered correctly
  - Verified by: __________ Date: __________

- [ ] **CSV export working**
  - Test: Export report to CSV
  - Expected: CSV file downloads with correct data
  - Verified by: __________ Date: __________

---

### 4.2 Operational Monitoring

- [ ] **Operations dashboard accessible (Admin only)**
  - Test: Navigate to `/Operations/Dashboard`
  - Expected: Dashboard loads with system health indicators
  - Verified by: __________ Date: __________

- [ ] **System health indicators working**
  - Test: View health status (Database, Sync, Audit)
  - Expected: Status shown as Healthy/Degraded/Unhealthy
  - Verified by: __________ Date: __________

- [ ] **Smoke test checklist accessible**
  - Test: Navigate to `/Operations/SmokeTest`
  - Expected: Checklist loads with 24 test items
  - Verified by: __________ Date: __________

- [ ] **Operational runbooks accessible**
  - Test: Navigate to `/Operations/Runbooks`
  - Expected: 5 runbooks displayed
  - Verified by: __________ Date: __________

---

### 4.3 Audit Logging

- [ ] ⚠️ **Audit logging working**
  - Test: Create sponsor, edit sponsor, submit change request
  - Test: Navigate to audit logs
  - Expected: All actions logged with user, timestamp, details
  - Verified by: __________ Date: __________

- [ ] **Audit log filtering working**
  - Test: Filter by date range
  - Test: Filter by module (Sponsor, LoG, ChangeRequest)
  - Expected: Results filtered correctly
  - Verified by: __________ Date: __________

---

## 5. User Acceptance Testing (UAT) Preparation

### 5.1 UAT Documentation

- [ ] ⚠️ **UAT Guide available**
  - Location: `docs/UAT_GUIDE.md`
  - Content: 10 test scenarios (UT01-UT10) across 4 roles
  - Verified by: __________ Date: __________

- [ ] **Defect template available**
  - Location: `docs/DEFECT_TEMPLATE.md`
  - Content: Complete defect report template
  - Verified by: __________ Date: __________

- [ ] **Test execution template ready**
  - Format: Spreadsheet or markdown with pass/fail checkboxes
  - Content: All 10 scenarios listed
  - Verified by: __________ Date: __________

---

### 5.2 UAT Accounts and Data

- [ ] ⚠️ **UAT user accounts created and tested**
  - demo.admin@ism.edu.ph (Admin) - Login works: ☐ Yes
  - demo.admissions@ism.edu.ph (Admissions) - Login works: ☐ Yes
  - demo.cashier@ism.edu.ph (Cashier) - Login works: ☐ Yes
  - demo.sponsor@ism.edu.ph (Sponsor) - Login works: ☐ Yes
  - Verified by: __________ Date: __________

- [ ] **Demo data seeded and verified**
  - 6 demo sponsors: ☐ Yes
  - 8 demo students: ☐ Yes
  - 5 demo LoGs: ☐ Yes
  - 4 demo change requests: ☐ Yes
  - Demo sync logs and audit logs: ☐ Yes
  - Verified by: __________ Date: __________

- [ ] **UAT environment accessible to testers**
  - URL provided to testers: [Insert URL]
  - Testers can access environment: ☐ Yes
  - Verified by: __________ Date: __________

---

## 6. Training and Documentation

### 6.1 Training Materials

- [ ] **Training Guide available**
  - Location: `docs/TRAINING_GUIDE.md`
  - Content: Quick-start by role, common workflows, mistake recovery
  - Verified by: __________ Date: __________

- [ ] **Demo Walkthrough Guide available**
  - Location: `docs/DEMO_GUIDE.md`
  - Content: Presenter scripts for 10 demo flows
  - Verified by: __________ Date: __________

- [ ] **Handoff Documentation available**
  - Location: `docs/HANDOFF.md`
  - Content: Solution overview, architecture, module inventory, operations
  - Verified by: __________ Date: __________

---

### 6.2 Operationalreadiness

- [ ] **Operational runbooks reviewed by IT staff**
  - Runbooks: Sync Failure, Data Mismatch, Access Issues, Coverage Evaluation, Deployment Health
  - IT staff understands procedures: ☐ Yes
  - Verified by: __________ Date: __________

- [ ] **Smoke test checklist reviewed and understood**
  - 24 test items across 6 categories
  - IT staff can execute smoke tests: ☐ Yes
  - Verified by: __________ Date: __________

- [ ] **Feedback system tested**
  - Test: Submit feedback as each role
  - Test: Admin can view and update feedback status
  - Expected: Feedback system working
  - Verified by: __________ Date: __________

---

## 7. Integration and Sync

### 7.1 External System Integration

- [ ] **PowerSchool sync adapter tested (stub/manual)**
  - Test: View sync logs
  - Expected: Sync logs displayed (even if stub data)
  - Verified by: __________ Date: __________

- [ ] **NetSuite sync adapter tested (stub/manual)**
  - Test: View sync logs
  - Expected: Sync logs displayed (even if stub data)
  - Verified by: __________ Date: __________

- [ ] **Sync failure logging working**
  - Test: View sync failures on operations dashboard
  - Expected: Failures displayed with error messages
  - Verified by: __________ Date: __________

---

## 8. Deployment and Rollback

### 8.1 Deployment Readiness

- [ ] **Deployment runbook reviewed**
  - Location: `docs/step8-implementation-summary.md` (Deployment section)
  - Content: Pre-deployment, deployment, post-deployment steps
  - Verified by: __________ Date: __________

- [ ] **Rollback plan documented and understood**
  - Rollback procedure: Stop app, restore code, restore database, restart app
  - IT staff can execute rollback: ☐ Yes
  - Verified by: __________  Date: __________

- [ ] **Smoke test executed post-deployment**
  - Execute all 24 smoke test items
  - Pass rate: _____% (target: 100%)
  - Verified by: __________ Date: __________

---

## 9. Stakeholder Sign-Off

### 9.1 Business Sign-Off

- [ ] **Product Owner reviewed and approved**
  - Name: __________________________
  - Signature: _______________________ Date: __________

- [ ] **Admissions Director reviewed and approved**
  - Name: __________________________
  - Signature: _______________________ Date: __________

- [ ] **Finance Director reviewed and approved**
  - Name: __________________________
  - Signature: _______________________ Date: __________

---

### 9.2 Technical Sign-Off

- [ ] **Technical Lead reviewed and approved**
  - Name: __________________________
  - Signature: _______________________ Date: __________

- [ ] **IT Manager reviewed and approved**
  - Name: __________________________
  - Signature: _______________________ Date: __________

- [ ] **Security review completed (if applicable)**
  - Reviewer: __________________________
  - Findings: ☐ No blocking issues ☐ Issues documented and mitigated
  - Signature: _______________________ Date: __________

---

## 10. Final Verification

### 10.1 Pre-Launch Checklist

- [ ] **All critical checklist items completed (marked with ⚠️)**
  - Count of critical items: _____
  - Count completed: _____
  - Completion rate: _____% (target: 100%)

- [ ] **All known defects documented**
  - Critical defects: _____ (target: 0)
  - High defects: _____ (target: ≤2)
  - Medium/Low defects: _____ (accepted with workarounds)

- [ ] **UAT scheduled and participants confirmed**
  - UAT start date: __________
  - UAT participants: ☐ Admin ☐ Admissions ☐ Cashier ☐ Sponsor
  - UAT lead: __________________________

- [ ] **Pilot go/no-go decision made**
  - Decision: ☐ GO (proceed with pilot) ☐ NO-GO (address blockers first)
  - Decision maker: __________________________
  - Decision date: __________

---

## Final Approval for Pilot Deployment

**I certify that all critical items in this checklist have been completed and the ISM Sponsor Management System is ready for pilot deployment.**

**Approved by:** ___________________________  
**Title:** ___________________________  
**Signature:** ___________________________  
**Date:** ___________________________

---

## Notes and Comments

[Add any additional notes, conditions, or comments here]

---

**End of Pilot Readiness Checklist**
