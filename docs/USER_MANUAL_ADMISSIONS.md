# Admissions Staff User Manual
## ISM Sponsor Management System

**Version:** 1.0  
**Last Updated:** April 23, 2026  
**Role:** Admissions Staff  
**Access Level:** Create & Edit Sponsors, Create & Edit LoGs, Submit Change Requests

---

## Table of Contents

1. [Role Overview](#role-overview)
2. [Getting Started](#getting-started)
3. [Dashboard Guide](#dashboard-guide)
4. [Sponsor Management](#sponsor-management)
5. [Letter of Guarantee (LoG) Management](#letter-of-guarantee-log-management)
6. [Change Requests](#change-requests)
7. [Reports](#reports)
8. [Common Workflows](#common-workflows)
9. [Troubleshooting](#troubleshooting)
10. [FAQs](#faqs)

---

## Role Overview

### Admissions Responsibilities

As an **Admissions Staff** member, you play a crucial

 role in managing sponsor relationships and student coverage. Your primary responsibilities include:

**What You Can Do:**
- ✅ View all sponsor records
- ✅ Create new sponsor records
- ✅ Edit sponsor contact information directly
- ✅ Submit change requests for sensitive sponsor fields
- ✅ Create Letters of Guarantee (LoGs)
- ✅ Edit draft LoGs (before activation)
- ✅ View all LoGs across school years
- ✅ Generate Admissions reports
- ✅ View audit logs for your actions
- ✅ Track status of your change requests

**What You Cannot Do:**
- ❌ Activate or deactivate LoGs (Administrator only)
- ❌ Approve change requests (Administrator only)
- ❌ Merge duplicate sponsors (Administrator only)
- ❌ Delete sponsors (Administrator only)
- ❌ Manage users or roles (Administrator only)
- ❌ Edit sensitive sponsor fields directly (must submit change request)
- ❌ Access system operations dashboard (Administrator only)

**Your Typical Daily Workflow:**
1. Check Dashboard for pending LoG reviews
2. Create new sponsor records when sponsors register
3. Create LoGs for new students or school years
4. Add coverage rules to LoGs
5. Submit change requests for sponsor updates
6. Follow up on pending change requests
7. Generate reports as needed

---

## Getting Started

### Initial Login

![Login Page](screenshots/admissions/admissions_01_login_page.png)  
*Figure 1.1: System login page for Admissions staff*

1. **Navigate to System URL:**
   ```
   Production: https://ismsponsor.azurewebsites.net
   ```

2. **Enter Your Credentials:**
   - Email: your.email@ism.edu.ph
   - Password: [Your password]

3. **Security:**
   - Change default password on first login
   - Use strong password (8+ characters, mixed case, numbers, symbols)
   - Keep credentials confidential

4. **Dashboard:**
   - Upon login, you'll see the **Admissions Dashboard**
   - Shows your pending work and important metrics

### Navigation Overview

**Main Navigation Bar:**
```
[Home] [Dashboard] [Sponsors] [Portal] [Reports]
```

**Menu Sections:**
- **Home** → System landing page
- **Dashboard** → Admissions dashboard with your metrics
- **Sponsors** → Sponsor search and management
- **Portal** → Letter of Guarantee management
- **Reports** → Admissions-specific reports

**User Menu (Top-Right):**
```
[Your Name ▼]
  ├─ Profile (change password)
  ├─ Feedback (report issues)
  └─ Logout
```

### Key Concepts

**Sponsors:**
- Organizations or individuals who provide financial support
- Each sponsor has unique Sponsor ID
- Can support multiple students
- Must be active to create LoGs

**Letters of Guarantee (LoGs):**
- Formal documents defining coverage for sponsored students
- One LoG per student per sponsor per school year
- Status: Draft → Active → Past (expired)
- Contains coverage rules (what's covered, what's not)

**Change Requests:**
- Required for editing sensitive sponsor fields
- Must be submitted for Administrator approval
- Tracks justification and supporting documents
- Status: Pending → Approved/Rejected

**School Years:**
- Academic years (e.g., 2025-2026)
- Controls which students and LoGs you see
- Always verify correct school year selected

---

## Dashboard Guide

### Admissions Dashboard

**URL:** `/Dashboard/AdmissionsDashboard` or `/Dashboard`

Your dashboard provides an overview of your work and pending tasks.

### Dashboard Sections

#### 1. Your Statistics

```
┌─────────────────┬─────────────────┬─────────────────┐
│ Active LoGs     │ Pending Requests│ Drafts          │
│     42          │       3         │      5          │
│ ↗ +5 this week  │ ⏱ Awaiting review│ 📝 Need attention│
└─────────────────┴─────────────────┴─────────────────┘
```

**Metrics Explained:**

1. **Active LoGs:** 
   - LoGs you've created that are currently active
   - Shows your contribution this school year
   - Click to view your active LoGs

2. **Pending Requests:**
   - Change requests awaiting Administrator approval
   - Your submitted requests needing review
   - Click to track request status

3. **Draft LoGs:**
   - LoGs created but not yet activated
   - Need completion or Administrator activation
   - Click to complete drafts

#### 2. Recent Activity

Shows your last 10 actions:
```
📝 April 23, 2026 10:30 AM
   Created LoG LOG-2526-15 for student Doe, Jane
   
📄 April 23, 2026 9:45 AM
   Submitted change request CR-005 for SP-12345
   
✏️ April 22, 2026 3:15 PM
   Created sponsor SP-12399 (New Tech Solutions)
```

#### 3. Quick Actions

**One-Click Actions:**
- 📝 Create New Sponsor
- 📄 Create New LoG
- 📊 My Change Requests
- 📈 Generate Report
- 🔍 Search Sponsors

---

## Sponsor Management

### Workflow 1: Search for a Sponsor

**Why:** Always search before creating to avoid duplicates

![Sponsors List with Search](screenshots/admissions/sponsors/admissions_04_sponsors_list.png)  
*Figure 3.1: Sponsors list view with search functionality*

**Steps:**

1. **Navigate to Sponsors:**
   - Dashboard → Sponsors
   - Or: Main Menu → Sponsors

2. **Use Search Box:**
   ```
   🔍 [Search sponsors by name, ID, or contact...] [Search]
   ```

3. **Search Tips:**
   - Search is case-insensitive
   - Partial matches work: "tech" finds "Global Tech"
   - Search by: Sponsor ID, Name, Legal Name, Contact Email
   
4. **Review Results:**
   ```
   ┌────────────┬──────────────────┬─────────────────┬──────────┐
   │ Sponsor ID │ Sponsor Name     │ Contact Person  │ Status   │
   ├────────────┼──────────────────┼─────────────────┼──────────┤
   │ SP-12345   │ Global Tech Corp │ John Smith      │ 🟢 Active│
   │ SP-12350   │ Global Technology│ Jane Doe        │ 🟢 Active│
   └────────────┴──────────────────┴─────────────────┴──────────┘
   ```

5. **Check for Duplicates:**
   - Similar names may be duplicates
   - Check contact information
   - Verify different entities before creating new

**Common Scenarios:**

**Scenario 1: Exact Match Found**
```
Search: "Global Tech Corporation"
Result: SP-12345 | Global Tech Corporation
Action: Use existing record, don't create new
```

**Scenario 2: Similar Name Found**
```
Search: "Global Tech"
Results: 
- SP-12345 | Global Tech Corporation
- SP-12350 | Global Technology Solutions
Action: Verify contacts - if different entities, create new
```

**Scenario 3: No Match Found**
```
Search: "New Sponsor Company"
Result: No sponsors found
Action: Proceed to create new sponsor
```

### Workflow 2: Create New Sponsor

**When:** New organization wants to sponsor students

![Create Sponsor Form](screenshots/admissions/sponsors/admissions_06_create_sponsor_empty.png)  
*Figure 3.2: Create new sponsor form showing required fields*

**Prerequisites:**
- Sponsor doesn't exist in system (checked via search)
- You have sponsor's complete information
- Sponsor contact information verified

**Step-by-Step:**

1. **Navigate to Create:**
   - Sponsors → "Create New Sponsor"

2. **Fill Basic Information:**
   ```
   Sponsor ID*:        [SP-12400]
                       ↑ Use your institution's convention
                       Example: SP-XXXXX or COMPANYNAME-###
   
   Sponsor Name*:      [ABC Corporation]
                       ↑ Display name (short form)
   
   Legal Name*:        [ABC Corporation Pty Ltd]
                       ↑ Full legal entity name
   ```

   **Tips:**
   - Sponsor ID must be unique (system validates)
   - Use consistent format (e.g., SP-XXXXX)
   - Legal Name is for official documents
   - Sponsor Name is for UI display

3. **Fill Contact Information:**
   ```
   Contact Person*:    [Maria Santos]
                       ↑ Primary contact name
   
   Contact Email*:     [maria.santos@abc.com]
                       ↑ Valid email address
   
   Contact Phone*:     [+63 2 8765 4321]
                       ↑ Include country code
   
   Alternative Email:  [info@abc.com]  (Optional)
   Alternative Phone:  [+63 917 234 5678]  (Optional)
   ```

   **Tips:**
   - Use professional email (not personal)
   - Verify email before saving
   - Phone: Include country code (+63 for Philippines)
   - Alternative contacts useful for backup

4. **Fill Address Information:**
   ```
   Address Line 1*:    [456 Ortigas Avenue]
                       ↑ Street address
   
   Address Line 2:     [Ortigas Center]  (Optional)
                       ↑ Building/Unit
   
   City*:              [Pasig City]
   
   State/Province*:    [Metro Manila]
   
   Postal Code:        [1605]  (Optional but recommended)
   
   Country*:           [Philippines ▼]
                       ↑ Select from dropdown
   ```

   **Tips:**
   - Use complete, accurate address
   - Include building name if applicable
   - Postal code helps with sorting
   - Country dropdown standardizes names

5. **Set Status:**
   ```
   ☑ Is Active
   ```
   - **Check this box** for new active sponsors
   - Inactive sponsors don't appear in LoG creation

6. **Review & Save:**
   - Double-check all information
   - Verify email addresses
   - Confirm Sponsor ID is correct (cannot change later)
   - Click **"Create"** button

7. **Confirmation:**
   ```
   ✅ Sponsor Created Successfully!
   
   Sponsor ID: SP-12400
   Sponsor Name: ABC Corporation
   
   [View Sponsor Details] [Create LoG for This Sponsor] [Back to List]
   ```

**Common Mistakes to Avoid:**

❌ **Duplicate Sponsor:**
- Always search first
- Check for similar names
- Verify with Administrator if unsure

❌ **Incorrect Sponsor ID:**
- Cannot be changed after creation
- Use consistent format
- Check for typos before saving

❌ **Informal Names:**
- Don't use: "ABC Corp (John's company)"
- Use formal names only
- Add notes in separate field if needed

❌ **Incomplete Information:**
- All required fields (*) must be complete
- Form won't submit without required fields
- Save partial data as draft if available

**Best Practices:**

✅ **Naming Convention:**
```
Good Examples:
- Sponsor ID: SP-12400, SP-12401, SP-12402
- Sponsor Name: ABC Corporation
- Legal Name: ABC Corporation Pty Ltd

Bad Examples:
- Sponsor ID: ABC, Company1, Sponsor-X
- Sponsor Name: ABC Corp (John)
- Legal Name: ABC
```

✅ **Contact Information:**
- Use primary contact for official communications
- Verify email before saving
- Update contacts when they change
- Keep alternative contacts current

✅ **Address Formatting:**
```
Good Example:
Address Line 1: 456 Ortigas Avenue
Address Line 2: Ortigas Center, 15th Floor
City: Pasig City
State: Metro Manila
Postal Code: 1605
Country: Philippines

Bad Example:
Address Line 1: ortigas
City: pasig
(incomplete, improper capitalization)
```

### Workflow 3: Edit Sponsor (Direct Edit)

**What You Can Edit Directly:**
- ✅ Contact Person
- ✅ Contact Email
- ✅ Contact Phone
- ✅ Alternative Email
- ✅ Alternative Phone
- ✅ Address fields (all)

**What Requires Change Request:**
- ⚠️ Sponsor ID (rarely changed)
- ⚠️ Legal Name (official name change)
- ⚠️ Sponsor Name (if significantly different)

**Step-by-Step (Direct Edit):**

1. **Find Sponsor:**
   - Search for sponsor (see Workflow 1)
   - Click sponsor name to view details

2. **Click "Edit" Button**

3. **Edit Form:**
   ```
   Edit Sponsor: SP-12345 | Global Tech Corporation
   
   Sponsor ID:         [SP-12345]  ← Read-only
   Sponsor Name:       [Global Tech Corporation]  ← Submit request if changing
   Legal Name:         [Global Tech Corp Pty Ltd]  ← Submit request if changing
   
   Contact Person:     [John Smith]  ← Can edit directly
   Contact Email:      [john.smith@globaltech.com]  ← Can edit directly
   Contact Phone:      [+63 2 1234 5678]  ← Can edit directly
   ...
   ```

4. **Make Changes:**
   - Update editable fields as needed
   - System highlights fields you can edit
   - Grayed-out fields require change request

5. **Save:**
   - Click "Save Changes"
   - Changes take effect immediately
   - Audit log records your action

6. **Confirmation:**
   ```
   ✅ Sponsor Updated Successfully
   
   Changes made:
   - Contact Person: "John Smith" → "Jane Doe"
   - Contact Email: "john.smith@..." → "jane.doe@..."
   
   [View Sponsor] [Back to List]
   ```

**When to Edit vs. Create New:**

✅ **Edit Existing Sponsor When:**
- Same organization, updated contact information
- Address changed (relocation)
- Phone/email changed
- Minor name correction (typo fix)

❌ **Create New Sponsor When:**
- Completely different organization
- Different legal entity (even if related)
- Merger created new entity name
- No existing record found

### Workflow 4: Submit Change Request

**When to Use:** Need to change Sponsor Name, Legal Name, or Sponsor ID

**Prerequisites:**
- Have justification for change
- Supporting documentation (if available)
- Verified change with sponsor contact

**Step-by-Step:**

1. **Navigate to Sponsor:**
   - Find sponsor needing change
   - Click "Edit" button
   - Notice fields requiring change request

2. **Click "Request Change" Link:**
   ```
   Sponsor Name:  [Global Tech Corporation]
                  ⚠️ Changes to this field require Administrator approval
                  [Request Name Change]  ← Click here
   ```

3. **Change Request Form:**
   ```
   Submit Change Request: SP-12345
   
   Field to Change*:    ● Sponsor Name
                        ○ Legal Name
                        ○ Sponsor ID
   
   Current Value:       "Global Tech Corporation"
   
   Proposed Value*:     [Global Tech Corporation Inc.]
                        ↑ Enter new value
   
   Justification*:      [____________________________________________]
                        [Company officially changed name as part of  ]
                        [rebranding. Requested by sponsor contact.   ]
                        [____________________________________________]
                        ↑ Explain why change is needed (required)
   
   Supporting Documents: [Choose File] [No file chosen]
                         ↑ Optional but recommended
                         Accepted: PDF, DOC, DOCX, JPG, PNG (10 MB max)
   
   [Cancel] [Submit Request]
   ```

4. **Fill Form Completely:**
   - Select field to change
   - Enter exact new value
   - Provide clear justification (required)
   - Upload supporting documents if available:
     - Official letter from sponsor
     - Business registration update
     - Email correspondence
     - Contract showing new name

5. **Submit Request:**
   - Review information
   - Click "Submit Request"
   - Confirmation message appears

6. **Post-Submission:**
   ```
   ✅ Change Request Submitted
   
   Request ID: CR-006
   Status: Pending Administrator Approval
   
   You'll be notified when the request is reviewed.
   Track status: Dashboard → My Change Requests
   
   [View Request] [Back to Sponsor]
   ```

**Tips for Successful Requests:**

✅ **Clear Justification:**
```
Good Justification:
"Sponsor officially changed company name from 'Global Tech Corporation' 
to 'Global Tech Corporation Inc.' as part of corporate restructuring. 
Change effective April 1, 2026 per attached business registration. 
Sponsor contact (jane.doe@globaltech.com) requested update."

Bad Justification:
"Name change"
(Too brief, no context, no supporting information)
```

✅ **Supporting Documents:**
- Official letterhead documents
- Business registration certificates
- Signed correspondence
- Legal name change filings
- Board resolutions

✅ **Proposed Values:**
```
Good Proposed Value:
"Global Tech Corporation Inc."
(Exact, properly capitalized, complete)

Bad Proposed Value:
"global tech inc"
(Improper capitalization, informal)
```

**Tracking Your Requests:**

1. **View All Your Requests:**
   - Dashboard → My Change Requests
   - Shows all requests with status

2. **Request Status:**
   ```
   ┌────────┬─────────────────┬──────────────┬──────────┬────────────┐
   │ Req ID │ Field           │ Sponsor      │ Status   │ Submitted  │
   ├────────┼─────────────────┼──────────────┼──────────┼────────────┤
   │ CR-006 │ Sponsor Name    │ SP-12345     │ 🟡 Pending│ 04/23/2026 │
   │ CR-005 │ Legal Name      │ SP-12346     │ 🟢 Approved│04/22/2026 │
   │ CR-004 │ Address         │ SP-12347     │ 🔴 Rejected│04/21/2026 │
   └────────┴─────────────────┴──────────────┴──────────┴────────────┘
   ```

3. **Status Meanings:**
   - 🟡 **Pending:** Awaiting Administrator review
   - 🟢 **Approved:** Change applied to sponsor record
   - 🔴 **Rejected:** Change not applied, see reason
   - 🔵 **Info Requested:** Administrator needs more information

4. **If Rejected:**
   - Click request to view rejection reason
   - Review Administrator feedback
   - Correct issues noted
   - Resubmit with corrections

---

## Letter of Guarantee (LoG) Management

### LoG Overview

Letters of Guarantee (LoGs) define coverage agreements between sponsors and students.

**Key Concepts:**

- **One LoG per Student-Sponsor-SchoolYear**
- **Status Progression:** Draft → Active → Past
- **Coverage Rules:** What's covered, what's split, what's not covered
- **Effective Dates:** Coverage start and end dates

### Workflow 5: View LoGs

![LoG List View](screenshots/admissions/logs/admissions_11_portal_logs_list.png)  
*Figure 4.1: Letters of Guarantee list for current school year*

**URL:** `/Portal/Index`

![LoG List](screenshots/admissions/logs/admissions_11_portal_logs_list.png)  
*Figure 4.1: Letters of Guarantee list with current school year*

**Access:**
- Main Menu → Portal
- Dashboard → Active LoGs

**LoG List View:**

```
School Year: [2025-2026 ▼]  ← Always check selected year!

┌─────────────┬────────────────┬──────────────┬──────────────┬──────────┐
│ LoG ID      │ Sponsor        │ Student      │ Coverage     │ Status   │
├─────────────┼────────────────┼──────────────┼──────────────┼──────────┤
│ LOG-2526-01 │ Global Tech    │ Smith, John  │ Full Medical │ 🟡 Draft │
│ LOG-2526-02 │ ABC Corp       │ Doe, Jane    │ Tuition      │ 🟢 Active│
│ LOG-2526-03 │ XYZ Holdings   │ Brown, Bob   │ Partial      │ 🟢 Active│
└─────────────┴────────────────┴──────────────┴──────────────┴──────────┘

[Create New LoG] [Export to CSV]
```

**Filters:**
```
Status:    [All ▼] [Draft] [Active] [Inactive]
Sponsor:   [All ▼] [Search sponsors...]
Student:   [All ▼] [Search students...]
```

**Status Indicators:**
- 🟡 **Draft:** Created but not activated (you can edit)
- 🟢 **Active:** Activated by Administrator (limited editing)
- 🔴 **Inactive:** Deactivated (historical record)
- ⚫ **Past:** Expired (history only)

### Workflow 6: Create New LoG

**When:** New student needs coverage or new school year begins

**Prerequisites:**
- ✅ Sponsor exists and is active
- ✅ Student exists in system
- ✅ Correct school year selected
- ✅ No existing active LoG for this student-sponsor-year

**Step-by-Step:**

1. **Navigate to Portal:**
   - Main Menu → Portal
   - Click "Create New LoG"

2. **Verify School Year:**
   ```
   ⚠️ Important: Verify school year before proceeding!
   
   School Year*: [2025-2026 ▼]
   ```
   - School year selector at top
   - Ensure correct year selected
   - Common mistake: Wrong year selected

3. **Select Sponsor:**
   ```
   Sponsor*: [Search or select sponsor...  ▼]
             
   Start typing to search:
   [glo___]
   
   Results:
   - SP-12345 | Global Tech Corporation
   - SP-12399 | Globe Holdings Inc
   ```
   
   **Tips:**
   - Type to search (case-insensitive)
   - Shows Sponsor ID and Name
   - Only active sponsors appear
   - If sponsor missing, create sponsor first

4. **Select Student:**
   ```
   Student*: [Search or select student...   ▼]
             
   Start typing to search:
   [smith___]
   
   Results:
   - 123456 | Smith, John - Grade 10
   - 123499 | Smith, Jane - Grade 11
   ```
   
   **Tips:**
   - Students from selected school year only
   - Shows: Student ID | Last, First - Grade
   - If student missing, sync from PowerSchool
   - Or contact Administrator to add manually

5. **LoG Details:**
   ```
   LoG Number:          [LOG-2526-15]
                        ↑ Auto-generated (can edit if needed)
   
   Issue Date*:         [04/23/2026]
                        ↑ Today's date (when LoG issued)
   
   Effective Date*:     [08/01/2025]
                        ↑ Coverage start date (usually school year start)
   
   Expiry Date*:        [06/30/2026]
                        ↑ Coverage end date (usually school year end)
   ```
   
   **Date Guidelines:**
   - **Issue Date:** Today or date LoG document signed
   - **Effective Date:** Usually school year start (August 1)
   - **Expiry Date:** Usually school year end (June 30)
   - Effective Date must be ≤ Expiry Date

6. **Coverage Type:**
   ```
   Coverage Type*:      [Full Medical ▼]
                        
   Options:
   - Full Medical (100% coverage all categories)
   - Partial Medical (coverage with rules)
   - Tuition Only (tuition fees only)
   - Custom (define your own rules)
   ```
   
   **Coverage Type Descriptions:**
   
   **Full Medical:**
   - All medical expenses covered 100%
   - All dental expenses covered 100%
   - Tuition covered 100%
   - Books & materials covered 100%
   - Activities covered 100%
   - No exclusions (unless specified)
   
   **Partial Medical:**
   - Medical covered with rules (percentages, limits)
   - Dental may be covered partially
   - Tuition may be covered
   - Define specific rules below
   
   **Tuition Only:**
   - Only tuition fees covered
   - No medical, dental, books, activities
   - Specific to educational fees
   
   **Custom:**
   - Define your own coverage rules
   - Mix of covered/split/not covered items

7. **Coverage Rules (Define What's Covered):**
   
   Click **"Add Coverage Rule"** to define rules:
   
   ```
   Coverage Rules:
   
   Rule #1:
   ┌──────────────────────────────────────────────────┐
   │ Item Category*:  [Medical ▼]                     │
   │                  - Medical                       │
   │                  - Dental                        │
   │                  - Tuition                       │
   │                  - Books & Materials             │
   │                  - Uniforms                      │
   │                  - Activities & Field Trips      │
   │                  - Laboratory Fees               │
   │                  - Other                         │
   │                                                  │
   │ Coverage Level*: [Covered ▼]                     │
   │                  - Covered (100% by sponsor)     │
   │                  - Split (shared cost)           │
   │                  - Not Covered (0% by sponsor)   │
   │                                                  │
   │ Percentage:      [100] %                         │
   │                  ↑ Only if "Split" selected      │
   │                                                  │
   │ Notes:           [All medical expenses covered   ]│
   │                  [except cosmetic procedures.    ]│
   │                                                  │
   │ [Remove Rule]                                    │
   └──────────────────────────────────────────────────┘
   
   [+ Add Another Rule]
   ```
   
   **Example Coverage Rules:**
   
   **Example 1: Full Sponsorship**
   ```
   Rule 1: Medical - Covered 100%
   Rule 2: Dental - Covered 100%
   Rule 3: Tuition - Covered 100%
   Rule 4: Books - Covered 100%
   Rule 5: Activities - Covered 100%
   
   Notes: Full sponsorship, no exclusions
   ```
   
   **Example 2: Medical Only**
   ```
   Rule 1: Medical - Covered 100%
   Rule 2: Dental - Covered 100%
   Rule 3: Tuition - Not Covered
   Rule 4: Books - Not Covered
   Rule 5: Activities - Not Covered
   
   Notes: Medical expenses only. Family pays tuition and other fees.
   ```
   
   **Example 3: Cost Sharing**
   ```
   Rule 1: Medical - Split 80% (sponsor pays 80%, family pays 20%)
   Rule 2: Dental - Split 50%
   Rule 3: Tuition - Covered 100%
   Rule 4: Books - Split 75%
   Rule 5: Activities - Not Covered
   
   Notes: Cost-sharing arrangement with family.
   ```

8. **Special Instructions:**
   ```
   Special Instructions:
   [__________________________________________________________]
   [Pre-authorization required for medical expenses >PHP5,000]
   [Contact sponsor before scheduling dental procedures       ]
   [__________________________________________________________]
   
   Use this field for:
   - Pre-authorization requirements
   - Spending limits
   - Contact information for claims
   - Exclusions
   - Special conditions
   ```

9. **Attachments (Optional):**
   ```
   Upload LoG Document:  [Choose File] [No file chosen]
   
   Accepted formats: PDF, DOC, DOCX, JPG, PNG
   Maximum size: 10 MB
   
   Upload signed LoG document, sponsor letter, or agreement.
   ```

10. **Save as Draft:**
    ```
    [Cancel] [Save as Draft]
    ```
    
    - Click "Save as Draft"
    - LoG saved but NOT activated
    - Can edit later before activation
    - Administrator must activate before coverage takes effect

11. **Confirmation:**
    ```
    ✅ LoG Created Successfully!
    
    LoG ID: LOG-2526-15
    Status: Draft
    Sponsor: ABC Corporation
    Student: Doe, Jane
    School Year: 2025-2026
    
    ⚠️ This LoG must be activated by an Administrator 
       before coverage takes effect.
    
    [View LoG Details] [Create Another LoG] [Back to List]
    ```

**Common Mistakes:**

❌ **Wrong School Year:**
- Always verify school year selector
- Easy to create LoG for wrong year
- Cannot easily fix after creation

❌ **Missing Coverage Rules:**
- Define clear coverage rules
- Don't leave ambiguous
- Specify what's covered explicitly

❌ **Incorrect Date Range:**
- Effective Date should be ≤ Expiry Date
- Usually aligns with school year
- Check for typos in year (2025 vs 2026)

❌ **Duplicate LoG:**
- Check existing LoGs for student-sponsor-year
- System may allow duplicates (warn only)
- Avoid creating multiple active LoGs

### Workflow 7: Edit Draft LoG

**When You Can Edit:**
- LoG status is "Draft"
- You created the LoG
- LoG is not yet activated

**Step-by-Step:**

1. **Find Draft LoG:**
   - Portal → LoG List
   - Filter by Status: Draft
   - Find your LoG

2. **Click LoG ID or "View"**

3. **Click "Edit" Button:**
   - Only visible if LoG is Draft
   - All fields editable

4. **Make Changes:**
   - Update any field as needed
   - Add/remove coverage rules
   - Update special instructions

5. **Save:**
   - Click "Save Changes"
   - Remains as Draft
   - Or click "Request Activation" if ready

6. **Request Activation:**
   ```
   Request LoG Activation: LOG-2526-15
   
   Review checklist:
   ☑ Sponsor is correct
   ☑ Student is correct
   ☑ Dates are accurate
   ☑ Coverage rules are complete
   ☑ Special instructions added
   ☑ Document uploaded (if required)
   
   [Cancel] [Request Activation]
   ```
   
   - Click "Request Activation"
   - Administrator will review and activate
   - You'll be notified when activated

**Cannot Edit After Activation:**

⚠️ Once Administrator activates LoG:
- Cannot edit sponsor, student, school year
- Cannot edit coverage rules
- Can only update special instructions
- To make major changes: Contact Administrator

### Workflow 8: View LoG Details

**URL:** `/Portal/Details/{id}`

**LoG Details Page:**

```
┌──────────────────────────────────────────────────────────┐
│ LOG-2526-15                                    🟡 Draft   │
├──────────────────────────────────────────────────────────┤
│ School Year:    2025-2026                                │
│ Sponsor:        SP-12400 | ABC Corporation               │
│ Student:        123457 | Doe, Jane - Grade 11            │
│                                                          │
│ Issue Date:     April 23, 2026                           │
│ Effective Date: August 1, 2025                           │
│ Expiry Date:    June 30, 2026                            │
│                                                          │
│ Coverage Type:  Custom                                   │
│                                                          │
│ Coverage Rules:                                          │
│   Medical:      ✅ Covered (100%)                        │
│   Dental:       ⚖️ Split (50% sponsor, 50% family)       │
│   Tuition:      ✅ Covered (100%)                        │
│   Books:        ❌ Not Covered                           │
│   Activities:   ❌ Not Covered                           │
│                                                          │
│ Special Instructions:                                    │
│   Pre-authorization required for medical >PHP 5,000      │
│   Dental procedures require prior approval               │
│                                                          │
│ Attachments:                                             │
│   📎 log_document.pdf (uploaded 04/23/2026)              │
│                                                          │
│ Created:        April 23, 2026 by admissions@ism.edu.ph │
│ Last Modified:  April 23, 2026                           │
├──────────────────────────────────────────────────────────┤
│ [Edit] [Request Activation] [Delete] [Print]            │
└──────────────────────────────────────────────────────────┘
```

**Actions Available:**

**Edit:** 
- Only if Draft status
- Opens edit form
- All fields editable

**Request Activation:**
- Sends to Administrator for activation
- Provide activation checklist
- Administrator reviews and activates

**Delete:**
- Only if Draft status
- Removes LoG completely
- Confirmation required

**Print:**
- Generates printable LoG document
- Includes all details and coverage rules
- PDF format

---

## Change Requests

### Overview

You previously learned about submitting change requests for sponsors. Here's a detailed view of managing all your requests.

### Workflow 9: View All Your Change Requests

**URL:** `/ReviewRequest/MyRequests`

**Access:**
- Dashboard → My Change Requests
- Track all submitted requests

**Requests List:**

```
My Change Requests

┌────────┬─────────────────┬──────────────┬──────────────┬────────────┐
│ Req ID │ Type            │ Sponsor      │ Status       │ Submitted  │
├────────┼─────────────────┼──────────────┼──────────────┼────────────┤
│ CR-006 │ Sponsor Name    │ SP-12345     │ 🟡 Pending   │ 04/23/2026 │
│ CR-005 │ Legal Name      │ SP-12346     │ 🟢 Approved  │ 04/22/2026 │
│ CR-004 │ Address Update  │ SP-12347     │ 🔴 Rejected  │ 04/21/2026 │
│ CR-003 │ Contact Change  │ SP-12348     │ 🔵 Info Req. │ 04/20/2026 │
└────────┴─────────────────┴──────────────┴──────────────┴────────────┘

[Filter by Status ▼] [Export to CSV]
```

**Status Details:**

🟡 **Pending:**
- Awaiting Administrator review
- No action needed from you
- Check back periodically

🟢 **Approved:**
- Change applied to sponsor record
- View sponsor to see updated information
- Email notification sent (if configured)

🔴 **Rejected:**
- Change not applied
- Click request to view rejection reason
- Review feedback and resubmit if appropriate

🔵 **Info Requested:**
- Administrator needs more information
- Click request to see what's needed
- Provide additional information promptly

### Workflow 10: Respond to Information Request

**When:** Administrator requests more information on your change request

**Step-by-Step:**

1. **Identify Request:**
   - My Change Requests shows "Info Requested" status
   - Email notification (if configured)

2. **Click Request ID:**
   ```
   Change Request: CR-003
   Status: 🔵 Information Requested
   
   Original Request:
   Field: Contact Person
   Current: "John Smith"
   Proposed: "Jane Doe"
   Justification: "Contact changed"
   
   Administrator Comments:
   "Please provide official documentation showing the contact 
    change. Current justification is insufficient. Need:
    - Email from sponsor confirming change
    - Updated business card or directory listing
    - Or official letter on sponsor letterhead"
   
   Requested by: admin@ism.edu.ph
   Request Date: April 22, 2026
   ```

3. **Provide Additional Information:**
   ```
   Respond to Information Request: CR-003
   
   Additional Information*:
   [_________________________________________________________]
   [Attached email from sponsor CEO confirming Jane Doe is   ]
   [new primary contact effective April 15, 2026. Also       ]
   [including updated org chart showing Jane as Director.    ]
   [_________________________________________________________]
   
   Additional Documents:
   [Choose Files] [sponsor_email.pdf, org_chart.pdf]
   
   [Cancel] [Submit Response]
   ```

4. **Submit Response:**
   - Click "Submit Response"
   - Request status returns to "Pending"
   - Administrator notified
   - Reviews again with new information

5. **Follow-Up:**
   - Check back in 24-48 hours
   - Request will be approved or rejected
   - May be asked for more info again if insufficient

---

## Reports

### Admissions Reports

**URL:** `/AdmissionsReports/Index`

As Admissions staff, you have access to reports relevant to your work.

### Available Reports

#### 1. My Created LoGs Report

**What It Shows:**
- All LoGs you've created
- Status breakdown (Draft/Active/Past)
- By school year

**Filters:**
- School year
- Status
- Date range

**Usage:**
- Track your work volume
- Follow up on draft LoGs
- Performance metrics

---

#### 2. Sponsors I Manage Report

**What It Shows:**
- Sponsors you created
- Associated LoGs
- Contact information

**Filters:**
- Active/Inactive
- Date created

**Usage:**
- Track sponsors you work with
- Contact list for follow-ups
- Portfolio overview

---

#### 3. LoG Creation Activity Report

**What It Shows:**
- LoGs created by all Admissions staff
- Creation trends over time
- By staff member

**Filters:**
- School year
- Date range
- Staff member

**Usage:**
- Team performance tracking
- Identify busy periods
- Workload distribution

---

#### 4. Pending Activations Report

**What It Shows:**
- Draft LoGs awaiting activation
- How long pending
- Created by whom

**Filters:**
- Created by (self/all)
- Age (how long pending)

**Usage:**
- Follow up on pending LoGs
- Identify bottlenecks
- Prioritize activations

---

### Workflow 11: Generate Report

**Step-by-Step:**

1. **Navigate to Reports:**
   - Main Menu → Reports
   - Or: Dashboard → Generate Report

2. **Select Report Type:**
   - Click report name from list

3. **Configure Filters:**
   ```
   My Created LoGs Report
   
   Filters:
   School Year:   [2025-2026 ▼]
   Status:        [All ▼] [Draft] [Active] [Past]
   Created From:  [08/01/2025]
   Created To:    [04/23/2026]
   
   [Clear Filters] [Generate Report]
   ```

4. **Generate:**
   - Click "Generate Report"
   - Wait for processing
   - Results appear

5. **View Results:**
   ```
   My Created LoGs Report
   Generated: April 23, 2026 11:15 AM
   Total LoGs: 42
   
   Breakdown by Status:
   - Draft: 5 (12%)
   - Active: 35 (83%)
   - Past: 2 (5%)
   
   [Export to CSV] [Export to Excel] [Print]
   
   ┌─────────────┬────────────────┬──────────────┬──────────┐
   │ LoG ID      │ Sponsor        │ Student      │ Status   │
   ├─────────────┼────────────────┼──────────────┼──────────┤
   │ LOG-2526-01 │ Global Tech    │ Smith, John  │ 🟢 Active│
   │ LOG-2526-02 │ ABC Corp       │ Doe, Jane    │ 🟡 Draft │
   │ ...         │ ...            │ ...          │ ...      │
   └─────────────┴────────────────┴──────────────┴──────────┘
   ```

6. **Export (Optional):**
   - Click export format
   - File downloads
   - Use for further analysis

---

## Common Workflows

### Workflow 12: Start of School Year Process

**Scenario:** New school year beginning, need to create LoGs for returning sponsored students

**Step-by-Step:**

1. **Preparation:**
   - Verify school year created (e.g., 2026-2027)
   - Confirm students synced from PowerSchool
   - Review list of returning sponsored students
   - Contact sponsors for continued support confirmation

2. **Review Previous Year LoGs:**
   - Portal → LoG List
   - School Year: Select previous year (e.g., 2025-2026)
   - Status: Active
   - Export list for reference

3. **For Each Returning Student:**
   
   **Check Sponsor Commitment:**
   - Contact sponsor
   - Confirm continued support
   - Note any coverage changes
   
   **Create New LoG:**
   - Portal → Create New LoG
   - School Year: 2026-2027 (new year)
   - Select same sponsor
   - Select student (now in next grade)
   - Set dates: 08/01/2026 to 06/30/2027
   - Copy coverage rules from previous year (or update if changed)
   - Add special instructions
   - Save as Draft
   
   **Track Progress:**
   - Keep spreadsheet of completed LoGs
   - Mark status (Created/Pending/Activated)
   - Follow up on pending activations

4. **Bulk Creation Tips:**
   - Create in batches (e.g., 10 LoGs per session)
   - Use consistent naming (LOG-2627-001, LOG-2627-002, etc.)
   - Request activation in batches
   - Communicate with Administrator about volume

5. **Follow-Up:**
   - Check dashboard for draft count
   - Follow up on pending activations
   - Generate report of completed LoGs
   - Share progress with supervisor

### Workflow 13: New Student Onboarding

**Scenario:** New student joins mid-year with sponsor support

**Step-by-Step:**

1. **Gather Information:**
   - Student information (ID, name, grade)
   - Sponsor information (company, contact)
   - Coverage agreement details
   - Effective date (enrollment date)
   - LoG document (if available)

2. **Check for Existing Records:**
   
   **Search Student:**
   - Settings → Students (if you have access)
   - Or: Try creating LoG and search student dropdown
   - If not found: Request Administrator to sync from PowerSchool
   
   **Search Sponsor:**
   - Sponsors → Search
   - If not found: Create new sponsor (see Workflow 2)

3. **Create Sponsor (if needed):**
   - Collect complete sponsor information
   - Verify no duplicate exists
   - Create sponsor with active status

4. **Create LoG:**
   - Portal → Create New LoG
   - Select correct school year
   - Select sponsor and student
   - Set Issue Date: Today
   - Set Effective Date: Student's enrollment date
   - Set Expiry Date: End of school year
   - Define coverage rules based on agreement
   - Add special instructions
   - Upload LoG document
   - Save as Draft

5. **Request Activation:**
   - Review LoG for accuracy
   - Request activation from Administrator
   - Notify Cashier of new sponsored student

6. **Communication:**
   - Email sponsor confirmation of LoG creation
   - Provide LoG details and effective date
   - Include cashier contact for billing questions

### Workflow 14: Sponsor Contact Change

**Scenario:** Sponsor's primary contact person changed

**Step-by-Step:**

1. **Receive Notice:**
   - Email from sponsor
   - Phone call
   - Official letter

2. **Verify Information:**
   - Confirm with sponsor
   - New contact: Name, email, phone
   - Effective date of change

3. **Update Sponsor Record:**
   - Search for sponsor
   - Click Edit
   - Update Contact Person field ← Direct edit (no request needed)
   - Update Contact Email ← Direct edit
   - Update Contact Phone ← Direct edit
   - Move old contact to Alternative Email/Phone (optional)
   - Save changes

4. **Confirmation:**
   - Changes take effect immediately
   - Send email to new contact welcoming them
   - Include your contact information
   - Offer assistance

5. **Update Communication Lists:**
   - Update your contact spreadsheet
   - Note change in sponsor file
   - Inform colleagues if shared contact

### Workflow 15: Coverage Change Request

**Scenario:** Sponsor wants to change coverage for existing active LoG

**Problem:** Cannot directly edit active LoG

**Options:**

**Option A: Contact Administrator**
1. Email Administrator
2. Explain coverage change needed
3. Provide LoG ID and sponsor confirmation
4. Administrator can deactivate, you edit, then reactivate

**Option B: Create New LoG (if mid-year change)**
1. Deactivate old LoG (Administrator)
2. Create new LoG with new coverage
3. Effective date: Date of change
4. Note change in special instructions
5. Maintain audit trail

**Option C: Special Instructions Update** (Minor changes only)
1. Edit LoG (if you still have edit permission)
2. Update Special Instructions field:
   - "Coverage changed effective [date]: [details]"
3. Document change
4. Notify Cashier of change

**Best Practice:**
- Document all coverage changes
- Get written confirmation from sponsor
- Update cash ier and finance immediately
- Note change in LoG special instructions

---

## Troubleshooting

### Common Issues & Solutions

#### Issue: Cannot Find Sponsor When Creating LoG

**Cause:** Sponsor inactive or doesn't exist

**Solution:**
1. Search sponsors list for sponsor name
2. If found but inactive: Contact Administrator to reactivate
3. If not found: Create new sponsor first
4. Return to LoG creation and search again

---

#### Issue: Student Not in Dropdown

**Cause:** Student not in system or wrong school year

**Solution:**
1. Verify correct school year selected
2. Contact Administrator to sync from PowerSchool
3. If urgent: Request Administrator to manually add student
4. Wait for sync, then retry LoG creation

---

#### Issue: Duplicate LoG Warning

**System Message:** "Student already has an active LoG for this sponsor in this school year."

**Solution:**
1. Check Portal for existing LoG
2. If duplicate is a mistake: Delete one
3. If different sponsor: Allowed (different sponsors can support same student)
4. If same sponsor: Update existing LoG instead of creating new

---

#### Issue: Change Request Rejected

**Scenario:** Administrator rejected your change request

**Solution:**
1. View request details
2. Read rejection reason carefully
3. Understand what's needed:
   - More documentation?
   - Better justification?
   - Different approach?
4. Gather required information
5. Resubmit with corrections
6. Or: Contact Administrator for clarification

---

#### Issue: Draft LoG Not Activating

**Cause:** Pending Administrator action

**Solution:**
1. Check if request submitted for activation
2. If not submitted: Review LoG, request activation
3. If submitted: Wait for Administrator review (24-48 hours)
4. If urgent: Contact Administrator via email or phone
5. Verify all required fields complete

---

#### Issue: Cannot Edit Sponsor Field

**Scenario:** Field is grayed out in edit form

**Cause:** Field requires change request

**Solution:**
1. Note which field needs changing
2. Click "Request Change" link next to field
3. Submit change request with justification
4. Wait for Administrator approval
5. Changes apply after approval

---

#### Issue: Report Shows No Data

**Cause:** Filters too restrictive or no data exists

**Solution:**
1. Clear all filters
2. Select broader date range
3. Change status filter to "All"
4. Verify you created records in that period
5. Check correct school year selected

---

## FAQs

### General Questions

**Q: What is my role as Admissions staff?**
A: You manage sponsor relationships and create Letters of Guarantee for sponsored students. You can create/edit sponsors (with some restrictions), create LoGs, and submit change requests for Administrator approval.

---

**Q: Can I delete a sponsor?**
A: No, only Administrators can delete sponsors. You can edit contact information directly, but sensitive fields require a change request.

---

**Q: How do I change my password?**
A: Click your name (top-right) → Profile → Change Password. Enter your current password and new password.

---

**Q: What's the difference between saving as Draft vs. requesting Activation for a LoG?**
A:
- **Save as Draft:** LoG saved but not active; you can edit; coverage not in effect; Administrator not notified
- **Request Activation:** LoG submitted to Administrator for review and activation; coverage will take effect after activation; limited editing after activation

---

### Sponsor Questions

**Q: Should I create a new sponsor or edit existing?**
A:
- **Edit existing** if: Same organization, updated contact info, address change, minor corrections
- **Create new** if: Completely different organization, different legal entity, no existing record found

---

**Q: Why can't I edit Sponsor Name or Legal Name directly?**
A: These are sensitive fields that require Administrator approval to ensure data accuracy and maintain audit trail. Submit a change request with justification and supporting documents.

---

**Q: How do I know if a sponsor already exists?**
A: Always search before creating:
1. Sponsors → Search
2. Type sponsor name or partial name
3. Review results for similar names
4. Check contact information to verify if same entity
5. If unsure, ask Administrator

---

**Q: Can a sponsor support multiple students?**
A: Yes! One sponsor can have multiple active LoGs for different students. Create separate LoG for each student.

---

### LoG Questions

**Q: Can I create a LoG for a past school year?**
A: Generally no, unless special circumstances. LoGs should be created for current or future school years. Contact Administrator if you need to create a historical LoG.

---

**Q: What if I created a LoG for the wrong school year?**
A: If still Draft:
1. Delete the incorrect LoG
2. Create new LoG with correct school year

If already Activated: Contact Administrator to deactivate, then you can recreate with correct school year.

---

**Q: How long does it take for a LoG to be activated?**
A: Typical turnaround: 24-48 hours. Administrator reviews LoG for completeness and accuracy before activating. If urgent, contact Administrator directly.

---

**Q: Can a student have multiple LoGs from different sponsors?**
A: Yes, a student can have LoGs from different sponsors. Each LoG defines separate coverage from that sponsor.

---

**Q: What happens when a LoG expires?**
A: LoG automatically becomes "Past" status on expiry date. Coverage stops. No action needed from you. For next school year, create a new LoG.

---

### Change Request Questions

**Q: How long does it take for a change request to be approved?**
A: Typical turnaround: 24-48 hours. Complex requests may take longer. Check "My Change Requests" for status updates.

---

**Q: Can I cancel a pending change request?**
A: Once submitted, you cannot cancel directly. Contact Administrator via email requesting cancellation if needed.

---

**Q: What if I submitted a request with wrong information?**
A: Contact Administrator immediately via email explaining the error. Administrator can reject the request, allowing you to resubmit with correct information.

---

**Q: Do I need documentation for every change request?**
A: Documentation is optional but highly recommended, especially for:
- Legal name changes
- Sponsor ID changes
- Major updates

Documentation increases approval likelihood and speeds up the process.

---

### Report Questions

**Q: Why don't I see all LoGs in my report?**
A: Reports show only LoGs YOU created. Administrators see all LoGs. Check filters—date range, status, school year may be limiting results.

---

**Q: Can I schedule reports to run automatically?**
A: Not currently. This feature is planned for future release. You must generate reports manually.

---

**Q: How do I share a report with my supervisor?**
A: Generate report, export to CSV or Excel, email file to supervisor. Or: Take screenshot of report and share.

---

### Technical Questions

**Q: What browsers are supported?**
A: Chrome, Edge, Firefox (latest versions). Internet Explorer is not supported.

---

**Q: Can I use the system on my phone?**
A: Limited mobile support. System is designed for desktop use. For best experience, use desktop or laptop computer.

---

**Q: Why is the system slow?**
A: Possible reasons:
- Slow internet connection
- Large dataset being loaded
- System under heavy use
- Browser cache needs clearing

Solutions:
- Use search/filters to reduce data
- Clear browser cache
- Try different browser
- Report persistent slowness to Administrator

---

**Q: What if I encounter an error?**
A:
1. Take screenshot of error message
2. Note what you were doing when error occurred
3. Click your name → Feedback → Report Bug
4. Describe issue with screenshot
5. Or: Contact Administrator directly

---

## Conclusion

As an Admissions staff member, you play a vital role in maintaining sponsor relationships and ensuring students receive proper coverage through Letters of Guarantee.

### Key Takeaways

✅ **Your Responsibilities:**
- Create and maintain sponsor records
- Create LoGs for sponsored students
- Submit change requests for sensitive fields
- Maintain accurate and complete data
- Generate reports as needed

✅ **Best Practices:**
- Always search before creating (avoid duplicates)
- Use clear, complete information
- Verify school year before creating LoGs
- Define specific coverage rules
- Submit change requests with good justification
- Follow up on pending items

✅ **Communication:**
- Keep sponsor contacts updated
- Notify Cashier of new/changed LoGs
- Collaborate with Administrator on complex issues
- Document conversations and agreements

✅ **Quality:**
- Double-check data before saving
- Review LoGs before requesting activation
- Keep information current
- Maintain professional standards

### Daily Checklist

**Start of Day:**
- [ ] Check dashboard for pending work
- [ ] Review draft LoGs needing completion
- [ ] Check status of change requests

**During Day:**
- [ ] Process new sponsor requests
- [ ] Create LoGs as needed
- [ ] Respond to sponsor inquiries
- [ ] Update contact information
- [ ] Submit change requests with documentation

**End of Day:**
- [ ] Complete draft LoGs
- [ ] Follow up on pending activations
- [ ] Review tomorrow's priorities

### Getting Help

**Resources:**
- This User Manual
- In-app help (? icon)
- Your Name → Feedback
- Administrator via email
- Training materials
- Colleague support

**Support Contacts:**
- Administrator: admin@ism.edu.ph
- Technical Support: support@ism.edu.ph
- Training: training@ism.edu.ph

---

**Document Information:**
- **Version:** 1.0
- **Last Updated:** April 23, 2026
- **Document Owner:** ISM Sponsor System Team
- **Feedback:** Submit via Your Name → Feedback

---

**Thank you for your dedication to supporting our sponsored students!**
