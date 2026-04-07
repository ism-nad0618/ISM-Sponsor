# ISM Sponsor Portal

ASP.NET Core 8.0 web application for managing sponsor relationships and Letters of Guarantee (LoGs).

## Project Status

**Current Phase**: Pilot Deployment Ready  
**Completion**: Steps 1-9 fully implemented  
**Documentation**: UAT, training, demo, handoff, and operational docs complete

## Setup

### Prerequisites
- .NET 8.0 SDK
- SQL Server (or SQL Server Express)

### Configuration

1. Copy `appsettings.json.example` to `appsettings.json`:
   ```bash
   cp appsettings.json.example appsettings.json
   ```

2. Update `appsettings.json` with your database connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost,1433;Database=ISMSponsor;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true"
     }
   }
   ```

### Running the Application

1. Build the project:
   ```bash
   dotnet build
   ```

2. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Open browser to `https://localhost:5001` or `http://localhost:5000`

### Demo Data (Step 9)

Demo data is automatically seeded on startup in Development or Staging environments. This includes:

- **6 Demo Sponsors**: DEMO-SP001 to DEMO-SP006 (Global Tech Corporation, Asian Development Bank, Embassy of Canada, etc.)
- **8 Demo Students**: DEMO-ST001 to DEMO-ST008 (across grades 3-11)
- **5 Demo LoGs**: DEMO-LOG001 to DEMO-LOG005 (Active, Under Review, Inactive statuses)
- **4 Demo Change Requests**: Pending, Approved, Rejected scenarios
- **4 Demo User Accounts**: Admin, Admissions, Cashier, Sponsor roles

See [docs/SAMPLE_TEST_DATA.md](docs/SAMPLE_TEST_DATA.md) for complete demo data reference.

### User Accounts

#### Production/Default Users (Seeded by DbInitializer)

- **Admin**: username: `admin`, password: `admin`
- **Admissions**: username: `admissions`, password: `admissions`
- **Cashier**: username: `cashier`, password: `cashier`
- **Sponsor (ACME)**: username: `acme_sponsor`, password: `sponsor`
- **Sponsor (XYZ)**: username: `xyz_sponsor`, password: `sponsor`

⚠️ **Change these passwords in production!**

#### Demo/UAT Users (Seeded by DemoDataSeeder - Step 9)

All demo users have password: `Demo@2026!`

- **Demo Admin**: `demo.admin@ism.edu.ph`
- **Demo Admissions**: `demo.admissions@ism.edu.ph`
- **Demo Cashier**: `demo.cashier@ism.edu.ph`
- **Demo Sponsor**: `demo.sponsor@ism.edu.ph`

Use these accounts for UAT testing, training, and demos.

## Features

### Core Functionality (Steps 1-6)
- **Sponsor Management**: Create, edit, search, and archive sponsor profiles
- **Student Management**: Manage student records and sponsor relationships
- **Letters of Guarantee (LoGs)**: Track LoGs with coverage rules and activation workflows
- **Coverage Evaluation**: Determine fee coverage (Covered, Split, Not Covered) based on LoG rules
- **Change Request Workflow**: Admissions submits requests, Admin approves/rejects
- **Duplicate Detection and Merge**: Identify and consolidate duplicate sponsor records
- **School Year Management**: Define and switch between school years
- **Item Management**: Manage fee codes (tuition, books, activities, etc.)
- **Role-Based Access Control**: Admin, Admissions, Cashier, Sponsor roles with different permissions

### Security and Operations (Steps 7-8)
- **Authentication**: ASP.NET Core Identity with password policies and lockout
- **Authorization**: Role-based permissions with fine-grained access control
- **Audit Logging**: Track all create/update/delete actions with user attribution
- **Security Hardening**: HTTPS, anti-forgery tokens, secure cookies, input validation
- **Health Checks**: Database, configuration, sync, audit endpoints
- **Monitoring Dashboard**: System health metrics, recent activity, alerts (Admin only)
- **Smoke Test Checklist**: Post-deployment verification (Admin only)
- **Operational Runbooks**: Common issue resolution guides (Admin only)
- **Admin Reports**: Sponsor list, LoG summary, coverage, sync log, audit log with CSV export
- **User Feedback**: In-app feedback collection for pilot/UAT participants

### Pilot Support (Step 9)
- **Demo Data Seeding**: Realistic test data for UAT, training, and demos
- **In-App Pilot Support**: Admin-only section with:
  - Demo account credentials display
  - Test data summary and reference
  - UAT scripts (10 test scenarios)
  - Pilot readiness checklist (100+ items)
  - Documentation hub (training, demo, handoff docs)
- **Comprehensive Documentation**: 8 documentation files (4,500+ lines) covering UAT, training, demos, handoff, limitations, roadmap

## Documentation

All documentation is located in the `docs/` folder:

### UAT and Testing
- [**UAT_GUIDE.md**](docs/UAT_GUIDE.md) - 10 test scenarios (UT01-UT10) with execution instructions
- [**DEFECT_TEMPLATE.md**](docs/DEFECT_TEMPLATE.md) - Defect reporting template with severity levels
- [**PILOT_READINESS_CHECKLIST.md**](docs/PILOT_READINESS_CHECKLIST.md) - 100+ item readiness verification checklist
- [**SAMPLE_TEST_DATA.md**](docs/SAMPLE_TEST_DATA.md) - Complete demo data reference

### Training and Demos
- [**TRAINING_GUIDE.md**](docs/TRAINING_GUIDE.md) - Role-specific quick-starts, 11 workflows, quick reference card
- [**DEMO_GUIDE.md**](docs/DEMO_GUIDE.md) - 10 presenter flows with talking points and Q&A prep

### Handoff and Technical
- [**HANDOFF.md**](docs/HANDOFF.md) - Business, technical, operational handoff documentation
- [**LIMITATIONS_AND_FUTURE_WORK.md**](docs/LIMITATIONS_AND_FUTURE_WORK.md) - Known limitations, future work backlog, post-pilot roadmap

### Recommended Reading Order

**For Business Stakeholders**: HANDOFF.md → DEMO_GUIDE.md → LIMITATIONS_AND_FUTURE_WORK.md → PILOT_READINESS_CHECKLIST.md

**For UAT Participants**: TRAINING_GUIDE.md → SAMPLE_TEST_DATA.md → UAT_GUIDE.md → DEFECT_TEMPLATE.md

**For Demo Presenters**: DEMO_GUIDE.md → SAMPLE_TEST_DATA.md → LIMITATIONS_AND_FUTURE_WORK.md

**For Technical Team**: HANDOFF.md → Operations Dashboard → Operational Runbooks → PILOT_READINESS_CHECKLIST.md

## In-App Features

### For All Authenticated Users
- Dashboard with role-appropriate widgets
- Sponsor search and profile viewing
- Reports (role-based access)
- User feedback submission

### For Admin Users Only
- Full CRUD on sponsors, students, LoGs, settings
- **Approve/reject sponsors** (Phase 2: Approval workflow with status tracking)
- Approve/reject change requests
- Merge duplicate sponsors
- Access to Operations Dashboard
- **Access to Integration Retry Dashboard** (Phase 1: Manual sync retry at /Operations/SyncRetry)
- **Enhanced Dashboard Widgets** (Phase 3: Pending Approvals & Failed Integrations monitoring)
- Access to Smoke Test Checklist
- Access to Operational Runbooks
- Access to Admin Reports (with CSV export)
- **Access to Pilot Support** (Step 9)
  - Demo account credentials
  - Test data summary
  - UAT scripts
  - Pilot readiness checklist
  - Documentation hub

### For Admissions Users
- Create sponsors
- Submit change requests (cannot approve own requests)
- View LoGs and coverage
- Limited report access

### For Cashier Users
- Read-only access to sponsors, students, LoGs
- View reports

### For Sponsor Users
- View own sponsor profile (future: Sponsor self-service portal)
- Limited portal access (placeholder for future phase)

## Project Structure

```
Controllers/          # MVC Controllers (Sponsors, Dashboard, Reports, Settings, Operations, PilotSupport)
Data/                # Database context, initialization, demo data seeder
Migrations/          # Entity Framework migrations
Models/              # Domain models (Sponsor, Student, LoG, ChangeRequest, etc.) and view models
Services/            # Business logic services (SponsorService, ChangeRequestService, CoverageEvaluationService, etc.)
Views/               # Razor views (Sponsors, Dashboard, Reports, Settings, Operations, PilotSupport, etc.)
wwwroot/             # Static files (CSS, JS, images)
docs/                # Step 9 documentation (UAT, training, demo, handoff, etc.)
Integration/         # Mock adapters for PowerSchool, NetSuite, etc. (future: real implementations)
Middleware/          # Audit logging middleware
HealthChecks/        # Custom health check implementations
```

## Development Roadmap

### Completed (Steps 1-10)
- ✅ **Step 1**: Database schema and migrations
- ✅ **Step 2**: Basic CRUD operations for sponsors, students, LoGs
- ✅ **Step 3**: Role-based access control (Admin, Admissions, Cashier, Sponsor)
- ✅ **Step 4**: Change request workflow (submit, approve, reject)
- ✅ **Step 5**: Coverage evaluation logic (Covered, Split, Not Covered)
- ✅ **Step 6**: Duplicate detection and merge workflow, integration mock adapters
- ✅ **Step 7**: Security hardening, audit logging, health checks, authentication/authorization
- ✅ **Step 8**: Operational monitoring, smoke tests, runbooks, admin reports, user feedback
- ✅ **Step 9**: UAT pack, demo data, training support, handoff documentation, pilot readiness
- ✅ **Step 10**: Integration Retry Dashboard with manual retry capability
- ✅ **Gap Closure - Phase 1**: Integration Retry Dashboard (/Operations/SyncRetry)
- ✅ **Gap Closure - Phase 2**: Sponsor Approval Status workflow with approval/rejection UI
- ✅ **Gap Closure - Phase 3**: Enhanced Admin Dashboard with Pending Approvals and Failed Integrations widgets
- ✅ **PowerSchool Integration**: Automatic Sponsor_OrgName custom field synchronization on sponsor create/update/merge events

### Planned (Post-Pilot)
- 🔜 **Phase 2 (0-6 months)**: Real integration with PowerSchool and NetSuite APIs, scheduled sync jobs, automated email notifications
- 🔜 **Phase 3 (6-12 months)**: Sponsor self-service portal, document attachment management, predictive analytics
- 🔜 **Phase 4 (12-24 months)**: Advanced reporting dashboards, mobile UI optimization, full DR/HA setup

See [docs/LIMITATIONS_AND_FUTURE_WORK.md](docs/LIMITATIONS_AND_FUTURE_WORK.md) for detailed future work backlog.

## Pilot Deployment

### Prerequisites
- Azure SQL Database (or equivalent) provisioned
- Azure App Service (or equivalent) configured
- HTTPS/TLS configured
- Environment variables configured (appsettings.Staging.json)
- Demo data seeding enabled

### Deployment Steps
1. Run database migrations: `dotnet ef database update`
2. Deploy application to staging/pilot environment
3. Verify demo data seeded (6 sponsors, 8 students, 5 LoGs, 4 users)
4. Run smoke test checklist (Operations → Smoke Test)
5. Execute UAT (follow docs/UAT_GUIDE.md)
6. Obtain stakeholder sign-off (docs/PILOT_READINESS_CHECKLIST.md)
7. Go/No-Go decision
8. Communicate pilot URL and support contacts to users

### Rollback Plan
1. Stop application
2. Restore database backup (taken before deployment)
3. Redeploy previous version
4. Verify smoke tests pass
5. Communicate rollback to users

See [docs/HANDOFF.md](docs/HANDOFF.md) for detailed deployment and rollback procedures.

## Support

### Technical Support
- Email: itsupport@ism.edu.ph
- Operational Runbooks: Available in-app (Operations → Runbooks)

### Business Owner
- TBD (update in docs/HANDOFF.md)

### Technical Lead
- TBD (update in docs/HANDOFF.md)

## License

Proprietary - International School Manila

---

**Last Updated**: March 30, 2026 - PowerSchool Sponsor_OrgName Integration Complete + Gap Closure Phases 1-3 Complete

