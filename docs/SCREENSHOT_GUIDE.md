# Screenshot Guide for User Manuals
## ISM Sponsor Management System

**Purpose:** This guide lists all screenshots needed for the user manuals.  
**Target Location:** `/docs/screenshots/` folder  
**Naming Convention:** `{role}_{section}_{description}.png`

---

## Screenshot Requirements

### Screenshot Specifications
- **Format:** PNG (preferred) or JPG
- **Resolution:** 1920x1080 or actual screen resolution
- **Quality:** High quality, clear text
- **Annotations:** Add red boxes/arrows for key elements (optional)
- **Privacy:** Remove/blur any sensitive real data

### Tools for Capturing
- **macOS:** Cmd+Shift+4 (select area) or Cmd+Shift+3 (full screen)
- **Windows:** Snipping Tool or Win+Shift+S
- **Annotation:** Use Preview (Mac), Paint (Windows), or Greenshot

---

## 1. Administrator Manual Screenshots

### A. Login & Dashboard (5 screenshots)

**1.1 Login Page**
- Filename: `admin_01_login_page.png`
- URL: `/Account/Login`
- Show: Login form with email/password fields
- Annotations: Highlight email field, password field, login button

**1.2 Admin Dashboard Overview**
- Filename: `admin_02_dashboard_overview.png`
- URL: `/Dashboard/AdminDashboard`
- Show: Full dashboard with all sections visible
- Annotations: Label each section (stats, alerts, recent activity)

**1.3 Dashboard Statistics Cards**
- Filename: `admin_03_dashboard_stats.png`
- URL: `/Dashboard/AdminDashboard`
- Show: Close-up of top statistics row
- Annotations: Highlight each metric card

**1.4 Dashboard Alerts Section**
- Filename: `admin_04_dashboard_alerts.png`
- URL: `/Dashboard/AdminDashboard`
- Show: Recent system alerts panel
- Annotations: Show different alert types (critical, warning, info)

**1.5 Quick Actions Panel**
- Filename: `admin_05_quick_actions.png`
- URL: `/Dashboard/AdminDashboard`
- Show: Quick action buttons
- Annotations: Highlight most-used actions

### B. Sponsor Management (8 screenshots)

**2.1 Sponsors List View**
- Filename: `admin_06_sponsors_list.png`
- URL: `/Settings/Sponsors`
- Show: Full sponsors list with search and filters
- Annotations: Label search box, filters, action buttons

**2.2 Sponsor Search in Action**
- Filename: `admin_07_sponsor_search.png`
- URL: `/Settings/Sponsors`
- Show: Search results for "Global Tech"
- Annotations: Highlight search box with query, results

**2.3 Sponsor Filters Applied**
- Filename: `admin_08_sponsor_filters.png`
- URL: `/Settings/Sponsors`
- Show: Filters panel expanded with options selected
- Annotations: Show active filters

**2.4 Create Sponsor Form (Empty)**
- Filename: `admin_09_create_sponsor_empty.png`
- URL: `/Settings/Sponsors/Create`
- Show: Empty create form
- Annotations: Mark required fields with asterisks

**2.5 Create Sponsor Form (Filled)**
- Filename: `admin_10_create_sponsor_filled.png`
- URL: `/Settings/Sponsors/Create`
- Show: Form completed with sample data
- Annotations: Show all sections filled

**2.6 Sponsor Details View**
- Filename: `admin_11_sponsor_details.png`
- URL: `/Settings/Sponsors/Details/{id}`
- Show: Complete sponsor details page
- Annotations: Highlight information sections, action buttons

**2.7 Edit Sponsor Form**
- Filename: `admin_12_edit_sponsor.png`
- URL: `/Settings/Sponsors/Edit/{id}`
- Show: Edit form with current values
- Annotations: Show editable vs read-only fields

**2.8 Sponsor Details - Associated LoGs**
- Filename: `admin_13_sponsor_logs_list.png`
- URL: `/Settings/Sponsors/Details/{id}` (scroll to LoGs section)
- Show: List of LoGs associated with sponsor
- Annotations: Highlight LoG list and school year selector

### C. LoG Management (10 screenshots)

**3.1 Portal/LoG List View**
- Filename: `admin_14_portal_logs_list.png`
- URL: `/Portal/Index`
- Show: Full LoG list with school year selector
- Annotations: Highlight school year dropdown, filters

**3.2 LoG Filters and Search**
- Filename: `admin_15_logs_filters.png`
- URL: `/Portal/Index`
- Show: Filter panel with options
- Annotations: Show status, sponsor, student filters

**3.3 Create LoG - Step 1 (School Year & Sponsor)**
- Filename: `admin_16_create_log_step1.png`
- URL: `/Portal/Create`
- Show: Top section with school year and sponsor selection
- Annotations: Highlight dropdowns, search functionality

**3.4 Create LoG - Step 2 (Student & Dates)**
- Filename: `admin_17_create_log_step2.png`
- URL: `/Portal/Create`
- Show: Student selection and date fields
- Annotations: Show date pickers

**3.5 Create LoG - Step 3 (Coverage Type)**
- Filename: `admin_18_create_log_coverage_type.png`
- URL: `/Portal/Create`
- Show: Coverage type dropdown expanded
- Annotations: Show coverage options

**3.6 Create LoG - Step 4 (Coverage Rules)**
- Filename: `admin_19_create_log_coverage_rules.png`
- URL: `/Portal/Create`
- Show: Coverage rules section with examples
- Annotations: Highlight add rule button, rule fields

**3.7 Create LoG - Coverage Rule Example**
- Filename: `admin_20_log_coverage_rule_detail.png`
- URL: `/Portal/Create`
- Show: One complete coverage rule
- Annotations: Label category, level, percentage, notes

**3.8 LoG Details View (Draft)**
- Filename: `admin_21_log_details_draft.png`
- URL: `/Portal/Details/{id}`
- Show: Draft LoG with all details
- Annotations: Highlight draft status, edit/activate buttons

**3.9 LoG Details View (Active)**
- Filename: `admin_22_log_details_active.png`
- URL: `/Portal/Details/{id}`
- Show: Active LoG with coverage rules
- Annotations: Show active status, limited actions

**3.10 LoG Activation Confirmation**
- Filename: `admin_23_log_activate_confirm.png`
- URL: `/Portal/Activate/{id}`
- Show: Activation confirmation dialog
- Annotations: Highlight warning message, buttons

### D. Request Review (6 screenshots)

**4.1 Change Requests List**
- Filename: `admin_24_requests_list.png`
- URL: `/ReviewRequest/Index`
- Show: List of pending requests
- Annotations: Show status indicators, filters

**4.2 Change Request Details**
- Filename: `admin_25_request_details.png`
- URL: `/ReviewRequest/Details/{id}`
- Show: Full request details with old/new values
- Annotations: Highlight current vs proposed values

**4.3 Change Request with Attachments**
- Filename: `admin_26_request_attachments.png`
- URL: `/ReviewRequest/Details/{id}`
- Show: Request with attached documents
- Annotations: Show attachment icons/links

**4.4 Approve Request Dialog**
- Filename: `admin_27_approve_request.png`
- URL: `/ReviewRequest/Details/{id}` (approve action)
- Show: Approval confirmation form
- Annotations: Show notes field, notification checkbox

**4.5 Reject Request Dialog**
- Filename: `admin_28_reject_request.png`
- URL: `/ReviewRequest/Reject/{id}`
- Show: Rejection form with reason dropdown
- Annotations: Highlight reason selection, notes

**4.6 Request Information Dialog**
- Filename: `admin_29_request_more_info.png`
- URL: `/ReviewRequest/RequestInfo/{id}`
- Show: Information request form
- Annotations: Show what to ask for field

### E. User Management (6 screenshots)

**5.1 Users List**
- Filename: `admin_30_users_list.png`
- URL: `/Settings/Users`
- Show: Complete users list with roles
- Annotations: Highlight role badges, status indicators

**5.2 Create User Form**
- Filename: `admin_31_create_user.png`
- URL: `/Settings/Users/Create`
- Show: User creation form
- Annotations: Show role checkboxes, password field

**5.3 Edit User Form**
- Filename: `admin_32_edit_user.png`
- URL: `/Settings/Users/Edit/{id}`
- Show: User edit form with current values
- Annotations: Show role modifications, status toggles

**5.4 Reset Password Dialog**
- Filename: `admin_33_reset_password.png`
- URL: `/Settings/Users/Edit/{id}` (reset password action)
- Show: Password reset form
- Annotations: Highlight temporary password, requirements

**5.5 Roles List**
- Filename: `admin_34_roles_list.png`
- URL: `/Settings/Roles`
- Show: System roles with user counts
- Annotations: Show role descriptions

**5.6 Role Permissions Matrix**
- Filename: `admin_35_role_permissions.png`
- URL: `/Settings/Roles/Details/{id}` (if exists) or create as table
- Show: Permissions table from manual
- Annotations: Use checkmarks/X marks clearly

### F. Reports (5 screenshots)

**6.1 Admin Reports Menu**
- Filename: `admin_36_reports_menu.png`
- URL: `/AdminReports/Index`
- Show: List of available reports
- Annotations: Categorize report types

**6.2 Report Filter Configuration**
- Filename: `admin_37_report_filters.png`
- URL: `/AdminReports/SponsorMaster` (or any report)
- Show: Filter panel for report
- Annotations: Show date range, dropdown filters

**6.3 Report Results View**
- Filename: `admin_38_report_results.png`
- URL: `/AdminReports/SponsorMaster` (after generate)
- Show: Generated report with data
- Annotations: Highlight export buttons, pagination

**6.4 Report Export Options**
- Filename: `admin_39_export_options.png`
- URL: Any report page (export dropdown expanded)
- Show: Export format buttons/dropdown
- Annotations: Show CSV, Excel, PDF options

**6.5 Generated Report Example**
- Filename: `admin_40_report_example.png`
- URL: Any report results
- Show: Actual report data in table format
- Annotations: Label columns, show summary stats

### G. Operations Dashboard (5 screenshots)

**7.1 Operations Dashboard Overview**
- Filename: `admin_41_operations_dashboard.png`
- URL: `/Operations/Dashboard`
- Show: Full operations dashboard
- Annotations: Label system health, components, metrics

**7.2 System Health Status**
- Filename: `admin_42_system_health.png`
- URL: `/Operations/Dashboard`
- Show: System health section with all components
- Annotations: Color code status indicators

**7.3 PowerSchool Integration Status**
- Filename: `admin_43_powerschool_status.png`
- URL: `/Operations/Dashboard`
- Show: PowerSchool sync section
- Annotations: Show last sync, schedule, history

**7.4 Performance Metrics**
- Filename: `admin_44_performance_metrics.png`
- URL: `/Operations/Dashboard`
- Show: Performance metrics panel
- Annotations: Highlight key metrics, graphs if present

**7.5 Recent Errors and Alerts**
- Filename: `admin_45_recent_errors.png`
- URL: `/Operations/Dashboard`
- Show: Error log section
- Annotations: Show different severity levels

### H. Advanced Features (4 screenshots)

**8.1 Bulk Operations Interface**
- Filename: `admin_46_bulk_operations.png`
- URL: `/Settings/Bulk` (if exists)
- Show: Bulk update interface
- Annotations: Show selection, action, preview

**8.2 Import Data Form**
- Filename: `admin_47_import_data.png`
- URL: `/Settings/Import`
- Show: Import wizard with file upload
- Annotations: Show template download, validation

**8.3 Export Configuration**
- Filename: `admin_48_export_config.png`
- URL: `/Settings/Export`
- Show: Export configuration form
- Annotations: Show field selection, format options

**8.4 Merge Sponsors Interface**
- Filename: `admin_49_merge_sponsors.png`
- URL: `/Duplicates/Index`
- Show: Merge interface with two sponsors
- Annotations: Show primary selection, merge preview

---

## 2. Admissions Manual Screenshots

### A. Login & Dashboard (3 screenshots)

**1.1 Admissions Dashboard**
- Filename: `admissions_01_dashboard.png`
- URL: `/Dashboard/AdmissionsDashboard`
- Show: Admissions-specific dashboard
- Annotations: Highlight admissions metrics, pending work

**1.2 Admissions Statistics**
- Filename: `admissions_02_stats.png`
- URL: `/Dashboard/AdmissionsDashboard`
- Show: Statistics cards for admissions
- Annotations: Show active LoGs, pending requests, drafts

**1.3 Recent Activity Feed**
- Filename: `admissions_03_recent_activity.png`
- URL: `/Dashboard/AdmissionsDashboard`
- Show: Recent actions by admissions user
- Annotations: Highlight activity log entries

### B. Sponsor Management (6 screenshots)

**2.1 Sponsor Search**
- Filename: `admissions_04_sponsor_search.png`
- URL: `/Settings/Sponsors`
- Show: Search in action with results
- Annotations: Highlight search process

**2.2 Create Sponsor (Admissions View)**
- Filename: `admissions_05_create_sponsor.png`
- URL: `/Settings/Sponsors/Create`
- Show: Create form as seen by admissions
- Annotations: Show same fields as admin

**2.3 Edit Sponsor (Direct Fields)**
- Filename: `admissions_06_edit_sponsor_direct.png`
- URL: `/Settings/Sponsors/Edit/{id}`
- Show: Edit form highlighting editable fields
- Annotations: Mark which fields can be edited directly

**2.4 Edit Sponsor (Request Required)**
- Filename: `admissions_07_edit_sponsor_request.png`
- URL: `/Settings/Sponsors/Edit/{id}`
- Show: Fields requiring change request
- Annotations: Show "Request Change" links

**2.5 Submit Change Request Form**
- Filename: `admissions_08_submit_request.png`
- URL: `/Settings/Sponsors/RequestChange`
- Show: Change request form
- Annotations: Highlight field, value, justification

**2.6 Change Request Submitted**
- Filename: `admissions_09_request_submitted.png`
- URL: After request submission
- Show: Confirmation message
- Annotations: Show request ID, status

### C. LoG Creation (8 screenshots)

**3.1 Portal List (Admissions View)**
- Filename: `admissions_10_portal_list.png`
- URL: `/Portal/Index`
- Show: LoG list with admissions permissions
- Annotations: Show create button, filters

**3.2 Create LoG - School Year Selection**
- Filename: `admissions_11_log_school_year.png`
- URL: `/Portal/Create`
- Show: School year dropdown expanded
- Annotations: Highlight current year, warning

**3.3 Create LoG - Sponsor Selection**
- Filename: `admissions_12_log_sponsor_select.png`
- URL: `/Portal/Create`
- Show: Sponsor dropdown with search
- Annotations: Show search in dropdown

**3.4 Create LoG - Student Selection**
- Filename: `admissions_13_log_student_select.png`
- URL: `/Portal/Create`
- Show: Student dropdown with grade info
- Annotations: Show student format (ID | Name - Grade)

**3.5 Create LoG - Date Fields**
- Filename: `admissions_14_log_dates.png`
- URL: `/Portal/Create`
- Show: Date picker interface
- Annotations: Show issue, effective, expiry dates

**3.6 Create LoG - Coverage Rules Examples**
- Filename: `admissions_15_log_coverage_examples.png`
- URL: `/Portal/Create`
- Show: Multiple coverage rules configured
- Annotations: Show full, partial, split examples

**3.7 Create LoG - Special Instructions**
- Filename: `admissions_16_log_special_instructions.png`
- URL: `/Portal/Create`
- Show: Special instructions field with example
- Annotations: Show pre-auth, limits, exclusions

**3.8 Create LoG - Save as Draft**
- Filename: `admissions_17_log_save_draft.png`
- URL: `/Portal/Create`
- Show: Completed form with Save Draft button
- Annotations: Highlight save options

### D. Change Requests (4 screenshots)

**4.1 My Change Requests List**
- Filename: `admissions_18_my_requests.png`
- URL: `/ReviewRequest/MyRequests`
- Show: List of submitted requests
- Annotations: Show status colors, filters

**4.2 Request Status - Pending**
- Filename: `admissions_19_request_pending.png`
- URL: `/ReviewRequest/MyRequests`
- Show: Pending request details
- Annotations: Highlight pending status

**4.3 Request Status - Approved**
- Filename: `admissions_20_request_approved.png`
- URL: `/ReviewRequest/MyRequests`
- Show: Approved request with confirmation
- Annotations: Show green checkmark, approval date

**4.4 Request Status - Rejected**
- Filename: `admissions_21_request_rejected.png`
- URL: `/ReviewRequest/MyRequests`
- Show: Rejected request with reason
- Annotations: Highlight rejection reason, feedback

### E. Reports (3 screenshots)

**5.1 Admissions Reports Menu**
- Filename: `admissions_22_reports_menu.png`
- URL: `/AdmissionsReports/Index`
- Show: Available reports for admissions
- Annotations: List report types

**5.2 My Created LoGs Report**
- Filename: `admissions_23_my_logs_report.png`
- URL: `/AdmissionsReports/MyLoGs`
- Show: Report showing user's LoGs
- Annotations: Show filters, results

**5.3 Pending Activations Report**
- Filename: `admissions_24_pending_activations.png`
- URL: `/AdmissionsReports/PendingActivations`
- Show: Draft LoGs awaiting activation
- Annotations: Highlight age, created by

---

## 3. Cashier Manual Screenshots

### A. Dashboard (2 screenshots)

**1.1 Cashier Dashboard**
- Filename: `cashier_01_dashboard.png`
- URL: `/Dashboard/CashierDashboard`
- Show: Cashier dashboard with stats
- Annotations: Highlight read-only nature, quick search

**1.2 Quick Search Panel**
- Filename: `cashier_02_quick_search.png`
- URL: `/Dashboard/CashierDashboard`
- Show: Quick search boxes
- Annotations: Show sponsor and LoG search

### B. Viewing Sponsors (3 screenshots)

**2.1 Sponsors List (Read-Only)**
- Filename: `cashier_03_sponsors_list.png`
- URL: `/Settings/Sponsors`
- Show: Sponsors list without edit buttons
- Annotations: Highlight view-only access

**2.2 Sponsor Details (Cashier View)**
- Filename: `cashier_04_sponsor_details.png`
- URL: `/Settings/Sponsors/Details/{id}`
- Show: Sponsor details for billing reference
- Annotations: Highlight contact info, address

**2.3 Sponsor Search by Student**
- Filename: `cashier_05_search_by_student.png`
- URL: `/Settings/Sponsors`
- Show: Search results for student name
- Annotations: Show student search finds sponsor

### C. Viewing LoGs (4 screenshots)

**3.1 LoG List (Cashier View)**
- Filename: `cashier_06_logs_list.png`
- URL: `/Portal/Index`
- Show: LoG list with view-only access
- Annotations: Show no edit buttons, read-only

**3.2 LoG Details - Coverage Rules**
- Filename: `cashier_07_log_coverage_rules.png`
- URL: `/Portal/Details/{id}`
- Show: Coverage rules table prominent
- Annotations: Highlight covered/split/not covered

**3.3 LoG Details - Example Full Coverage**
- Filename: `cashier_08_log_full_coverage.png`
- URL: `/Portal/Details/{id}`
- Show: LoG with 100% coverage all categories
- Annotations: Show all green checkmarks

**3.4 LoG Details - Example Split Coverage**
- Filename: `cashier_09_log_split_coverage.png`
- URL: `/Portal/Details/{id}`
- Show: LoG with percentage splits
- Annotations: Highlight split percentages

### D. Coverage Verification (3 screenshots)

**4.1 Coverage Rules Table - Full View**
- Filename: `cashier_10_coverage_table.png`
- URL: `/Portal/Details/{id}`
- Show: Complete coverage rules table
- Annotations: Label each category, clearly show icons

**4.2 Special Instructions Example**
- Filename: `cashier_11_special_instructions.png`
- URL: `/Portal/Details/{id}`
- Show: Special instructions with pre-auth
- Annotations: Highlight pre-auth, limits, exclusions

**4.3 Coverage Summary Print View**
- Filename: `cashier_12_coverage_print.png`
- URL: `/Portal/Print/{id}` or PDF export
- Show: Printable coverage summary
- Annotations: Show print-friendly format

### E. Reports (3 screenshots)

**5.1 Cashier Reports Menu**
- Filename: `cashier_13_reports_menu.png`
- URL: `/CashierReports/Index`
- Show: Available cashier reports
- Annotations: List report types

**5.2 Billing Reconciliation Report**
- Filename: `cashier_14_reconciliation_report.png`
- URL: `/CashierReports/Reconciliation`
- Show: Report with filters and results
- Annotations: Show date range, sponsor filter, totals

**5.3 Student Coverage List**
- Filename: `cashier_15_student_coverage_list.png`
- URL: `/CashierReports/StudentCoverage`
- Show: List for desk reference
- Annotations: Show student, sponsor, coverage columns

---

## 4. Common/Shared Screenshots

### A. Navigation (3 screenshots)

**1.1 Main Navigation Bar**
- Filename: `common_01_main_nav.png`
- URL: Any page
- Show: Top navigation bar
- Annotations: Label all menu items

**1.2 User Menu Dropdown**
- Filename: `common_02_user_menu.png`
- URL: Any page (user menu expanded)
- Show: Profile, Feedback, Logout options
- Annotations: Show dropdown items

**1.3 School Year Selector**
- Filename: `common_03_school_year_selector.png`
- URL: `/Portal/Index` or any page with selector
- Show: School year dropdown expanded
- Annotations: Show current, past, future years

### B. Forms & Validation (4 screenshots)

**2.1 Required Field Indicators**
- Filename: `common_04_required_fields.png`
- URL: Any create form
- Show: Form with asterisks on required fields
- Annotations: Highlight red asterisks

**2.2 Validation Errors**
- Filename: `common_05_validation_errors.png`
- URL: Any form (submit with errors)
- Show: Form with validation error messages
- Annotations: Show red error text, highlighted fields

**2.3 Success Message**
- Filename: `common_06_success_message.png`
- URL: After successful action
- Show: Green success banner/alert
- Annotations: Show success icon, message

**2.4 Confirmation Dialog**
- Filename: `common_07_confirmation_dialog.png`
- URL: Before delete/deactivate action
- Show: Confirmation popup/modal
- Annotations: Show warning, buttons

### C. Search & Filter (3 screenshots)

**3.1 Search Box Examples**
- Filename: `common_08_search_boxes.png`
- URL: Various pages
- Show: Different search implementations
- Annotations: Show search icon, placeholder text

**3.2 Filter Panel Collapsed**
- Filename: `common_09_filters_collapsed.png`
- URL: Any list page
- Show: Filter section minimized
- Annotations: Show expand/collapse button

**3.3 Filter Panel Expanded**
- Filename: `common_10_filters_expanded.png`
- URL: Any list page
- Show: All filter options visible
- Annotations: Label each filter type

---

## 5. Error States & Edge Cases (Optional but Recommended)

**1. No Results Found**
- Filename: `common_11_no_results.png`
- Show: Empty state with helpful message

**2. Loading State**
- Filename: `common_12_loading.png`
- Show: Loading spinner or progress indicator

**3. 404 Error Page**
- Filename: `common_13_404_page.png`
- Show: Custom 404 error page if exists

**4. Permission Denied**
- Filename: `common_14_permission_denied.png`
- Show: Access denied message

**5. Network Error**
- Filename: `common_15_network_error.png`
- Show: Connection error message

---

## Screenshot Capture Workflow

### Step 1: Set Up Test Environment
1. Deploy application to test/staging environment
2. Seed with sample data (use DemoDataSeeder)
3. Create test user accounts for each role
4. Have 2-3 browser windows ready (different roles)

### Step 2: Prepare Browser
1. Use Chrome or Firefox (latest version)
2. Set browser to 1920x1080 resolution (or your standard)
3. Zoom to 100%
4. Close unnecessary tabs
5. Clear notifications/alerts

### Step 3: Capture Process
For each screenshot:
1. Navigate to specified URL
2. Wait for page to fully load
3. Remove demo banners if present
4. Take screenshot (Cmd+Shift+4 on Mac)
5. Save with specified filename
6. Verify image quality
7. Annotate if needed

### Step 4: Annotation (Optional)
Use Greenshot, Skitch, or similar tool:
1. Add red boxes around key elements
2. Add arrows pointing to important features
3. Add numbered callouts for sequences
4. Keep annotations minimal and clear

### Step 5: Organization
```
/docs/screenshots/
  ├── admin/
  │   ├── dashboard/
  │   ├── sponsors/
  │   ├── logs/
  │   ├── requests/
  │   ├── users/
  │   ├── reports/
  │   └── operations/
  ├── admissions/
  │   ├── dashboard/
  │   ├── sponsors/
  │   ├── logs/
  │   └── reports/
  ├── cashier/
  │   ├── dashboard/
  │   ├── sponsors/
  │   ├── logs/
  │   └── reports/
  └── common/
      ├── navigation/
      ├── forms/
      └── search/
```

### Step 6: Insert into Manuals
After capturing, update markdown files:
```markdown
![Dashboard Overview](screenshots/admin/dashboard/admin_02_dashboard_overview.png)
*Figure 2.1: Admin Dashboard showing system overview and key metrics*
```

---

## Screenshot Checklist

Use this checklist to track progress:

### Administrator Manual
- [ ] Login & Dashboard (5)
- [ ] Sponsor Management (8)
- [ ] LoG Management (10)
- [ ] Request Review (6)
- [ ] User Management (6)
- [ ] Reports (5)
- [ ] Operations (5)
- [ ] Advanced Features (4)
**Total: 49 screenshots**

### Admissions Manual
- [ ] Dashboard (3)
- [ ] Sponsor Management (6)
- [ ] LoG Creation (8)
- [ ] Change Requests (4)
- [ ] Reports (3)
**Total: 24 screenshots**

### Cashier Manual
- [ ] Dashboard (2)
- [ ] Viewing Sponsors (3)
- [ ] Viewing LoGs (4)
- [ ] Coverage Verification (3)
- [ ] Reports (3)
**Total: 15 screenshots**

### Common/Shared
- [ ] Navigation (3)
- [ ] Forms & Validation (4)
- [ ] Search & Filter (3)
- [ ] Error States (5, optional)
**Total: 15 screenshots**

---

## Grand Total: ~103 Screenshots

**Estimated Time:**
- Setup: 1 hour
- Capture: 3-4 hours
- Annotation: 2-3 hours
- Integration: 1-2 hours
**Total: 7-10 hours**

---

## Tips for Best Results

✅ **Consistency:**
- Use same browser and zoom level
- Capture at same time of day (consistent lighting)
- Use same test data where possible

✅ **Quality:**
- Clear, readable text
- No blurry images
- Proper framing (not too much empty space)

✅ **Privacy:**
- Use fake/demo data only
- No real names, emails, addresses
- No sensitive information visible

✅ **Annotations:**
- Keep minimal (don't clutter)
- Use consistent colors (red for highlights)
- Add figure numbers and captions

✅ **Maintenance:**
- Date screenshots (in folder or metadata)
- Re-capture when UI changes
- Keep source files organized

---

**Next Steps:**
1. Review this guide
2. Set up test environment with demo data
3. Begin capturing screenshots systematically
4. Annotate as needed
5. Update markdown files with image references
6. Build PDF versions with images included

---

**Questions or Issues?**
Contact: dev.team@ism.edu.ph
