# Demo Walkthrough Guide
## ISM Sponsor Management System

**Version:** 1.0  
**Date:** March 2026  
**Purpose:** Presenter script for stakeholder demonstrations

---

## Table of Contents

1. [Demo Overview](#demo-overview)
2. [Demo Preparation](#demo-preparation)
3. [Demo Flows](#demo-flows)
4. [Talking Points](#talking-points)
5. [Q&A Preparation](#qa-preparation)

---

## Demo Overview

### Audience
- School administrators
- Finance/cashier staff
- Admissions staff
- IT stakeholders
- Executive leadership

### Duration
- **Quick Demo:** 15 minutes (core flows only)
- **Full Demo:** 45 minutes (all flows with Q&A)
- **Deep Dive:** 90 minutes (includes operational features and reporting)

### Demo Objectives
1. Show how the system centralizes sponsor data
2. Demonstrate LoG coverage rule management
3. Illustrate real-time coverage evaluation
4. Highlight approval workflows and controls
5. Show duplicate detection and merge capability
6. Demonstrate audit trails and reporting
7. Build confidence in pilot readiness

---

## Demo Preparation

### Before the Demo
- [ ] Seed demo data successfully
- [ ] Test all demo accounts (demo.admin@ism.edu.ph, demo.admissions@ism.edu.ph, demo.cashier@ism.edu.ph)
- [ ] Clear browser cache
- [ ] Open demo URLs in separate tabs
- [ ] Prepare backup browser window in case of issues
- [ ] Have talking points ready
- [ ] Set screen resolution to 1920x1080 (or presentation-friendly size)
- [ ] Disable browser notifications and popups
- [ ] Have demo data cheat sheet ready (sponsor IDs, student IDs, etc.)

### Demo Data Summary
- **Sponsors:** 6 demo sponsors (DEMO-SP001 to DEMO-SP006)
- **Students:** 8 students across different grade levels
- **LoGs:** 5 LoGs with Active, UnderReview, and Inactive statuses
- **Change Requests:** 4 requests showing pending, approved, and rejected states
- **Users:** 4 demo accounts (Admin, Admissions, Cashier, Sponsor)

---

## Demo Flows

### Flow 1: Sign In by Role (5 minutes)

**Purpose:** Show role-based access and personalized dashboards

**Script:**

1. **Open login page**
   - _"Let me show you how users access the system."_
   
2. **Log in as Admin (demo.admin@ism.edu.ph / Demo@2026!)**
   - _"First, I'll log in as an Admin user who has full system access."_
   - **Point out:** Dashboard shows pending approval requests, system health, and operational metrics
   - _"Notice the Admin sees pending requests awaiting approval and system health status."_

3. **Log out and log in as Admissions (demo.admissions@ism.edu.ph / Demo@2026!)**
   - _"Now let's see what an Admissions staff member sees."_
   - **Point out:** Different navigation menu, focused on sponsor creation and LoG management
   - _"Admissions users see tools for creating sponsors, managing LoGs, and submitting change requests, but they don't have full admin privileges."_

4. **Log out and log in as Cashier (demo.cashier@ism.edu.ph / Demo@2026!)**
   - _ "Finally, here's the Cashier view."_
   - **Point out:** Read-only access to sponsors and LoGs, reconciliation reports
   - _"Cashiers can view sponsor information and generate reconciliation reports but cannot modify sponsor data."_

**Expected Questions:**
- Q: "Can we customize dashboards per role?"
- A: "Yes, each role sees contextually relevant information. We can adjust dashboard widgets in future phases."

**Talking Points:**
- Role-based access control (RBAC) ensures users only see what they need
- Reduces training burden by showing relevant features only
- Improves security by limiting access to sensitive operations

---

### Flow 2: Sponsor Search and Create (7 minutes)

**Purpose:** Show sponsor data management and validation

**Script:**

1. **Log in as Admin or Admissions**

2. **Navigate to Sponsors list**
   - _"Here's our centralized sponsor registry."_
   - **Point out:** List shows sponsor ID, name, active status, student count, sync status
   - _"We're tracking 6 demo sponsors here. Notice the sync icons showing which sponsors have been synced to PowerSchool or NetSuite."_

3. **Use search/filter**
   - _"We can search by sponsor name or ID."_
   - Search for "Global Tech"
   - **Point out:** Filtered results

4. **Click "Create New Sponsor"**
   - _"Let me create a new sponsor to show the data entry flow."_
   
5. **Enter sponsor details:**
   - Sponsor ID: DEMO-SP-NEW
   - Sponsor Name: Pacific Resources Group
   - Legal Name: Pacific Resources Group Inc.
   - Contact Person: Maria Chen
   - Contact Email: m.chen@pacificres.com
   - Contact Phone: +63 2 8555 7890
   - Address: 10th Floor, Corporate Tower, Makati
   - City: Makati, State: Metro Manila, Postal: 1200, Country: Philippines
   - Is Active: Checked

6. **Click "Create"**
   - **Point out:** Success message
   - _"Notice the confirmation message and we're redirected back to the list."_

7. **View newly created sponsor**
   - **Point out:** Created timestamp, audit trail
   - _"The system automatically tracks who created this record and when."_

**Expected Questions:**
- Q: "What happens if we enter a duplicate sponsor?"
- A: "Great question—I'll show you duplicate detection in a moment. The system can identify similar sponsors and help you merge them."

**Talking Points:**
- Single source of truth for sponsor data
- Reduces manual errors with validation
- Automatic audit trail for compliance
- Integration-ready (PowerSchool, NetSuite sync)

---

### Flow 3: Sponsor Edit and Validation Messages (5 minutes)

**Purpose:** Demonstrate data validation and user-friendly error handling

**Script:**

1. **Click "Edit" on DEMO-SP001**
   - _"Now let's edit an existing sponsor."_

2. **Clear the Contact Email field (make it blank)**

3. **Enter invalid phone format like "12345"**

4. **Click "Save"**
   - **Point out:** Validation messages appear
   - _"Notice the system provides clear, specific error messages right next to the fields."_
   - _"The email field is required, and the phone format is invalid."_

5. **Correct the errors:**
   - Restore valid email: maria.santos@globaltech.com
   - Correct phone: +63 2 8123 4567

6. **Click "Save" again**
   - **Point out:** Success message
   - _"With valid data, the update succeeds and we see a confirmation message."_

**Expected Questions:**
- Q: "Can we customize validation rules?"
- A: "Yes, validation rules are configurable. For example, we can adjust phone number formats for international sponsors."

**Talking Points:**
- User-friendly error messages reduce help desk calls
- Real-time validation prevents bad data entry
- System guides users to correct issues quickly

---

### Flow 4: Sponsor Request Submission (7 minutes)

**Purpose:** Show approval workflow for controlled changes

**Script:**

1. **Log in as Admissions user (demo.admissions@ism.edu.ph)**

2. **Navigate to DEMO-SP002 (Asian Development Bank)**

3. **Click "Request Change"**
   - _"Admissions staff can't directly edit certain fields. They submit change requests for Admin approval."_

4. **Fill out change request:**
   - Field to Change: "Contact Email"
   - Current Value: (auto-populated) j.lee@adb.org
   - Proposed Value: john.lee@adb.org
   - Justification: "Updated to full first name per sponsor request via email 3/5/2026"

5. **Click "Submit Request"**
   - **Point out:** Success message, request appears in "My Requests"
   - _"The request is now pending Admin approval."_

6. **Log out and log in as Admin (demo.admin@ism.edu.ph)**

7. **Navigate to Dashboard**
   - **Point out:** Pending request appears in dashboard widget
   - _"Admins see pending requests right on their dashboard."_

8. **Click "Review Requests"**
   - _"Now I'll review the pending request."_

9. **Click "Review" on the request**
   - **Point out:** Shows old vs. new value, justification, requester
   - _"The Admin sees complete context: what's changing, why, and who requested it."_

10. **Enter review comments: "Approved. Email confirmed with sponsor liaison."**

11. **Click "Approve"**
    - **Point out:** Success message
    - _"The system automatically applies approved changes to the sponsor record."_

12. **Navigate back to DEMO-SP002 detail**
    - **Point out:** Email updated to john.lee@adb.org
    - _"The change is now reflected in the sponsor profile."_

**Expected Questions:**
- Q: "Can we configure which fields require approval?"
- A: "Yes, we can designate sensitive fields that require approval workflows."

- Q: "What if a request is rejected?"
- A: "Rejected requests are logged with the Admin's comments. The requester is notified (if notifications are enabled) and can resubmit with more information."

**Talking Points:**
- Approval workflow ensures data integrity
- Audit trail captures who requested, who approved, and when
- Reduces unauthorized changes to critical sponsor data
- Balances empowerment (Admissions can propose changes) with control (Admin approves)

---

### Flow 5: Dashboard Pending Request Follow-Up (3 minutes)

**Purpose:** Show operational awareness via dashboard

**Script:**

1. **Log in as Admin**

2. **View Dashboard**
   - **Point out dashboard sections:**
     - Pending approval requests count
     - Pending LoG reviews count
     - System health indicators (green/yellow/red)
     - Recent sync attempts
   - _"The dashboard gives at-a-glance visibility into pending work and system health."_

3. **Click "View All Pending Requests"**
   - **Point out:** Filtered list of all pending change requests
   - _"Admins can quickly review and act on all pending requests from one place."_

4. **Return to Dashboard**

5. **Scroll to "System Health" section**
   - **Point out:** Database (Healthy), Sync (Healthy), Audit (Healthy)
   - _"Green indicators show all systems operating normally. If there were issues, we'd see yellow (degraded) or red (unhealthy)."_

**Talking Points:**
- Dashboard provides operational situational awareness
- Reduces need to navigate multiple pages to find pending work
- Proactive alerts for system issues

---

### Flow 6: LoG Create/Edit and Coverage View (8 minutes)

**Purpose:** Demonstrate LoG workflow and coverage rule management

**Script:**

1. **Log in as Admissions user**

2. **Navigate to "Letters of Guarantee" (LoGs)**
   - _"Letters of Guarantee define what expenses each sponsor will cover."_

3. **Filter by School Year: 2025-2026, Status: Active**
   - **Point out:** Filtered list shows active LoGs
   - _"We have 3 active LoGs for the current school year."_

4. **Click on DEMO-LOG001 for Global Tech Corporation**

5. **View LoG detail page**
   - **Point out:**
     - LoG header (ID, Sponsor, School Year, Status, dates)
     - Coverage rules table (Item, Coverage Type, Sponsor %, Parent %)
   - _"This LoG shows Global Tech covers 100% of tuition, but only 70% of bus service and 50% of lunch."_

6. **Explain coverage types:**
   - **Covered (100% sponsor, 0% parent):** "Sponsor pays all"
   - **Split (X% sponsor, Y% parent):** "Cost split between sponsor and parent"
   - **NotCovered (0% sponsor, 100% parent):** "Parent pays all"

7. **Click "Edit LoG" or "Add Coverage Rule"**
   - _"We can add new items or adjust percentages."_

8. **Add a new rule:**
   - Item: "TECHNOLOGY" (Technology Fee)
   - Coverage Type: Covered
   - Sponsor %: 100, Parent %: 0

9. **Click "Save"**
   - **Point out:** New rule appears in table
   - _"The updated LoG now includes technology fees as fully covered."_

**Expected Questions:**
- Q: "Can rules change mid-year?"
- A: "Yes, but typically we create new LoGs for mid-year changes. We can also deactivate old LoGs and activate new ones to maintain history."

- Q: "What if a student has multiple sponsors?"
- A: "Currently, each student is linked to one primary sponsor. Future phases could support split sponsorships with priority rules."

**Talking Points:**
- LoG centralizes coverage policy in one place
- Reduces confusion about what's covered vs. not covered
- Coverage rules drive downstream billing decisions
- Audit trail tracks all LoG changes

---

### Flow 7: Coverage Evaluation Demo (6 minutes)

**Purpose:** Show real-time API coverage evaluation

**Script:**

1. **Navigate to Coverage Evaluation (if public page) or API testing tool**
   - _"Let me show you the coverage evaluation engine."_

2. **Enter evaluation parameters:**
   - Sponsor ID: DEMO-SP001
   - Student ID: DEMO-ST001
   - Item Code: TUITION-ES
   - Amount: 450,000 PHP

3. **Click "Evaluate Coverage" (or call API)**
   - **Point out:** Response within 1-2 seconds
   - _"The system evaluates coverage in real-time."_

4. **Review evaluation result:**
   - Decision: "Covered"
   - Sponsor Amount: 450,000 PHP
   - Parent Amount: 0 PHP
   - Bill To: "Sponsor"
   - Rule Applied: "TUITION-ES - Covered (100% sponsor)"

5. **Change Item Code to BUS-SERVICE, Amount to 80,000 PHP**

6. **Evaluate again**
   - **Point out:**
     - Decision: "Split"
     - Sponsor Amount: 56,000 PHP (70%)
     - Parent Amount: 24,000 PHP (30%)
     - Bill To: "Both"
   - _"For bus service, the cost is split 70/30 per the LoG rules."_

7. **Change Item Code to UNIFORM (not in LoG), Amount to 5,000 PHP**

8. **Evaluate**
   - **Point out:**
     - Decision: "NotCovered"
     - Sponsor Amount: 0 PHP
     - Parent Amount: 5,000 PHP
     - Bill To: "Parent"
   - _"If an item isn't in the LoG, it defaults to parent responsibility."_

**Expected Questions:**
- Q: "How does this integrate with PowerSchool or OBS?"
- A: "This API can be called from PowerSchool or OBS during charge entry. The response tells the billing system how to split the charge."

- Q: "What about exceptions or overrides?"
- A: "Future phases can include override capability with approval workflow."

**Talking Points:**
- Real-time evaluation eliminates manual lookups
- Consistent coverage decisions reduce billing errors
- API-based design enables integration with existing systems
- Audit trail captures all evaluations for reconciliation

---

### Flow 8: Duplicate Merge Demo (7 minutes)

**Purpose:** Show duplicate detection and controlled merge

**Script:**

1. **Log in as Admin**

2. **Navigate to "Duplicate Detection" (or `/Sponsors/DetectDuplicates`)**
   - _"The system can identify potential duplicate sponsors."_

3. **Click "Run Duplicate Detection" (or view pre-run results)**
   - **Point out:** Suggested duplicate pairs
   - _"The system found two sponsors with similar names: 'Global Tech Corporation' and 'Global Tech Corp'."_

4. **Click "Review" or "Merge" on the pair DEMO-SP001 and DEMO-SP006**

5. **Review merge preview:**
   - **Primary Record:** DEMO-SP001 (Global Tech Corporation)
   - **Record to Merge:** DEMO-SP006 (Global Tech Corp)
   - **Point out side-by-side comparison:**
     - Name, contact, address
     - Students: DEMO-SP001 has 2 students, DEMO-SP006 has 1 student
     - LoGs: DEMO-SP001 has 1 LoG, DEMO-SP006 has none
   - _"The preview shows what will happen: students and LoGs from DEMO-SP006 will be reassigned to DEMO-SP001."_

6. **Confirm merge**
   - **Point out:** Success message: "Sponsors merged successfully"

7. **Navigate to Sponsors list**
   - Search for DEMO-SP006
   - **Point out:** DEMO-SP006 no longer appears in active list (or shows "Merged" status)
   - _"The duplicate record is marked as merged and hidden from active lists."_

8. **Click on DEMO-SP001**
   - **Point out:** Student count increased, audit log shows merge action
   - _"All related records are preserved and reassigned to the primary sponsor."_

**Expected Questions:**
- Q: "Can we undo a merge?"
- A: "Not automatically, but because we mark records as 'merged' rather than deleting them, we can manually reverse if needed."

- Q: "What if we merge the wrong records?"
- A: "The preview step reduces that risk. We can add confirmation prompts or additional checks if desired."

**Talking Points:**
- Duplicate detection improves data quality
- Controlled merge prevents data loss (no deletion, only reassignment)
- Audit trail tracks merge actions
- Reduces confusion from multiple records for same sponsor

---

### Flow 9: Audit Retrieval Demo (5 minutes)

**Purpose:** Show audit logging and compliance support

**Script:**

1. **Log in as Admin**

2. **Navigate to "Logs" or "Audit Logs"**
   - _"Every significant action in the system is logged for audit and compliance."_

3. **View audit log list**
   - **Point out columns:**
     - Date/Time
     - Module (Sponsor, LoG, ChangeRequest)
     - Action/Details
     - User
     - Role
   - _"We can see who did what and when."_

4. **Apply filter: Last 7 days, Module: Sponsor**
   - **Point out:** Filtered results show recent sponsor activities
   - _"For example, here are all sponsor-related actions from the past week."_

5. **Click on a log entry to view details (if applicable)**
   - **Point out:** Full details including before/after values
   - _"We can drill down to see exactly what changed."_

6. **Apply filter: User: demo.admissions@ism.edu.ph**
   - **Point out:** All actions by specific user
   - _"Useful for reviewing a specific user's activity or investigating issues."_

**Expected Questions:**
- Q: "How long are audit logs retained?"
- A: "Audit logs are retained indefinitely in the database. We can implement archiving rules during production hardening."

- Q: "Can we export audit logs?"
- A: "Yes, we can add CSV export to the audit log view, similar to reports."

**Talking Points:**
- Comprehensive audit trail supports compliance (e.g., who approved what, when)
- Helps troubleshoot issues ("What changed before this broke?")
- Transparency and accountability for sensitive data changes

---

### Flow 10: Report and Monitoring Demo (7 minutes)

**Purpose:** Show reporting and operational monitoring capabilities

**Script:**

1. **Log in as Admin**

2. **Navigate to "Reports"**
   - _"The system includes several built-in reports."_

3. **Click "Sponsor Master Report"**
   - Enter date range: Last 30 days
   - Click "Generate Report"
   - **Point out:**
     - Summary metrics (total sponsors, active, inactive, created in period)
     - Detailed table (sponsor ID, name, students, LoGs, sync status)
   - _"This report gives a complete snapshot of our sponsor population."_

4. **Click "Export to CSV"**
   - **Point out:** CSV file downloads
   - _"Reports can be exported for further analysis in Excel."_

5. **Navigate back to Reports, click "LoG Activity Report"**
   - Filter: School Year 2025-2026, Status: Active
   - **Point out:** Shows LoG activity and rule counts
   - _"Useful for tracking LoG lifecycle and approvals."_

6. **Navigate to "Operations Dashboard"**
   - _"Let me show you the operational monitoring features."_

7. **View Operations Dashboard**
   - **Point out:**
     - System Health: Database (Healthy), Sync (Healthy), Audit (Healthy)
     - Recent Sync Attempts table
     - Recent Sync Failures (if any)
     - Data Consistency Warnings (if any)
   - _"This dashboard helps IT and operations teams monitor system health."_

8. **Click "Smoke Test" (if demo-appropriate)**
   - **Point out:** Post-deployment verification checklist
   - _"After each deployment, we can run through this checklist to verify all features work."_

**Expected Questions:**
- Q: "Can we schedule reports to run automatically?"
- A: "Not yet, but that's a natural next enhancement. We could email reports daily or weekly."

- Q: "What happens if sync fails?"
- A: "Failed syncs are logged on the operations dashboard with error details. IT can retry or investigate the cause."

**Talking Points:**
- Built-in reports reduce ad-hoc data requests
- CSV export enables further analysis
- Operational monitoring provides proactive issue detection
- Smoke tests ensure quality after deployments

---

## Talking Points

### Business Value
- **Centralized sponsor data:** Eliminates scattered spreadsheets and email chains
- **Reduced billing errors:** Automated coverage evaluation ensures consistent decisions
- **Audit compliance:** Every action tracked for accountability
- **Time savings:** Admissions staff spend less time on manual lookups and approvals
- **Data quality:** Duplicate detection and validation reduce errors

### Technical Strengths
- **Modern architecture:** ASP.NET Core MVC + Web API
- **Role-based security:** Fine-grained access control
- **Integration-ready:** API-first design for PowerSchool, NetSuite, OBS integration
- **Audit trail:** Comprehensive logging for compliance
- **Scalable:** Designed to handle growth in sponsors, students, and transactions

### Pilot Readiness
- **Core workflows implemented:** Sponsor CRUD, LoG management, approval workflows, reporting
- **Demo data seeded:** Realistic test scenarios ready
- **UAT scripts prepared:** 10 scenarios across 4 roles
- **Training materials available:** Quick-start guides by role
- **Operational support:** Runbooks, smoke tests, feedback system

---

## Q&A Preparation

### Common Questions and Answers

**Q: When can we start using this in production?**  
A: We're ready for pilot deployment pending UAT sign-off. Full production rollout would follow a successful pilot period (typically 2-3 months).

**Q: What training is required?**  
A: We've prepared role-based quick-start guides (15-30 minutes per role) and hands-on demo sessions. Most users will need 1-2 hours of training.

**Q: What if we find bugs during the pilot?**  
A: We have a defect logging process and prioritization framework. Critical issues will be addressed immediately; lower-priority items can be scheduled for post-pilot releases.

**Q: How does this integrate with PowerSchool and NetSuite?**  
A: The coverage evaluation API can be called from PowerSchool during charge entry. Sponsor and LoG data can sync to NetSuite for invoicing. Full integration details in technical handoff docs.

**Q: Can we customize fields or add new features?**  
A: Yes. The system is built with extensibility in mind. We can add custom fields, new reports, or workflow adjustments based on pilot feedback.

**Q: What about data migration from our current systems?**  
A: We can import sponsor data via CSV or database scripts. LoG data may require manual entry initially to ensure accuracy.

**Q: What are the known limitations?**  
A: See [LIMITATIONS_AND_FUTURE_WORK.md](LIMITATIONS_AND_FUTURE_WORK.md). Key limitations: prototype scope only, no advanced monitoring/DR/HA yet, no predictive analytics, external system UIs not redesigned.

**Q: How do we report issues or provide feedback?**  
A: We've built a feedback system into the app (Feedback link in navigation). Users can categorize issues by severity and module.

**Q: What's the support model post-pilot?**  
A: TBD based on pilot outcomes. Likely a combination of internal IT support and vendor/development team escalation path.

**Q: What happens to our data if we decide not to proceed?**  
A: All data is stored in your SQL Server database. You retain full ownership and can export or migrate data as needed.

---

## Demo Tips

### Do's
- ✅ Start with login/RBAC to set security context
- ✅ Use realistic demo data and scenarios
- ✅ Pause for questions after each major flow
- ✅ Highlight business value, not just technical features
- ✅ Show both happy path and error handling
- ✅ Have backup browser window ready in case of issues
- ✅ Keep demo moving; don't get stuck on details
- ✅ Relate features to audience pain points

### Don'ts
- ❌ Rush through flows without explaining "why it matters"
- ❌ Show technical errors or stack traces
- ❌ Use production data or real sponsor names
- ❌ Promise features not yet implemented ("We'll have that soon" vs "That could be added in future phases")
- ❌ Skip testing the demo beforehand
- ❌ Ignore audience questions or defer all to end
- ❌ Get defensive about limitations—acknowledge and explain plan

---

**End of Demo Walkthrough Guide**
