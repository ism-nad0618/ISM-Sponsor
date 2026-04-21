# User Acceptance Testing (UAT) Guide
## ISM Sponsor Management System

**Version:** 1.0  
**Date:** March 2026  
**Status:** Ready for Testing

---

## Table of Contents

1. [UAT Overview](#uat-overview)
2. [Test Participants](#test-participants)
3. [Test Environment](#test-environment)
4. [Test Scenarios (UT01-UT10)](#test-scenarios)
5. [Test Execution Instructions](#test-execution-instructions)
6. [Defect Logging](#defect-logging)
7. [Pass/Fail Criteria](#passfail-criteria)
8. [Sign-Off Process](#sign-off-process)

---

## UAT Overview

### Purpose
User Acceptance Testing validates that the ISM Sponsor Management System meets business requirements and is ready for pilot deployment. Testing covers sponsor management, Letters of Guarantee (LoG) workflows, coverage evaluation, approval workflows, duplicate detection/merge, sync visibility, and audit retrieval.

### Scope
- **In Scope:** All core sponsor and LoG workflows, RBAC enforcement, coverage evaluation API, change request workflows, duplicate merge, reporting, operational monitoring
- **Out of Scope:** Full production hardening, predictive analytics, advanced monitoring/DR/HA, large UI redesigns of external systems (PowerSchool, NetSuite, OBS)

### Testing Approach
- Role-based testing across Admin, Admissions, Cashier, and Sponsor personas
- Task-oriented scenarios matching real operational workflows
- Staging environment close to production configuration
- De-identified demo data for realistic testing

### Success Criteria
- All critical scenarios (UT01-UT10) pass without blocking errors
- Task completion rate meets or exceeds 90%
- Real-time response for coverage evaluation API
- No security violations or unauthorized access
- System ready for pilot deployment

---

## Test Participants

### Participant Roles

| Role | Responsibilities | Test Scenarios |
|------|------------------|----------------|
| **Admin** | Full system access, user management, approval authority, merge authority | UT01, UT02, UT03, UT04, UT05, UT06, UT07, UT08, UT09, UT10 |
| **Admissions** | Sponsor creation, LoG management, change request submission | UT01, UT02, UT03, UT04, UT05, UT07, UT09, UT10 |
| **Cashier** | Read-only access to sponsors, LoGs, reconciliation reports | UT01, UT02, UT09, UT10 |
| **Sponsor** | View own sponsor profile and students (future phase) | UT01, UT02 |

### Demo Accounts

| Email | Password | Role | Purpose |
|-------|----------|------|---------|
| demo.admin@ism.edu.ph | Demo@2026! | Admin | Full system testing |
| demo.admissions@ism.edu.ph | Demo@2026! | Admissions | Admissions workflow testing |
| demo.cashier@ism.edu.ph | Demo@2026! | Cashier | Cashier workflow testing |
| demo.sponsor@ism.edu.ph | Demo@2026! | Sponsor | Sponsor self-service testing |

---

## Test Environment

### Environment Details
- **URL:** [Staging environment URL]
- **Database:** SQL Server staging database with demo data
- **External Systems:** Mock/stub integrations for PowerSchool, NetSuite
- **School Year:** 2025-2026 (active)

### Prerequisites
1. Demo data seeded successfully
2. All users have valid accounts
3. Browser: Chrome, Edge, or Firefox (latest versions)
4. Screen resolution: 1280x720 or higher

### Test Data Available
- 6 demo sponsors (DEMO-SP001 through DEMO-SP006)
- 8 demo students
- 5 demo LoGs with various statuses
- 4 demo change requests (pending, approved, rejected, applied)
- Demo sync logs and audit records

---

## Test Scenarios

### UT01: Login and Baseline Access

**Objective:** Verify all users can log in and access appropriate landing pages

**Actor:** Admin, Admissions, Cashier, Sponsor

**Setup:**
- Ensure all demo accounts are active
- Clear browser cache

**Test Steps:**

1. Navigate to application URL
2. Click "Login" or navigate to `/Account/Login`
3. Enter demo account credentials (see Demo Accounts table)
4. Click "Login"
5. Observe landing page

**Expected Results:**
- ✅ User successfully authenticates
- ✅ User redirected to appropriate landing page based on role:
  - Admin: Dashboard with pending requests, system health
  - Admissions: Dashboard with pending LoG reviews
  - Cashier: Dashboard with reconciliation summary
  - Sponsor: Dashboard with own sponsor profile
- ✅ Navigation menu shows only role-appropriate options
- ✅ No error messages displayed

**Evidence Notes:**
_[Tester: Record screenshot of successful login and landing page]_

---

### UT02: RBAC Enforcement

**Objective:** Verify role-based access control prevents unauthorized actions

**Actor:** Admissions, Cashier

**Setup:**
- Log in as demo.admissions@ism.edu.ph

**Test Steps:**

1. Attempt to navigate to `/Settings/Users` (Admin-only page)
2. Observe response
3. Attempt to navigate to `/Sponsors/Merge` (Admin-only action)
4. Log out
5. Log in as demo.cashier@ism.edu.ph
6. Attempt to create a new sponsor at `/Sponsors/Create`
7. Attempt to activate a LoG
8. Observe responses

**Expected Results:**
- ✅ Admissions user receives "Access Denied" or 403 Forbidden for Admin-only pages
- ✅ Admissions user cannot see merge buttons or admin navigation items
- ✅ Cashier user receives "Access Denied" for sponsor creation
- ✅ Cashier user cannot see activation controls
- ✅ Unauthorized actions are blocked at controller level
- ✅ User remains logged in after access denial

**Evidence Notes:**
_[Tester: Record access denied messages and URLs attempted]_

---

### UT03: Create Sponsor Record

**Objective:** Verify sponsor creation workflow with complete data entry

**Actor:** Admin or Admissions

**Setup:**
- Log in as demo.admin@ism.edu.ph or demo.admissions@ism.edu.ph
- Navigate to Sponsors list

**Test Steps:**

1. Click "Create New Sponsor"
2. Enter sponsor details:
   - **Sponsor ID:** TEST-SP100
   - **Sponsor Name:** Test Corporation Ltd
   - **Legal Name:** Test Corporation Limited Philippines
   - **Contact Person:** Jane Smith
   - **Contact Email:** jane.smith@testcorp.com
   - **Contact Phone:** +63 2 8555 1234
   - **Address Line 1:** 10th Floor Test Building
   - **City:** Makati
   - **State/Province:** Metro Manila
   - **Postal Code:** 1200
   - **Country:** Philippines
   - **Is Active:** Checked
3. Click "Create"
4. Observe confirmation message
5. Navigate to Sponsors list
6. Search for "TEST-SP100"

**Expected Results:**
- ✅ Form accepts all valid inputs
- ✅ Success message displayed: "Sponsor created successfully"
- ✅ System redirects to sponsor list or detail page
- ✅ New sponsor appears in list with correct details
- ✅ Created timestamp recorded
- ✅ Audit log entry created for sponsor creation

**Evidence Notes:**
_[Tester: Record Sponsor ID and timestamp of creation]_

---

### UT04: Validation and Error Messaging

**Objective:** Verify input validation and user-friendly error messages

**Actor:** Admin or Admissions

**Setup:**
- Log in as demo.admin@ism.edu.ph or demo.admissions@ism.edu.ph
- Navigate to "Create New Sponsor"

**Test Steps:**

1. Leave **Sponsor ID** blank
2. Enter **Sponsor Name:** "Short"
3. Enter **Contact Email:** "invalid-email"
4. Leave **Contact Phone** blank
5. Leave **Address Line 1** blank
6. Click "Create"
7. Observe validation messages
8. Correct **Sponsor ID:** TEST-SP101
9. Correct **Contact Email:** test@valid.com
10. Fill in required fields
11. Click "Create" again

**Expected Results:**
- ✅ Form does not submit with validation errors
- ✅ Clear, specific error messages displayed for each field:
  - "Sponsor ID is required"
  - "Contact Email is not a valid email address"
  - Required field messages for all mandatory fields
- ✅ Error messages appear near the relevant fields
- ✅ User can correct errors without losing other entered data
- ✅ After correction, form submits successfully
- ✅ No technical error messages exposed to user

**Evidence Notes:**
_[Tester: Record validation messages and note clarity]_

---

### UT05: Update Sponsor Record

**Objective:** Verify sponsor editing workflow and change tracking

**Actor:** Admin or Admissions

**Setup:**
- Log in as demo.admin@ism.edu.ph or demo.admissions@ism.edu.ph
- Navigate to Sponsors list
- Select "DEMO-SP001 - Global Tech Corporation"

**Test Steps:**

1. Click "Edit" on sponsor detail page
2. Update **Contact Phone:** +63 2 8123 9999
3. Update **Address Line 2:** Updated Wing B
4. Click "Save Changes"
5. Observe confirmation message
6. Review sponsor detail page
7. Navigate to Audit Logs (if accessible)
8. Search for DEMO-SP001 update audit entry

**Expected Results:**
- ✅ Edit form loads with existing values pre-populated
- ✅ User can modify editable fields
- ✅ Success message displayed: "Sponsor updated successfully"
- ✅ Updated values displayed on detail page
- ✅ Modified timestamp updated
- ✅ Audit log records the update with user, timestamp, and fields changed
- ✅ Non-editable fields (e.g., Sponsor ID) are protected

**Evidence Notes:**
_[Tester: Record timestamp of update and audit log ID if visible]_

---

### UT06: Duplicate Detection and Controlled Merge

**Objective:** Verify duplicate sponsor detection and merge workflow

**Actor:** Admin

**Setup:**
- Log in as demo.admin@ism.edu.ph
- Ensure DEMO-SP001 and DEMO-SP006 exist (similar names)

**Test Steps:**

1. Navigate to `/Sponsors/DetectDuplicates` or duplicate detection page
2. Click "Run Duplicate Detection"
3. Review suggested duplicate pairs
4. Identify pair: DEMO-SP001 (Global Tech Corporation) and DEMO-SP006 (Global Tech Corp)
5. Click "Review" or "Merge"
6. Select **Primary Record:** DEMO-SP001
7. Select **Record to Merge:** DEMO-SP006
8. Review merge preview showing combined students, LoGs, change requests
9. Confirm merge
10. Navigate to Sponsors list
11. Search for DEMO-SP006

**Expected Results:**
- ✅ Duplicate detection identifies similar sponsor names
- ✅ Merge interface shows side-by-side comparison
- ✅ User can select primary record
- ✅ Merge preview shows all related entities (students, LoGs, requests)
- ✅ After merge:
  - DEMO-SP006 marked as merged (IsMerged = true)
  - DEMO-SP006 students reassigned to DEMO-SP001
  - DEMO-SP006 LoGs reassigned to DEMO-SP001
  - No data loss
- ✅ Audit log records merge action
- ✅ Merged record no longer appears in active sponsor  list

**Evidence Notes:**
_[Tester: Record primary record ID and merged record ID]_

---

### UT07: Submit Sponsor Profile Change Request

**Objective:** Verify change request submission workflow for non-admin users

**Actor:** Admissions

**Setup:**
- Log in as demo.admissions@ism.edu.ph
- Navigate to sponsor "DEMO-SP002 - Asian Development Bank"

**Test Steps:**

1. Click "Request Change" on sponsor detail page
2. Select **Field to Change:** "Contact Email"
3. Review **Current Value:** j.lee@adb.org
4. Enter **Proposed Value:** john.lee@adb.org
5. Enter **Justification:** "Updated to full first name per sponsor request"
6. Click "Submit Request"
7. Observe confirmation message
8. Navigate to Dashboard or My Requests
9. Verify request appears as "Pending"

**Expected Results:**
- ✅ Change request form loads with fields pre-populated
- ✅ User can select field to change from dropdown
- ✅ Current value displayed for reference
- ✅ Justification field requires meaningful text
- ✅ Success message displayed: "Change request submitted successfully"
- ✅ Request appears in pending requests list
- ✅ Request shows: Sponsor, Field, Old/New Value, Justification, Status, Timestamp
- ✅ Audit log records request submission

**Evidence Notes:**
_[Tester: Record Change Request ID and timestamp]_

---

### UT08: Apply Sponsor Profile Change Request

**Objective:** Verify Admin approval and application of change requests

**Actor:** Admin

**Setup:**
- Log in as demo.admin@ism.edu.ph
- Ensure at least one pending change request exists (from UT07)

**Test Steps:**

1. Navigate to Dashboard or `/Sponsors/ReviewRequests`
2. View pending change requests
3. Click "Review" on the request from UT07
4. Review change details:
   - Sponsor name
   - Field being changed
   - Old value
   - New value
   - Justification
   - Requester
5. Enter **Review Comments:** "Approved. Email confirmed with sponsor liaison."
6. Click "Approve"
7. Observe confirmation message
8. Navigate back to sponsor detail page (DEMO-SP002)
9. Verify contact email updated to "john.lee@adb.org"
10. Check change request status shows "Applied"

**Expected Results:**
- ✅ Admin sees all pending requests
- ✅ Review interface shows complete change context
- ✅ Admin can approve or reject with comments
- ✅ Upon approval:
  - Success message displayed
  - Sponsor record updated automatically
  - Change request status updated to "Applied"
  - ReviewedOn and AppliedOn timestamps recorded
  - ReviewedByUserId captured
- ✅ Audit log records approval and application
- ✅ Original requester can see approval (if notifications implemented)

**Evidence Notes:**
_[Tester: Record approval timestamp and verify sponsor record updated]_

---

### UT09: LoG List, Coverage Access, and Activation Controls

**Objective:** Verify LoG workflows including list, coverage view, and activation

**Actor:** Admin or Admissions

**Setup:**
- Log in as demo.admin@ism.edu.ph or demo.admissions@ism.edu.ph
- Navigate to LoG list page

**Test Steps:**

1. View LoG list
2. Apply filter: **School Year:** 2025-2026, **Status:** Active
3. Observe filtered results
4. Select "DEMO-LOG001" for DEMO-SP001
5. View LoG detail page
6. Review coverage rules table
7. Navigate to inactive LoG "DEMO-LOG004" (status: UnderReview)
8. Click "Activate" (Admin only)
9. Confirm activation
10. Observe success message
11. Verify LoG status changed to "Active"
12. Check IsActive flag set to true

**Expected Results:**
- ✅ LoG list displays sponsor, school year, status, created date
- ✅ Filters work correctly (school year, sponsor, status)
- ✅ Clicking LoG ID navigates to detail page
- ✅ Detail page shows:
  - LoG header info (ID, Sponsor, School Year, Status)
  - Coverage rules table (Item, Coverage Type, Sponsor %, Parent %)
  - Activation controls (if Admin and status allows)
- ✅ Activation requires Admin role
- ✅ After activation:
  - Success message: "Letter of Guarantee activated successfully"
  - Status updated to "Active"
  - IsActive = true
  - ActivatedOn timestamp recorded
- ✅ Audit log records activation

**Evidence Notes:**
_[Tester: Record LoG ID activated and timestamp]_

---

### UT10: Audit Retrieval and Standardized Failure Handling

**Objective:** Verify audit log access and error handling consistency

**Actor:** Admin

**Setup:**
- Log in as demo.admin@ism.edu.ph
- Navigate to Logs page

**Test Steps:**

1. Navigate to `/Logs` or Audit Logs page
2. View default audit log list (recent entries)
3. Apply filter: **Date Range:** Last 7 days
4. Apply filter: **Module:** "Sponsor"
5. Observe filtered results
6. Review log entry details:
   - Activity Date
   - Module (Item)
   - Details
   - User
   - Role
   - School Year
7. Test pagination (if more than 50 entries)
8. Attempt to access audit logs as Cashier role (log in as demo.cashier@ism.edu.ph)
9. Observe access control
10. Test error handling: Navigate to invalid URL like `/Sponsors/InvalidAction`
11. Observe error page

**Expected Results:**
- ✅ Admin can access audit logs
- ✅ Logs display in reverse chronological order
- ✅ Filters work correctly  (date range, module, user)
- ✅ Each log entry shows:
  - Date/Time
  - Module/Item
  - Action/Details
  - User who performed action
  - Role
  - School Year context
- ✅ Pagination works for large result sets
- ✅ Cashier role has limited or no access to full audit logs (based on design)
- ✅ Error handling:
  - Invalid URLs show user-friendly 404 error page
  - System errors show generic error message (no stack traces)
  - User can navigate back to valid pages
  - Errors logged but not exposed to user

**Evidence Notes:**
_[Tester: Record audit log entry IDs reviewed and error scenarios tested]_

---

## Test Execution Instructions

### Before Testing
1. Review all test scenarios
2. Ensure demo environment is accessible
3. Verify demo data is seeded
4. Clear browser cache and cookies
5. Have defect logging template ready

### During Testing
1. Execute scenarios in order (UT01-UT10)
2. Log in with appropriate role for each scenario
3. Follow test steps exactly as written
4. Record actual results vs. expected results
5. Take screenshots for evidence
6. Log any defects immediately using defect template
7. Do not skip steps or deviate from script

### After Testing
1. Complete test execution record for each scenario
2. Summarize pass/fail counts
3. Prioritize defects by severity
4. Provide feedback on usability and clarity
5. Submit test results to test lead or project manager

---

## Defect Logging

### When to Log a Defect
- Expected result does not match actual result
- System error or unexpected behavior
- Unclear or confusing error messages
- Performance issue (page takes >5 seconds to load)
- UI rendering issue
- Data inconsistency

### Defect Severity Levels

| Severity | Definition | Example |
|----------|------------|---------|
| **Critical** | Blocking issue preventing test completion or major security flaw | Cannot log in, data corruption, unauthorized access allowed |
| **High** | Major functionality broken but workaround exists | Cannot create sponsor, approval workflow fails |
| **Medium** | Non-critical feature broken or usability issue | Filter doesn't work, validation message unclear |
| **Low** | Cosmetic issue or minor inconvenience | Typo, minor alignment issue, missing help text |

### Defect Template
See [DEFECT_TEMPLATE.md](DEFECT_TEMPLATE.md) for full template.

**Required Fields:**
- Defect ID (auto-generated)
- Test Case ID (e.g., UT03)
- Summary (one-line description)
- Severity (Critical, High, Medium, Low)
- Steps to Reproduce
- Expected Result
- Actual Result
- Evidence (screenshot, log excerpt)
- Environment details
- Reported by
- Date/Time

---

## Pass/Fail Criteria

### Individual Test Case
- **Pass:** All expected results achieved without defects or only Low severity defects
- **Fail:** Any Critical or High severity defect encountered

### Overall UAT
- **Pass Criteria:**
  - All 10 scenarios pass (or pass with Low severity defects only)
  - No Critical or High severity defects remain unresolved
  - Task completion rate ≥ 90%
  - Coverage evaluation API responds in real-time (<2 seconds)
  - No unauthorized access violations
  - Usability feedback is positive or constructive (not blocking)

- **Fail Criteria:**
  - Any Critical defects unresolved
  - More than 2 High severity defects across all scenarios
  - Task completion rate < 80%
  - Major usability concerns raised by multiple testers

---

## Sign-Off Process

### Test Execution Sign-Off

**Completed by:** [Tester Name]  
**Role:** [Admin / Admissions / Cashier / Sponsor]  
**Date:** [Date]

| Test Case | Pass/Fail | Defects Logged | Notes |
|-----------|-----------|----------------|-------|
| UT01 | ☐ Pass ☐ Fail | | |
| UT02 | ☐ Pass ☐ Fail | | |
| UT03 | ☐ Pass ☐ Fail | | |
| UT04 | ☐ Pass ☐ Fail | | |
| UT05 | ☐ Pass ☐ Fail | | |
| UT06 | ☐ Pass ☐ Fail | | |
| UT07 | ☐ Pass ☐ Fail | | |
| UT08 | ☐ Pass ☐ Fail | | |
| UT09 | ☐ Pass ☐ Fail | | |
| UT10 | ☐ Pass ☐ Fail | | |

**Overall UAT Result:** ☐ Pass ☐ Fail (with retest required) ☐ Fail (major issues)

**Tester Signature:** ___________________________  
**Date:** ___________________________

---

### UAT Approval Sign-Off

**Project Stakeholders**

| Stakeholder | Role | Sign-Off | Date |
|-------------|------|----------|------|
| [Name] | Business Owner | ☐ Approved | |
| [Name] | IT Manager | ☐ Approved | |
| [Name] | Test Lead | ☐ Approved | |
| [Name] | Admissions Director | ☐ Approved | |
| [Name] | Finance Director | ☐ Approved | |

**Conditions for Approval:**
- All test scenarios pass or have acceptable workarounds documented
- Defects are prioritized and resolution plan agreed
- System ready for pilot deployment
- Training materials reviewed and approved

**Final Approval:** ☐ Approved for Pilot  ☐ Not Approved (issues must be resolved)

**Approved by:** ___________________________  
**Title:** ___________________________  
**Date:** ___________________________

---

## Support and Questions

For UAT support, contact:
- **Test Lead:** [Name, Email]
- **Technical Support:** [Name, Email]
- **Business Analyst:** [Name, Email]

For defect reporting, use: [Defect tracking system or email]

---

**End of UAT Guide**
