# Training Guide
## ISM Sponsor Management System
### Quick-Start for Pilot Users

**Version:** 1.0  
**Date:** March 2026  
**Audience:** Admin, Admissions, Cashier, Sponsor users

---

## Table of Contents

1. [Getting Started](#getting-started)
2. [Role-Specific Quick-Start](#role-specific-quick-start)
3. [Common Navigation](#common-navigation)
4. [School Year Selector](#school-year-selector)
5. [Sponsor Workflows](#sponsor-workflows)
6. [Letter of Guarantee (LoG) Workflows](#letter-of-guarantee-log-workflows)
7. [Coverage Evaluation](#coverage-evaluation)
8. [Request and Approval Workflows](#request-and-approval-workflows)
9. [Audit and Reports](#audit-and-reports)
10. [Common Mistakes and Recovery](#common-mistakes-and-recovery)
11. [Getting Help](#getting-help)

---

## Getting Started

### System Access

**URL:** [Insert staging/production URL]  
**Supported Browsers:** Chrome, Edge, Firefox (latest versions)  
**Mobile:** Limited mobile support; desktop recommended

### Your Account

Your account has been created with a role-based email:
- **Admin:** demo.admin@ism.edu.ph
- **Admissions:** demo.admissions@ism.edu.ph
- **Cashier:** demo.cashier@ism.edu.ph
- **Sponsor:** demo.sponsor@ism.edu.ph

**Initial Password:** Demo@2026!  
**Change Password:** Click your name (top-right) → Profile → Change Password

### First Login

1. Navigate to system URL
2. Click "Login"
3. Enter your email and password
4. Click "Login"
5. You'll be redirected to your role-specific dashboard

---

## Role-Specific Quick-Start

### Admin Quick-Start (5 minutes)

**What you can do:**
- Create, edit, and merge sponsor records
- Activate and deactivate LoGs
- Approve or reject change requests
- Manage users and roles (in Settings)
- View all reports and audit logs
- Monitor system health

**Your typical workflow:**
1. Check Dashboard for pending requests and system health
2. Review and approve/reject pending change requests
3. Address any sync failures or data consistency warnings
4. Generate reports as needed
5. Manage users and roles in Settings

**Key pages:**
- **Dashboard:** `/Dashboard/Index` - Pending work and system health
- **Sponsors:** `/Sponsors/Index` - Sponsor management
- **Logs (LoGs):** `/Portal/Index` - LoG management (switch school year as needed)
- **Change Requests:** Available from Dashboard or `/Sponsors/ReviewRequests`
- **Reports:** `/AdminReports/Index` - Admin oversight reports
- **Settings:** `/Settings/Users`, `/Settings/Roles` - System configuration
- **Operations:** `/Operations/Dashboard` - System health monitoring

---

### Admissions Quick-Start (5 minutes)

**What you can do:**
- Create and edit sponsor records
- Create and edit LoGs
- Submit change requests for sensitive sponsor fields
- View admissions-specific reports
- View audit logs for your actions

**What you cannot do:**
- Approve change requests (Admin only)
- Activate LoGs (Admin only)
- Merge sponsors (Admin only)
- Manage users (Admin only)

**Your typical workflow:**
1. Create new sponsor records when sponsors enroll
2. Create LoGs for new sponsors or school years
3. Add coverage rules to LoGs (what's covered, split, not covered)
4. Submit change requests for updates to existing sponsors
5. Follow up on pending change requests

**Key pages:**
- **Dashboard:** `/Dashboard/Index` - Pending LoG reviews
- **Sponsors:** `/Sponsors/Index` - Search and create sponsors
- **Logs (LoGs):** `/Portal/Index` - LoG management
- **My Requests:** Track submitted change requests
- **Reports:** `/AdmissionsReports/Index` - Admissions tracking reports

---

### Cashier Quick-Start (3 minutes)

**What you can do:**
- View sponsor records (read-only)
- View LoG coverage rules (read-only)
- Generate cashier reconciliation reports
- Evaluate coverage for billing (if API access provided)

**What you cannot do:**
- Create or edit sponsors
- Create or edit LoGs
- Approve requests
- Manage users

**Your typical workflow:**
1. Look up sponsor and LoG information for billing inquiries
2. Generate reconciliation reports for sponsor-covered charges
3. Use coverage evaluation tool to check if a charge is covered
4. Export reports to CSV for further analysis

**Key pages:**
- **Sponsors:** `/Sponsors/Index` - Search sponsors (read-only)
- **Logs (LoGs):** `/Portal/Index` - View LoGs (read-only)
- **Reports:** `/CashierReports/Index` - Cashier reconciliation reports

---

### Sponsor Quick-Start (Future Phase)

**Note:** Sponsor self-service is planned for future phases. Currently, sponsors contact Admissions or Cashier for information.

---

## Common Navigation

### Main Navigation Menu

Your menu shows options based on your role:

**All Users:**
- **Home** - Landing page
- **Dashboard** - Role-specific dashboard
- **Sponsors** - Sponsor list and management

**Admin/Admissions:**
- **Logs (Portal section)** - LoG management
- **Reports** - Role-appropriate reports

**Admin Only:**
- **Settings** - User, Role, School Year, Student, Item management
- **Operations** - System health monitoring

### Top-Right Menu

- **Your Name** - Opens dropdown
  - Profile (change password, preferences)
  - Feedback (submit feedback or report issues)
  - Logout

### School Year Selector

Many pages show a **School Year** dropdown at the top. This controls which school year's data you're viewing.

**Important:** Always check the selected school year before creating or editing records.

- **Current School Year:** 2025-2026 (active)
- **Past School Years:** 2024-2025, 2023-2024 (read-only)
- **Future School Years:** 2026-2027 (planning)

---

## School Year Selector

### What It Controls

The school year selector controls:
- Which LoGs are displayed
- Which students are shown
- Which reports cover which period
- Which dashboard metrics are calculated

The school year selector does NOT control:
- Sponsor list (sponsors exist across school years)
- User list
- System settings

### How to Use

1. Look for the **School Year** dropdown (usually top-left or in filter area)
2. Click the dropdown
3. Select the school year you want to work with
4. The page will refresh showing data for that school year

**Tip:** The system remembers your last selected school year across sessions.

### Common Mistake

✗ **Creating a LoG for the wrong school year**

**How to avoid:** Always double-check the school year selector before clicking "Create New LoG."

**How to recover:** If you create a LoG for the wrong school year, contact your Admin to deactivate it or delete it (before activation).

---

## Sponsor Workflows

### Workflow 1: Search for a Sponsor

**Why:** Find existing sponsor before creating a new one

**Steps:**
1. Navigate to **Sponsors** → Index
2. Use the search box: Enter sponsor name, ID, or contact name
3. Click "Search" or press Enter
4. Review results
5. Click sponsor name or ID to view details

**Tips:**
- Search is case-insensitive: "global tech" finds "Global Tech Corporation"
- Search looks in Sponsor ID, Sponsor Name, and Legal Name
- Use filters: Active/Inactive, Date Range

---

### Workflow 2: Create a New Sponsor

**Role Required:** Admin or Admissions

**Steps:**
1. Navigate to **Sponsors** → Index
2. Click "Create New Sponsor"
3. Fill in required fields (marked with *):
   - **Sponsor ID:*** Unique identifier (e.g., SP-12345)
   - **Sponsor Name:*** Display name (e.g., "Global Tech Corporation")
   - **Legal Name:*** Full legal entity name
   - **Contact Person:*** Primary contact name
   - **Contact Email:*** Valid email address
   - **Contact Phone:*** Phone number
   - **Address Line 1:*** Street address
   - **City:*** City
   - **State/Province:*** State or province
   - **Postal Code:** Postal code
   - **Country:*** Country
   - **Is Active:** Check if sponsor is currently active
4. Click "Create"
5. Confirm success message
6. Review sponsor detail page

**Tips:**
- **Sponsor ID convention:** Use consistent format (e.g., SP-XXXXX or Company abbreviation)
- **Duplicate check:** Search for similar names before creating to avoid duplicates
- **Required fields:** Form will not submit without required fields completed

**Common mistake:** Using informal sponsor name instead of legal name.  
**Fix:** Use full legal entity name in "Legal Name" field; use common name in "Sponsor Name" field.

---

### Workflow 3: Edit a Sponsor

**Role Required:** Admin or Admissions

**Steps:**
1. Search for sponsor (see Workflow 1)
2. Click sponsor name to view details
3. Click "Edit"
4. Modify fields as needed
5. Click "Save Changes"
6. Confirm success message

**Note for Admissions users:**  
Some fields require approval (see Workflow 7: Submit Change Request).

**Tips:**
- **Sponsor ID cannot be changed** (it's the primary key)
- **Changes are audited:** Your name and timestamp are logged

---

### Workflow 4: Deactivate a Sponsor

**Role Required:** Admin

**Steps:**
1. Search for sponsor
2. Click sponsor name to view details
3. Click "Edit"
4. Uncheck "Is Active"
5. Click "Save Changes"
6. Confirm deactivation

**What happens:**
- Sponsor remains in database but hidden from active lists
- Associated students and LoGs are not affected
- Deactivated sponsor can be reactivated later

**When to use:**
- Sponsor contract ends
- Sponsor leaves the school
- Sponsor merged into another record (use Merge instead)

---

## Letter of Guarantee (LoG) Workflows

### What is a LoG?

A **Letter of Guarantee (LoG)** is a document (and database record) that defines:
- Which **school year** it applies to
- Which **sponsor** is providing coverage
- Which **expense items** (fee codes) are covered
- **How much** of each item is covered (Covered = 100% sponsor, Split = X% sponsor + Y% parent, NotCovered = 100% parent)

### LoG Lifecycle

1. **Created** - Admissions creates LoG with coverage rules
2. **Under Review** - Admin reviews LoG for approval
3. **Active** - Admin activates LoG; now in effect for billing
4. **Inactive** - LoG deactivated (end of school year or coverage changes)

---

### Workflow 5: Create a New LoG

**Role Required:** Admin or Admissions

**Steps:**
1. **Select school year** using school year selector (top of page)
2. Navigate to **Logs** (under Portal section)
3. Click "Create New LoG"
4. Fill in LoG header:
   - **LoG ID:*** Unique identifier (e.g., LOG-2025-SP001-01)
   - **Sponsor:** Select from dropdown
   - **School Year:** (pre-filled from school year selector; verify)
   - **Status:** Usually starts as "UnderReview"
5. Click "Create"
6. On LoG detail page, click "Add Coverage Rule"
7. For each covered item:
   - **Item Code:** Select fee code (e.g., TUITION-ES, BUS-SERVICE)
   - **Coverage Type:** Select Covered, Split, or NotCovered
   - **Sponsor %:** Enter sponsor percentage (0-100)
   - **Parent %:** Enter parent percentage (must total 100 with sponsor %)
   - **Is Active:** Check to activate rule
8. Click "Add Rule"
9. Repeat step 6-8 for all covered items
10. Notify Admin that LoG is ready for activation

**Tips:**
- **LoG ID convention:** Use format LOG-{SchoolYear}-{SponsorID}-{Sequence} for consistency
- **Coverage rules:** Add all items. If not listed, item defaults to "NotCovered."
- **Split percentages must total 100:** e.g., Sponsor 70% + Parent 30% = 100%

**Common mistake:** Forgetting to add coverage rules after creating LoG header.  
**Fix:** Always add rules before submitting for activation.

---

### Workflow 6: Activate a LoG

**Role Required:** Admin only

**Steps:**
1. Navigate to **Logs**
2. Filter by **Status:** UnderReview
3. Click LoG ID to view details
4. Review coverage rules for accuracy
5. Click "Activate"
6. Confirm activation
7. Observe success message and status change to "Active"

**What happens:**
- LoG status becomes "Active"
- IsActive flag set to true
- ActivatedOn timestamp recorded
- LoG is now used for coverage evaluation

**When to activate:**
- After Admissions creates LoG and all rules are correct
- After Admin reviews and approves coverage policy

---

### Workflow 7: Edit LoG Coverage Rules

**Role Required:** Admin or Admissions (before activation); Admin only (after activation)

**Steps:**
1. Navigate to LoG detail page
2. Click "Edit Rule" next to the rule you want to modify
3. Update Coverage Type, Sponsor %, or Parent %
4. Click "Save Changes"
5. If rule should be removed: Click "Delete Rule" (if available)

**Alternative: Create a new LoG**

For significant coverage changes, best practice is to:
1. Deactivate old LoG
2. Create new LoG with updated rules
3. Activate new LoG

This maintains clear audit trail of coverage history.

---

## Coverage Evaluation

### What is Coverage Evaluation?

Coverage evaluation determines:
- **Is this charge covered** by the sponsor's LoG?
- **How much** does the sponsor pay?
- **How much** does the parent pay?
- **Who to bill:** Sponsor, Parent, or Both

### How Coverage Evaluation Works

1. System looks up active LoG for sponsor
2. System finds matching coverage rule for fee code
3. System calculates sponsor and parent amounts based on percentages
4. System returns decision:  - **Covered:** Sponsor pays 100%, bill to sponsor
   - **Split:** Sponsor pays X%, parent pays Y%, bill to both
   - **NotCovered:** Parent pays 100%, bill to parent

### When to Use

- **Cashier:** When billing a charge, check coverage before applying charge
- **Admissions:** When discussing coverage with parent or sponsor
- **Admin:** When verifying LoG rules are correct

### Workflow 8: Evaluate Coverage (Manual)

**Role:** Cashier, Admissions, Admin

**Steps:**
1. Navigate to **Coverage Evaluation** page (if available as standalone page)
2. Enter evaluation parameters:
   - **Sponsor ID:** e.g., DEMO-SP001
   - **Student ID:** e.g., DEMO-ST001
   - **Item Code:** e.g., TUITION-ES
   - **Amount:** e.g., 450000
3. Click "Evaluate Coverage"
4. Review result:
   - **Decision:** Covered, Split, or NotCovered
   - **Sponsor Amount:** Amount sponsor will pay
   - **Parent Amount:** Amount parent will pay
   - **Bill To:** Sponsor, Parent, or Both
   - **Rule Applied:** Which LoG rule was used

**Tips:**
- **Evaluation is real-time:** Results in 1-2 seconds
- **Evaluation is read-only:** It doesn't create charges, only shows what would happen
- **Audit trail:** All evaluations are logged for reconciliation

---

## Request and Approval Workflows

### Why Change Requests?

Certain sponsor fields are sensitive (e.g., contact email, legal name) and require Admin approval to change. This ensures data integrity and provides an audit trail.

### Which Fields Require Approval?

- Contact Email
- Contact Phone
- Legal Name
- Address changes (sometimes)
- Other sensitive fields as configured

### Workflow 9: Submit a Change Request (Admissions)

**Role Required:** Admissions (non-Admin users)

**Steps:**
1. Navigate to sponsor detail page
2. Click "Request Change"
3. Select **Field to Change** from dropdown
4. Review **Current Value** (pre-filled)
5. Enter **Proposed Value**
6. Enter **Justification** (explain why change is needed, provide reference)
7. Click "Submit Request"
8. Confirm success message
9. Track request status in **My Requests** or **Dashboard**

**Tips:**
- **Be specific in justification:** Include reference (email date, phone call, sponsor letter)
- **Check status:** Requests show as Pending, Approved, Rejected, or Applied
- **Follow up:** If not approved within 2 business days, follow up with Admin

**Common mistake:** Not providing sufficient justification.  
**Fix:** Include date and source of change request (e.g., "Per sponsor email dated 3/5/2026").

---

### Workflow 10: Review and Approve Change Request (Admin)

**Role Required:** Admin

**Steps:**
1. Navigate to **Dashboard**
2. View "Pending Change Requests" widget
3. Click "View All" or "Review"
4. Select a change request to review
5. Review change details:
   - Sponsor name
   - Field being changed
   - Old value
   - New value
   - Justification
   - Requester and date
6. Enter **Review Comments**
7. Click "Approve" or "Reject"
8. If approved: System automatically applies change to sponsor record
9. Confirm success message

**Tips:**
- **Verify justification:** Check that requester provided source/reference
- **Contact sponsor if unsure:** Call or email sponsor to confirm change
- **Add comments:** Even for approval, add brief comment (e.g., "Confirmed via email 3/5")
- **Reject with explanation:** If rejecting, explain why so requester can resubmit

**Common mistake:** Approving without verifying justification.  
**Fix:** Always check justification field before approving.

---

## Audit and Reports

### Audit Logs

**Who can access:** Admin (full access), Admissions/Cashier (limited access to own actions)

**What's logged:**
- Sponsor creation, edits, deactivation, merge
- LoG creation, edits, activation, deactivation
- Change request submission, approval, rejection, application
- User login, logout (if enabled)
- Coverage evaluations (in separate audit table)

**How to access:**
1. Navigate to **Logs** (audit logs section)
2. View default list (recent entries)
3. Use filters:
   - **Date Range:** Last 7 days, Last 30 days, Custom
   - **Module:** Sponsor, LoG, ChangeRequest, etc.
   - **User:** Filter by specific user (Admin only)
4. Click log entry for details (if available)

**Tips:**
- **Use audit logs for troubleshooting:** "What changed before this broke?"
- **Use audit logs for compliance:** "Who approved this change?"

---

### Reports

**Access:** Role-based

**Admin Reports:**
- **Sponsor Master Report:** Complete sponsor listing with sync status
- **LoG Activity Report:** LoG lifecycle and approval tracking
- **Coverage Decisions Report:** Coverage evaluation outcomes
- **Sync Status Report:** Integration attempts and failures
- **Audit Activity Report:** System activity summary

**Admissions Reports:**
- **Admissions Tracking Report:** Pending work, recent sponsors, recent LoG changes

**Cashier Reports:**
- **Cashier Reconciliation Report:** Recent coverage decisions, student LoG status

### Workflow 11: Generate and Export a Report

**Steps:**
1. Navigate to **Reports** (role-appropriate section)
2. Select report type
3. Enter filter parameters:
   - Date range
   - School year
   - Sponsor (optional)
   - Status (optional)
4. Click "Generate Report"
5. Review report on screen
6. To export: Click "Export to CSV"
7. Open CSV in Excel for further analysis

**Tips:**
- **Date ranges:** Use fiscal year or school year boundaries for consistency
- **CSV export:** All reports support CSV export for Excel analysis
- **Save filters:** Bookmark commonly used report URLs with filters

---

## Common Mistakes and Recovery

### Mistake 1: Created Sponsor with Typo in Sponsor ID

**Symptom:** Sponsor ID has typo (e.g., "DMEO-SP001" instead of "DEMO-SP001")

**Impact:** Sponsor ID cannot be changed after creation

**Recovery:**
- **Option 1 (if no related records):** Admin deletes sponsor and recreates with correct ID
- **Option 2 (if related records exist):** Continue using incorrect ID but add note in comments; create correctly-named sponsor and merge

**Prevention:** Double-check Sponsor ID before clicking "Create"

---

### Mistake 2: Created LoG for Wrong School Year

**Symptom:** LoG created for 2024-2025 when should be 2025-2026

**Impact:** LoG won't appear in current school year LoG list

**Recovery:**
- **If not activated:** Contact Admin to delete or deactivate LoG; recreate for correct school year
- **If activated:** Admin must deactivate incorrect LoG and create new LoG for correct school year

**Prevention:** Always verify school year selector before creating LoG

---

### Mistake 3: Forgot to Add Coverage Rules to LoG

**Symptom:** LoG created but no rules added; all items default to "NotCovered"

**Impact:** Coverage evaluation returns "NotCovered" for all items

**Recovery:**
- Edit LoG and add coverage rules
- If already activated, Admin can edit to add rules

**Prevention:** Always add coverage rules immediately after creating LoG header

---

### Mistake 4: Split Percentages Don't Total 100%

**Symptom:** Entered Sponsor 70%, Parent 20% (totals 90%, not 100%)

**Impact:** Form validation error: "Sponsor and Parent percentages must total 100%"

**Recovery:**
- Correct Parent % to 30% (so 70% + 30% = 100%)
- Form will now submit

**Prevention:** Always check total = 100% for Split coverage type

---

### Mistake 5: Approved Change Request but Sponsor Record Not Updated

**Symptom:** Change request status shows "Approved" but sponsor record still has old value

**Impact:** May indicate system error or change not applied

**Recovery:**
- Check change request status: Should show "Applied" not just "Approved"
- If status is "Approved" but not "Applied," contact Admin or IT support
- Admin can manually update sponsor record

**Prevention:** After approving, verify change request status changes to "Applied"

---

### Mistake 6: Cannot Find Sponsor in List

**Symptom:** Sponsor exists but doesn't appear in search results

**Impact:** May think sponsor doesn't exist and create duplicate

**Possible Causes:**
- Sponsor is inactive (filter showing Active only)
- Searching wrong spelling
- Sponsor merged into another record

**Recovery:**
- Clear filters, try "Show All" or "Show Inactive"
- Search by Sponsor ID instead of name
- Check with Admin if sponsor was merged

**Prevention:** Use broad search terms; check filter settings

---

## Getting Help

### During Pilot

**For Technical Issues:**
- Click **Feedback** (top-right menu)
- Select Category: "IntegrationReliability" or "Other"
- Select Severity: Critical (system down), High (blocking work), Medium (inconvenient), Low (minor issue)
- Describe issue with steps to reproduce
- Submit feedback

**For Training Questions:**
- Contact: [Training lead name and email]
- Office hours: [Schedule]

**For Urgent Issues:**
- Contact: [IT support name and email]
- Phone: [Support phone]

### After Pilot

[Update with production support model]

---

### Training Resources

- **This Guide:** General training for all users
- **UAT Guide:** Detailed test scenarios (for testers)
- **Demo Guide:** Presenter scripts (for trainers)
- **Handoff Documentation:** Technical details (for IT staff)

**Online Resources:**
- System Help (if implemented): Click "?" icon in top-right
- Video tutorials (if available): [Link]

---

### Feedback and Continuous Improvement

We welcome your feedback! The system includes a built-in feedback form:

1. Click **Feedback** in top-right menu
2. Select **Category** and **Severity**
3. Choose affected **Module**
4. Enter **Title** and **Description**
5. Click "Submit Feedback"

Your feedback helps us improve the system for everyone.

---

**End of Training Guide**

---

**Quick Reference Card** (Print and Keep Handy)

| Task | Role | Quick Steps |
|------|------|-------------|
| Create Sponsor | Admin, Admissions | Sponsors → Create → Fill form → Create |
| Edit Sponsor | Admin, Admissions | Sponsors → Search → Edit → Save |
| Request Change | Admissions | Sponsor detail → Request Change → Fill → Submit |
| Approve Change | Admin | Dashboard → Review Requests → Approve/Reject |
| Create LoG | Admin, Admissions | Select school year → Logs → Create → Add rules |
| Activate LoG | Admin | Logs → UnderReview → View → Activate |
| Evaluate Coverage | All | Coverage page → Enter params → Evaluate |
| Generate Report | All | Reports → Select type → Filters → Generate → Export CSV |
| View Audit Log | Admin | Logs (Audit) → Filters → View |
| Submit Feedback | All | Top-right menu → Feedback → Fill form → Submit |

**Remember:** Always check the **school year selector** before creating or editing records!

---

**End of Quick Reference Card**
