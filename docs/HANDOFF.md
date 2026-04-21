# Handoff Documentation
## ISM Sponsor Management System

**Version:** 1.0  
**Date:** March 2026  
**Status:** Ready for Pilot Deployment

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Solution Overview](#solution-overview)
3. [Architecture Summary](#architecture-summary)
4. [Module Inventory](#module-inventory)
5. [Role and Permission Summary](#role-and-permission-summary)
6. [Environment and Configuration](#environment-and-configuration)
7. [Demo and Seed Data](#demo-and-seed-data)
8. [Deployment Summary](#deployment-summary)
9. [Monitoring and Operations](#monitoring-and-operations)
10. [Known Limitations](#known-limitations)
11. [Future Enhancement Backlog](#future-enhancement-backlog)
12. [Support and Ownership](#support-and-ownership)

---

## Executive Summary

### Purpose

The ISM Sponsor Management System centralizes sponsor data, Letter of Guarantee (LoG) coverage rules, charge-entry evaluation, and approval workflows for International School Manila. The system replaces scattered spreadsheets and manual processes with a centralized, auditable, role-based web application.

### Business Value

- **Centralized sponsor data:** Single source of truth for sponsor information
- **Automated coverage evaluation:** Real-time API determines billing responsibility
- **Reduced errors:** Validation and approval workflows prevent incorrect data
- **Audit compliance:** Complete audit trail for all changes
- **Time savings:** Eliminates manual lookups and spreadsheet maintenance
- **Integration-ready:** API-first design for PowerSchool, NetSuite, and OBS integration

### Pilot Readiness

- **Core workflows implemented:** Sponsor CRUD, LoG management, approval workflows, reporting
- **Security controls:** Role-based access, authentication, audit logging
- **Demo data seeded:** Realistic test scenarios with 6 sponsors, 8 students, 5 LoGs
- **UAT scripts prepared:** 10 scenarios across 4 roles (Admin, Admissions, Cashier, Sponsor)
- **Training materials:** Quick-start guides by role
- **Operational support:** Runbooks, smoke tests, feedback system

---

## Solution Overview

### Functional Scope

#### Implemented (Pilot-Ready)

**Sponsor Management:**
- Create, read, update, deactivate sponsor records
- Validation of sponsor data (email, phone, address)
- Duplicate detection and controlled merge
- Audit trail for all sponsor changes

**Letter of Guarantee (LoG) Management:**
- Create, edit, activate, deactivate LoGs
- Coverage rule definition (Covered, Split, NotCovered)
- School year-based LoG management
- Admin-only activation controls

**Coverage Evaluation API:**
- Real-time evaluation of coverage for charge entries
- Determines sponsor vs. parent payment responsibility
- Returns split amounts for billing systems
- Audit log of all evaluations

**Approval Workflows:**
- Change request submission (Admissions)
- Change request review and approval/rejection (Admin)
- Automatic application of approved changes
- Audit trail of request lifecycle

**Reporting:**
- Admin oversight reports (Sponsor Master, LoG Activity, Coverage Decisions, Sync Status, Audit Activity)
- Admissions tracking reports (pending work, recent additions)
- Cashier reconciliation reports (recent coverage decisions)
- CSV export for all reports

**Operational Support:**
- System health dashboard
- Smoke test checklist (24 tests)
- Operational runbooks (5 runbooks)
- Release checklist (pre/during/post deployment)
- User feedback system

**Security:**
- ASP.NET Core Identity authentication
- Role-based authorization (Admin, Admissions, Cashier, Sponsor)
- Password complexity requirements
- Account lockout after failed attempts
- Audit logging of security events

#### Not Implemented (Future Phases)

- Sponsor self-service portal (Sponsor role = read-only)
- Automated email notifications for change requests
- Scheduled report generation/distribution
- Advanced analytics and forecasting
- Document attachment management (LoG PDFs, contracts)
- Multi-sponsor support for students
- Automated sync with PowerSchool/NetSuite (currently stub/manual)
- Full production hardening (DR, HA, advanced monitoring)

---

## Architecture Summary

### Technology Stack

- **Framework:** ASP.NET Core 8.0 MVC + Web API
- **Language:** C# 12
- **Database:** SQL Server 2019+
- **ORM:** Entity Framework Core 8.0
- **Authentication:** ASP.NET Core Identity
- **Frontend:** Razor Pages, Bootstrap 5, jQuery
- **Hosting:** Azure App Service or IIS (Windows Server)

### Architectural Patterns

- **MVC (Model-View-Controller):** UI layer
- **Repository Pattern:** Data access abstraction (via EF Core DbContext)
- **Service Layer:** Business logic encapsulation
- **API-First Design:** Coverage evaluation exposed as API endpoint
- **Role-Based Access Control (RBAC):** Authorization via Identity roles
- **Audit Logging:** Cross-cutting concern for compliance

### Solution Structure

```
ISMSponsor/
├── Controllers/             # MVC Controllers + API Controllers
│   ├── AccountController.cs         # Authentication
│   ├── DashboardController.cs       # Role-specific dashboards
│   ├── SponsorsController.cs        # Sponsor CRUD
│   ├── PortalController.cs          # LoG management
│   ├── ReportsController.cs         # Reporting
│   ├── OperationsController.cs      # Operational monitoring
│   ├── FeedbackController.cs        # User feedback
│   └── Settings/                    # Admin settings controllers
├── Models/
│   ├── Domain/              # Entity models (Sponsor, LogCoverage, etc.)
│   ├── ApplicationUser.cs   # Identity user extension
│   └── [ViewModels]         # Request/response DTOs
├── ViewModels/              # View-specific DTOs
├── Services/                # Business logic layer
│   ├── SponsorService.cs            # Sponsor operations
│   ├── ChangeRequestService.cs      # Approval workflow
│   ├── ReportService.cs             # Report generation
│   ├── MonitoringService.cs         # System health
│   ├── SmokeTestService.cs          # Post-deployment verification
│   ├── FeedbackService.cs           # User feedback
│   └── (others)
├── Data/
│   ├── AppDbContext.cs      # EF Core DbContext
│   ├── DbInitializer.cs     # Seed initial data (roles, admin)
│   └── DemoDataSeeder.cs    # Seed demo data for UAT/pilot
├── Views/                   # Razor views
├── wwwroot/                 # Static files (CSS, JS, images)
├── Migrations/              # EF Core migrations
├── appsettings.json         # Configuration
└── Program.cs               # Application startup
```

### Database Schema Overview

**Key Entities:**

- **Sponsors:** Core sponsor entity (ID, name, contact, address, sync status, merge status)
- **Students:** Student entity linked to sponsors
- **LogCoverages:** Letter of Guarantee headers (sponsor, school year, status)
- **CoverageRules:** Individual coverage rules within LoGs (item, coverage type, percentages)
- **ChangeRequests:** Change request workflow (field, old/new value, status, reviewer)
- **SchoolYears:** School year definitions (start/end dates, active flag)
- **Items:** Fee code definitions (tuition, bus, lunch, etc.)
- **ActivityLogs:** Audit logs for user actions
- **CoverageEvaluationAudit:** Audit logs for coverage evaluations
- **SyncLogs:** Integration sync attempt logs
- **UserFeedback:** User feedback submissions
- **AspNetUsers, AspNetRoles, AspNetUserRoles:** Identity framework tables

**Key Relationships:**

- Sponsor 1:Many Students
- Sponsor 1:Many LogCoverages
- LogCoverage 1:Many CoverageRules
- Sponsor 1:Many ChangeRequests
- LogCoverage Many:1 SchoolYear
- CoverageRule Many:1 Item

### Integration Points

**Coverage Evaluation API:**
- **Endpoint:** `POST /api/coverage/evaluate`
- **Request:** `{ sponsorId, studentId, itemCode, amount }`
- **Response:** `{ decision, sponsorAmount, parentAmount, billTo, ruleApplied, success, errorMessage }`
- **Consumers:** PowerSchool, OBS, NetSuite (future)

**External System Sync (Stub/Manual):**
- **PowerSchool:** Sponsor and student data sync (stub implementation)
- **NetSuite:** Sponsor invoicing sync (stub implementation)
- **Sync Logs:** Track attempts, successes, failures

---

## Module Inventory

### Step 1: Sponsor Master CRUD Foundations
- Sponsor creation, editing, viewing
- Validation (email, phone, required fields)
- Sponsor list with search/filter
- Basic audit logging

### Step 2: Letters of Guarantee Module
- LoG creation, editing, viewing
- Coverage rule management (Create, Edit, Delete)
- LoG activation/deactivation controls (Admin only)
- School year-based LoG filtering

### Step 3: Coverage Evaluation API
- Real-time coverage evaluation
- Decision engine (Covered, Split, NotCovered)
- Split amount calculation
- Coverage evaluation audit logging
- API endpoint for external integration

### Step 4: Sponsor Request and Approval Workflow
- Change request submission (Admissions)
- Change request review (Admin)
- Approval/rejection workflow
- Automatic application of approved changes
- Audit trail of workflow

### Step 5: Dashboard, Settings Module, and Audit Retrieval
- Role-specific dashboards (Admin, Admissions, Cashier, Sponsor)
- Settings pages (Users, Roles, School Years, Students, Items)
- Audit log retrieval and filtering
- Dashboard widgets (pending requests, system health)

### Step 6: Duplicate Detection, Controlled Merge, and Sync Adapters
- Duplicate sponsor detection (similar names, addresses)
- Controlled merge workflow (select primary, preview, confirm)
- Merge preserves all related records (students, LoGs, requests)
- Sync adapter stubs (PowerSchool, NetSuite)
- Sync log tracking

### Step 7: Authentication Hardening, Security Controls, and CI/CD
- Account lockout after failed login attempts
- Password complexity requirements
- Role-based authorization enforcement
- Security audit logging
- Deployment-ready CI/CD foundations (GitHub Actions, Azure Pipelines ready)

### Step 8: Reports, Operational Runbooks, and Pilot-Readiness Polish
- Admin oversight reports (5 reports)
- Admissions tracking reports
- Cashier reconciliation reports
- CSV export for all reports
- Operational monitoring dashboard
- Smoke test checklist (24 tests)
- Operational runbooks (5 runbooks)
- Release checklist
- User feedback system

### Step 9: UAT Pack, Demo Data, Training, and Handoff
- UAT scripts (10 scenarios across 4 roles)
- Demo data seeder (6 sponsors, 8 students, 5 LoGs, change requests, sync logs, audit logs)
- Demo walkthrough guide for presenters
- Training guide for end users
- Handoff documentation (this document)
- Known limitations and future work backlog
- Pilot readiness checklist

---

## Role and Permission Summary

### Roles Defined

1. **Admin**
2. **Admissions**
3. **Cashier**
4. **Sponsor**

### Permission Matrix

| Feature | Admin | Admissions | Cashier | Sponsor |
|---------|-------|------------|---------|---------|
| **Sponsor Management** |
| View sponsors | ✅ Full | ✅ Full | ✅ Read-only | ❌ (Future: Own profile) |
| Create sponsor | ✅ Yes | ✅ Yes | ❌ No | ❌ No |
| Edit sponsor (direct) | ✅ Yes | ⚠️ Limited (some fields require approval) | ❌ No | ❌ No |
| Deactivate sponsor | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Merge sponsors | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Detect duplicates | ✅ Yes | ❌ No | ❌ No | ❌No |
| **LoG Management** | 
| View LoGs | ✅ Full | ✅ Full | ✅ Read-only | ❌ (Future: Own sponsor's LoGs) |
| Create LoG | ✅ Yes | ✅ Yes | ❌ No | ❌ No |
| Edit LoG | ✅ Yes | ⚠️ Before activation | ❌ No | ❌ No |
| Activate LoG | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Deactivate LoG | ✅ Yes | ❌ No | ❌ No | ❌ No |
| **Approval Workflows** |
| Submit change request | ✅ Yes (but can edit directly) | ✅ Yes | ❌ No | ❌ No |
| Review change request | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Approve/reject request | ✅ Yes | ❌ No | ❌ No | ❌ No |
| **Coverage Evaluation** |
| Evaluate coverage | ✅ Yes | ✅ Yes | ✅ Yes | ❌ (Future) |
| View evaluation audit | ✅ Full | ⚠️ Own evaluations | ⚠️ Own evaluations | ❌ No |
| **Reports** |
| Admin reports | ✅ All 5 reports | ❌ No | ❌ No | ❌ No |
| Admissions reports | ✅ Yes | ✅ Yes | ❌ No | ❌ No |
| Cashier reports | ✅ Yes | ❌ No | ✅ Yes | ❌ No |
| Export to CSV | ✅ Yes | ✅ Yes (own reports) | ✅ Yes (own reports) | ❌ No |
| **Settings** |
| Manage users | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Manage roles | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Manage school years | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Manage items (fee codes) | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Manage students | ✅ Yes | ⚠️ Limited | ❌ No | ❌ No |
| **Audit and Monitoring** |
| View audit logs | ✅ Full | ⚠️ Own actions | ⚠️ Own actions | ❌ No |
| View operations dashboard | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Run smoke tests | ✅ Yes | ❌ No | ❌ No | ❌ No |
| View runbooks | ✅ Yes | ❌ No | ❌ No | ❌ No |
| **Feedback** |
| Submit feedback | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| View all feedback | ✅ Yes | ❌ No | ❌ No | ❌ No |
| Update feedback status | ✅ Yes | ❌ No | ❌ No | ❌ No |

### Default Admin Account

**Email:** admin@ism.edu.ph  
**Password:** (Set during initial deployment)  
**Role:** Admin  
**Status:** Active

**Important:** Change default admin password immediately after first login.

---

## Environment and Configuration

### Deployment Environments

| Environment | Purpose | URL | Database |
|-------------|---------|-----|----------|
| **Development** | Local development and testing | http://localhost:5000 | LocalDB or SQL Server Express |
| **Staging** | UAT, demo, training | [Insert staging URL] | Staging SQL Server |
| **Production** | Live pilot deployment | [Insert production URL] | Production SQL Server |

### Configuration Files

**appsettings.json (Base):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ISMSponsor;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**appsettings.Development.json:**
- Development-specific settings
- Detailed logging
- LocalDB connection string

**appsettings.Production.json:**
- Production SQL Server connection string (Azure SQL or on-prem)
- HTTPS enforcement
- Reduced logging verbosity

### Environment Variables (Production)

**Required:**
- `ConnectionStrings__DefaultConnection`: SQL Server connection string (use Azure Key Vault or app service configuration)

**Optional:**
- `ASPNETCORE_ENVIRONMENT`: Production
- `ASPNETCORE_URLS`: https://+:443;http://+:80

### Database Migration

**To apply migrations:**
```bash
dotnet ef database update
```

**To create new migration:**
```bash
dotnet ef migrations add MigrationName
```

**Migration history in repository:** All migrations tracked in `/Migrations` folder

---

## Demo and Seed Data

### Demo Data Seeder

**Purpose:** Seed realistic demo data for UAT, training, and demos

**Location:** `Data/DemoDataSeeder.cs`

**How to Run:**
```csharp
// In Program.cs or during startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    await seeder.SeedDemoDataAsync();
}
```

### Demo Data Included

**Users (4):**
- demo.admin@ism.edu.ph (Admin)
- demo.admissions@ism.edu.ph (Admissions)
- demo.cashier@ism.edu.ph (Cashier)
- demo.sponsor@ism.edu.ph (Sponsor)

Password for all: `Demo@2026!`

**Sponsors (6):**
- DEMO-SP001: Global Tech Corporation (active, synced, 2 students, 1 active LoG)
- DEMO-SP002: Asian Development Bank (active, partial sync, 2 students, 1 active LoG)
- DEMO-SP003: Embassy of Canada (active, partial sync, 1 student, 1 active LoG)
- DEMO-SP004: Manila Consulting Group (active, no sync, 2 students, 1 pending LoG)
- DEMO-SP005: Pacific Resources Ltd (inactive, deactivated, 1 inactive student)
- DEMO-SP006: Global Tech Corp (duplicate candidate for merge testing)

**Students (8):** Across various grade levels and sponsors

**LoGs (5):**
- DEMO-LOG001: Active, Global Tech Corp, full coverage rules (Covered, Split)
- DEMO-LOG002: Active, ADB, full coverage rules
- DEMO-LOG003: Active, Embassy of Canada, selective coverage (includes NotCovered rules)
- DEMO-LOG004: UnderReview, Manila Consulting Group (pending activation)
- DEMO-LOG005: Inactive, ADB, previous school year

**Change Requests (4):**
- 1 pending request (demo.admissions)
- 1 approved and applied request
- 1 rejected request
- 1 pending recent request

**Sync Logs (9):** Mix of successful and failed sync attempts

**Audit Logs (9):** Sample audit entries for sponsor creation, LoG activation, request approval

**Coverage Evaluation Audits (8):** Sample evaluations showing Covered, Split, NotCovered decisions

### Production Data Initialization

**Initial Seed (DbInitializer.cs):**
- Admin account
- Roles (Admin, Admissions, Cashier, Sponsor)
- Initial school year
- Initial fee code items

**Production Migration:**
- Import sponsor data via CSV or database script
- Students imported from PowerSchool
- LoGs created manually by Admissions staff during pilot
- Change requests created organically during pilot

---

## Deployment Summary

### Deployment Steps

**1. Pre-Deployment**
- [x] Backup existing database (if applicable)
- [x] Review release notes and feature list
- [x] Confirm rollback plan
- [x] Verify environment credentials (SQL Server, Azure)

**2. Deployment**
- [ ] Stop application (if running)
- [ ] Deploy code to app service or IIS
  - Publish: `dotnet publish -c Release -o ./publish`
  - Copy to server or deploy via Azure DevOps
- [ ] Update `appsettings.Production.json` with production connection string
- [ ] Apply database migrations: `dotnet ef database update`
- [ ] Seed demo data (if UAT/staging): Run `DemoDataSeeder.SeedDemoDataAsync()`
- [ ] Start application

**3. Post-Deployment**
- [ ] Verify application starts successfully (check logs)
- [ ] Run smoke test checklist (`/Operations/SmokeTest`)
- [ ] Test authentication (Admin, Admissions, Cashier logins)
- [ ] Test critical path: Create sponsor, create LoG, evaluate coverage
- [ ] Check system health dashboard (`/Operations/Dashboard`)
- [ ] Monitor for errors (check logs, operations dashboard)

**4. Rollback (If Needed)**
- [ ] Stop application
- [ ] Restore previous code version
- [ ] Restore database backup (if schema changed)
- [ ] Restart application
- [ ] Verify rollback success

### Deployment Artifacts

**Published Output:** `./publish` folder (contains compiled application)

**Database Scripts:** Migrations in `/Migrations` folder

**Configuration:** `appsettings.Production.json` (store connection string in Azure Key Vault or secure config)

---

## Monitoring and Operations

### Health Monitoring

**Operations Dashboard:** `/Operations/Dashboard`

**Metrics Monitored:**
- Application health status (Healthy, Degraded, Unhealthy)
- Database connectivity
- Configuration validity
- Sync system health
- Audit system health

**Sync Monitoring:**
- Recent sync attempts (success/failure)
- Sync failures with error messages
- Retry counts

**Data Consistency Warnings:**
- Orphaned sponsors (students without sponsor)
- Missing LoGs (sponsors without active LoGs)
- Stale change requests (pending >30 days)

### Smoke Test Checklist

**Post-Deployment Verification:** `/Operations/SmokeTest`

**24 Test Items:**
- Authentication (3 tests)
- Sponsor management (4 tests)
- LoG management (4 tests)
- Coverage evaluation (4 tests)
- Sync visibility (4 tests)
- Audit logging (5 tests)

**Run after each deployment to verify all features work correctly.**

### Operational Runbooks

**5 Runbooks Available:** `/Operations/Runbooks`

1. **Sync Failure Response:** Target system failures, authentication issues, data validation errors
2. **Data Mismatch Investigation:** Duplicates, missing LoGs, inactive sponsors
3. **Access Issue Resolution:** Role assignments, account activation, authentication
4. **Coverage Evaluation Issues:** Missing LoGs, inactive rules, school year mismatch
5. **Deployment Health Check:** Post-deployment verification steps

### Logging

**Application Logs:**
- Location: Azure App Service logs or IIS logs
- Level: Information (Production), Debug (Development)
- Retention: 30 days (configurable)

**Audit Logs:**
- Location: `ActivityLogs` table in database
- Retention: Indefinite
- Exportable via Reports

**Coverage Evaluation Audit:**
- Location: `CoverageEvaluationAudit` table
- Retention: Indefinite
- Used for reconciliation and reporting

### Performance Monitoring

**Key Metrics:**
- Page load time (target: <2 seconds)
- API response time (coverage evaluation target: <1 second)
- Database query time (target: <500ms for most queries)

**Monitoring Tools:**
- Azure Application Insights (if hosted on Azure)
- IIS logs and performance counters (if hosted on Windows Server)

---

## Known Limitations

See [LIMITATIONS_AND_FUTURE_WORK.md](LIMITATIONS_AND_FUTURE_WORK.md) for comprehensive list.

**Key Limitations:**

1. **Prototype scope only:** Not full production hardening
2. **Sponsor self-service not implemented:** Sponsor role is read-only placeholder
3. **Manual sync:** PowerSchool and NetSuite sync are stub implementations; manual sync required
4. **No automated notifications:** Change request approvals don't trigger emails
5. **No document attachments:** LoG PDFs and contracts not managed in system
6. **Single sponsor per student:** Multi-sponsor support not implemented
7. **Limited reporting customization:** Reports are fixed; no custom report builder
8. **No advanced analytics:** No predictive analytics or financial forecasting
9. **External system UIs not redesigned:** PowerSchool, NetSuite, OBS UIs remain unchanged
10. **Advanced monitoring/DR/HA not implemented:** Basic monitoring only; full production resiliency pending

---

## Future Enhancement Backlog

See [LIMITATIONS_AND_FUTURE_WORK.md](LIMITATIONS_AND_FUTURE_WORK.md) for full backlog.

**Priority Future Work:**

1. **Automated PowerSchool/NetSuite sync:** Replace stub with real integration
2. **Email notifications:** Notify users of change request decisions, LoG activations
3. **Sponsor self-service portal:** Allow sponsors to view own profile, students, LoGs
4. **Document attachment management:** Upload and manage LoG PDFs, contracts, invoices
5. **Multi-sponsor support:** Support students with multiple sponsors and split rules
6. **Custom report builder:** Allow users to create ad-hoc reports
7. **Advanced monitoring and alerting:** Proactive alerts for sync failures, data issues
8. **Performance tuning and caching:** Optimize for larger data volumes
9. **Mobile-friendly UI:** Responsive design improvements
10. **Pilot feedback-driven UX refinements:** Based on UAT and pilot feedback

---

## Support and Ownership

### Business Ownership

**Product Owner:** [Name, Title, Email]  
**Business Stakeholders:**
- Admissions Director: [Name, Email]
- Finance Director: [Name, Email]
- IT Manager: [Name, Email]

### Technical Ownership

**Development Team:** [Vendor/Internal team name]  
**Technical Lead:** [Name, Email]  
**Database Administrator:** [Name, Email]  
**Infrastructure/DevOps:** [Name, Email]

### Support Model (Post-Pilot)

**Tier 1: End Users**
- Self-service: Training guides, in-app feedback
- Contact: Admissions or Cashier staff (for operational questions)

**Tier 2: Power Users / Super Users**
- Admin users trained on system
- Contact: IT support or designated super user

**Tier 3: Technical Support**
- Development team or vendor
- Contact: [Support email or ticket system]
- Escalation path: Critical issues to technical lead

### SLA and Response Times (TBD Post-Pilot)

- **Critical (system down, data loss):** 1-hour response, 4-hour resolution target
- **High (major feature broken):** 4-hour response, 1-business-day resolution target
- **Medium (workaround available):** 1-business-day response, 3-business-day resolution target
- **Low (cosmetic, enhancement):** Best effort, scheduled for future release

---

## Handoff Checklist

### Business Handoff

- [x] Business requirements documented
- [x] UAT scripts provided
- [x] Training materials provided
- [x] Demo walkthrough guide provided
- [x] Known limitations documented
- [x] Success metrics defined

### Technical Handoff

- [x] Source code repository provided
- [x] Architecture documentation provided
- [x] Database schema documented (via EF Core models and migrations)
- [x] API documentation provided (coverage evaluation API)
- [x] Deployment guide provided
- [x] Configuration guide provided
- [x] Runbooks provided

### Pilot Operations Handoff

- [x] Demo data seeded
- [x] Smoke test checklist provided
- [x] Operations dashboard available
- [x] Feedback system available
- [x] Defect logging template provided
- [x] Pilot readiness checklist provided

---

## Appendices

### Appendix A: Glossary

- **LoG:** Letter of Guarantee - Contract defining sponsor coverage
- **Coverage Type:** Covered (100% sponsor), Split (X% sponsor + Y% parent), NotCovered (100% parent)
- **School Year:** Academic year (e.g., 2025-2026)
- **Sponsor:** Organization providing financial support for student tuition/fees
- **Change Request:** Workflow for requesting changes to sensitive sponsor fields
- **Sync:** Integration with external systems (PowerSchool, NetSuite)
- **Smoke Test:** Post-deployment verification checklist
- **Runbook:** Operational procedure for common scenarios

### Appendix B: Contact List

| Role | Name | Email | Phone |
|------|------|-------|-------|
| Product Owner | [Name] | [Email] | [Phone] |
| Technical Lead | [Name] | [Email] | [Phone] |
| IT Manager | [Name] | [Email] | [Phone] |
| Admissions Director | [Name] | [Email] | [Phone] |
| Finance Director | [Name] | [Email] | [Phone] |
| Test Lead | [Name] | [Email] | [Phone] |

### Appendix C: Useful Links

- [GitHub Repository](link)
- [Azure DevOps Project](link)
- [SQL Server Database](connection details)
- [Staging Environment](URL)
- [Production Environment](URL)
- [Training Videos](link, if available)

---

**End of Handoff Documentation**

**Sign-Off:**

**Business Owner:** ___________________________  Date: __________

**Technical Lead:** ___________________________  Date: __________

**IT Manager:** ___________________________  Date: __________

---

**Document Version Control:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | March 2026 | [Your Name] | Initial handoff documentation |
