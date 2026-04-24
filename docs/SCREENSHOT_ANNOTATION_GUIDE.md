# Screenshot Annotation Guide
## ISM Sponsor Management System

**Purpose:** Add visual annotations (arrows, highlights, callouts) to screenshots for enhanced user manual clarity

**Date:** April 24, 2026

---

## Annotation Requirements by Screenshot

### Tools Needed
- **Recommended:** Greenshot (Free, Windows/Mac), Skitch (Mac), Snagit (Paid)
- **Alternative:** Preview.app (Mac), Paint.NET (Windows), GIMP (All platforms)
- **Online:** Markup.io, Pixlr Editor

### Annotation Standards

**Colors:**
- 🔴 **Red:** Primary actions, buttons to click
- 🟡 **Yellow:** Highlights for important information users must read
- 🔵 **Blue:** Secondary information, reference data
- 🟢 **Green:** Success states, confirmation messages
- 🟠 **Orange:** Warnings, cautions

**Elements:**
- **Arrows:** Point to specific buttons, fields, or actions
- **Boxes:** Highlight sections or groups of related controls
- **Numbers:** Sequential steps (1, 2, 3) for multi-step processes
- **Text Callouts:** Brief explanations near UI elements
- **Blur/Redact:** Sensitive data (already using demo data)

---

## Admin Manual Screenshots (13 screenshots)

### 1. admin_01_login_page.png
**Location:** `docs/screenshots/admin/logs/admin_01_login_page.png`

**Annotations Needed:**
- 🔴 Red arrow → Username field with label "Enter: admin"
- 🔴 Red arrow → Password field with label "Enter: Admin@123"
- 🔴 Red box → "Login" button with label "Click here"
- 🟡 Yellow highlight → "Forgot Password" link (if visible)

**Purpose:** Show exactly where to enter credentials and click

---

### 2. admin_02_dashboard_overview.png
**Location:** `docs/screenshots/admin/dashboard/admin_02_dashboard_overview.png`

**Annotations Needed:**
- 🔵 Blue boxes → Each statistics card (4 boxes total)
- Numbers 1-4 → Label each card: "Active Sponsors", "Pending Requests", "Active LoGs", "System Health"
- 🔴 Red arrows → Navigation menu items on left
- 🟡 Yellow highlight → User menu (top right corner)
- Text callout → "Your main control center"

**Purpose:** Orient users to dashboard layout and key areas

---

### 3. admin_03_dashboard_stats.png
**Location:** `docs/screenshots/admin/dashboard/admin_03_dashboard_stats.png`

**Annotations Needed:**
- 🟡 Yellow highlights → Each number/statistic
- Text callouts → Explain what each metric means
- 🔵 Blue box → Recent activity section
- 🔴 Red arrow → "View Details" or action links (if visible)

**Purpose:** Help users understand the metrics displayed

---

### 4. admin_06_sponsors_list.png
**Location:** `docs/screenshots/admin/sponsors/admin_06_sponsors_list.png`

**Annotations Needed:**
- 🔴 Red arrow → "Create New Sponsor" button
- 🟡 Yellow highlight → Search box with label "Search sponsors here"
- 🔵 Blue boxes → Each column header in table
- Numbers 1-3 → Show sequence: "1. Search" → "2. Filter" → "3. Click sponsor"
- 🔴 Red arrow → First sponsor row (example of clickable item)

**Purpose:** Guide users through sponsor list navigation

---

### 5. admin_09_create_sponsor_empty.png
**Location:** `docs/screenshots/admin/sponsors/admin_09_create_sponsor_empty.png`

**Annotations Needed:**
- 🔴 Red asterisk (*) highlights → All required fields
- Numbers 1-7 → Sequential order to complete form
- 🟡 Yellow highlight → Example format for each field
- 🔴 Red arrow → "Save" button at bottom
- Text callout → "All fields with * are required"

**Purpose:** Show users exactly how to fill the form

---

### 6. admin_11_sponsor_details.png
**Location:** `docs/screenshots/admin/sponsors/admin_11_sponsor_details.png`

**Annotations Needed:**
- 🔵 Blue boxes → Section groupings (Basic Info, Contact, Address)
- 🔴 Red arrows → Action buttons (Edit, Delete, View LoGs)
- 🟡 Yellow highlight → Sponsor status indicator
- Text callouts → Key information fields users should verify

**Purpose:** Help users locate information and actions

---

### 7. admin_14_portal_logs_list.png
**Location:** `docs/screenshots/admin/logs/admin_14_portal_logs_list.png`

**Annotations Needed:**
- 🔴 Red arrow → School year selector with label "Always check this first!"
- 🟡 Yellow highlight → Filter options
- 🔵 Blue boxes → Table columns
- 🔴 Red arrow → "Create New LoG" button
- Numbers → Show clickable LoG ID

**Purpose:** Guide navigation of LoG list

---

### 8. admin_16_create_log_step1.png
**Location:** `docs/screenshots/admin/logs/admin_16_create_log_step1.png`

**Annotations Needed:**
- Numbers 1-5 → Form completion sequence
- 🔴 Red arrows → Each required field
- 🟡 Yellow highlight → Dropdown selectors
- Text callout → "Step 1 of 3"
- 🔵 Blue box → Help text or instructions (if visible)

**Purpose:** Show step-by-step form completion

---

### 9. admin_19_create_log_coverage_rules.png
**Location:** `docs/screenshots/admin/logs/admin_19_create_log_coverage_rules.png`

**Annotations Needed:**
- 🔴 Red arrow → "Add Coverage Rule" button with label "Click to add each rule"
- Numbers 1-4 → Show sequence of adding rules
- 🟡 Yellow highlights → Coverage Level dropdown, Percentage field
- 🔵 Blue boxes → Each completed rule
- Text callout → "Add multiple rules to define complete coverage"

**Purpose:** Demonstrate coverage rule configuration

---

### 10. admin_24_requests_list.png
**Location:** `docs/screenshots/admin/requests/admin_24_requests_list.png`

**Annotations Needed:**
- 🟡 Yellow highlight → Status indicators (Pending, Approved, Rejected)
- 🔴 Red arrows → Clickable request IDs
- 🔵 Blue box → Filter section
- Numbers → Show workflow: "1. Find request" → "2. Click ID" → "3. Review"

**Purpose:** Guide request review process

---

### 11. admin_30_users_list.png
**Location:** `docs/screenshots/admin/users/admin_30_users_list.png`

**Annotations Needed:**
- 🔴 Red arrow → "Create New User" button
- 🟡 Yellow highlights → Role column, Status column
- 🔵 Blue boxes → Action buttons (Edit, Reset Password)
- Text callouts → Explain each role type

**Purpose:** User management navigation

---

### 12. admin_36_reports_menu.png
**Location:** `docs/screenshots/admin/reports/admin_36_reports_menu.png`

**Annotations Needed:**
- Numbers 1-5 → Label each report type
- 🔴 Red arrows → Click points for each report
- 🟡 Yellow highlight → Most commonly used reports
- Text callouts → Brief description of what each report contains

**Purpose:** Help users find the right report

---

### 13. admin_41_operations_dashboard.png
**Location:** `docs/screenshots/admin/dashboard/admin_41_operations_dashboard.png`

**Annotations Needed:**
- 🔵 Blue boxes → Key metrics sections
- 🟡 Yellow highlights → Warning or alert indicators
- 🔴 Red arrows → Action items requiring attention
- Text callouts → Explain what each section monitors

**Purpose:** Operations monitoring guidance

---

## Admissions Manual Screenshots (10 screenshots)

### 1. admissions_01_login_page.png
**Annotations:** Same as admin_01 but with "admissions" credentials

### 2. admissions_02_dashboard_overview.png
**Annotations Needed:**
- 🔵 Blue boxes → Statistics relevant to admissions staff
- 🔴 Red arrows → Quick action buttons
- Numbers → Workflow sequence

### 3. admissions_04_sponsors_list.png
**Annotations Needed:**
- 🔴 Red arrow → Search functionality
- 🟡 Yellow highlight → Read-only vs editable indicators
- Text callout → "Check for duplicates before creating"

### 4. admissions_06_create_sponsor_empty.png
**Annotations:** Similar to admin version but emphasize validation

### 5. admissions_08_sponsor_details.png
**Annotations Needed:**
- 🔵 Blue box → Editable fields vs read-only
- 🔴 Red arrow → "Edit" button
- 🟡 Yellow highlight → Fields requiring change request
- Text callout → "Direct edit only for contact info"

### 6. admissions_11_portal_logs_list.png
**Annotations:** Focus on filtering and searching

### 7. admissions_13_create_log_step1.png
**Annotations Needed:**
- Numbers 1-6 → Step sequence
- 🔴 Red arrows → Required selections
- Text callout → "Verify sponsor is active first"

### 8. admissions_15_create_log_coverage_rules.png
**Annotations:** Same as admin version

### 9. admissions_19_submit_change_request.png
**Annotations Needed:**
- 🟡 Yellow highlight → Justification field with label "Required! Explain  why"
- 🔴 Red arrow → File upload area
- 🔵 Blue box → Current vs proposed values
- Text callout → "Administrator approval required"

### 10. admissions_23_reports_menu.png
**Annotations:** Label reports available to admissions role

---

## Cashier Manual Screenshots (6 screenshots)

### 1. cashier_01_login_page.png
**Annotations:** Same pattern with cashier credentials

### 2. cashier_02_dashboard_overview.png
**Annotations Needed:**
- 🔵 Blue boxes → Read-only statistics
- 🔴 Red arrows → Search functions
- Text callout → "Read-only access - for verification only"

### 3. cashier_03_sponsor_details.png
**Annotations Needed:**
- 🟡 Yellow highlights → Contact information for billing
- 🔵 Blue box → TIN and billing details
- Text callout → "No edit button - read-only view"

### 4. cashier_04_portal_logs_list.png
**Annotations Needed:**
- 🔴 Red arrows → Search by student name or ID
- 🟡 Yellow highlight → Coverage type column
- Text callout → "Find LoG to verify coverage"

### 5. cashier_05_log_details_coverage.png
**Annotations Needed:**
- 🟡 Yellow highlights → Coverage percentages
- 🔵 Blue boxes → Each coverage rule
- Numbers → Point to specific coverage categories
- Text callout → "Use this to determine billing"

### 6. cashier_11_reports_menu.png
**Annotations:** Label cashier-specific reports

---

## Sponsor Manual Screenshots (9 screenshots)

### 1. sponsor_01_login_page.png
**Annotations:** Sponsor credentials

### 2. sponsor_02_portal_dashboard.png
**Annotations Needed:**
- 🔵 Blue boxes → Key statistics for sponsors
- 🔴 Red arrows → Main navigation options
- Text callout → "Your self-service portal"

### 3. sponsor_03_my_profile.png
**Annotations Needed:**
- 🟡 Yellow highlight → Organization information
- 🔵 Blue boxes → Contact sections
- 🔴 Red arrow → "Request Change" button
- Text callout → "Submit change request to update info"

### 4. sponsor_04_my_students.png
**Annotations Needed:**
- 🔴 Red arrows → Clickable student names
- 🟡 Yellow highlight → LoG status
- Text callout → "Click student to see coverage details"

### 5. sponsor_05_my_logs.png
**Annotations Needed:**
- 🔵 Blue boxes → LoG status indicators
- 🔴 Red arrows → Filter options
- Numbers → Table columns explanation

### 6. sponsor_06_log_details.png
**Annotations Needed:**
- 🟡 Yellow highlights → Coverage rules
- 🔵 Blue boxes → Coverage categories
- Text callout → "Your financial commitments"

### 7. sponsor_07_change_requests.png
**Annotations Needed:**
- 🔴 Red arrow → "Submit New Request" button
- 🟡 Yellow highlights → Status indicators
- Text callout → "Track your requests here"

### 8. sponsor_08_submit_request.png
**Annotations Needed:**
- Numbers 1-4 → Form completion steps
- 🔴 Red arrows → Required fields
- 🟡 Yellow highlight → Justification field
- Text callout → "Provide clear reason for change"

### 9. sponsor_09_reports_menu.png
**Annotations:** Label sponsor self-service reports

---

## Annotation Workflow

### Step 1: Install Tool
```bash
# Mac
brew install --cask greenshot

# Or download Skitch
# https://evernote.com/products/skitch
```

### Step 2: Annotate Each Screenshot

For each screenshot:

1. **Open** the original screenshot in annotation tool
2. **Add** annotations per specifications above:
   - Arrows (red for primary actions)
   - Boxes (blue for sections, yellow for highlights)
   - Numbers (for sequences)
   - Text callouts (clear, concise)
3. **Save** as PNG with same filename (overwrite original)
4. **Verify** quality - ensure annotations are clear at original resolution

### Step 3: Batch Processing

**Recommended order:**
1. Login pages (all 4) - Same pattern
2. Dashboard screenshots - Establish consistency
3. Form screenshots - Standard format
4. List views - Consistent highlighting
5. Detail pages - Information focus

### Step 4: Quality Check

✅ **Checklist for each screenshot:**
- [ ] Arrows point clearly to correct elements
- [ ] Text is readable (size 14pt minimum)
- [ ] Colors follow standard (red=action, yellow=highlight, blue=info)
- [ ] Numbering is sequential and logical
- [ ] No sensitive data visible (using demo data)
- [ ] Annotations don't obscure important UI elements
- [ ] Consistent style across all screenshots

### Step 5: Update & Commit

```bash
cd "/Users/cruzr/Documents/ISM Sponsor"

# Check which screenshots were updated
git status docs/screenshots/

# Add all annotated screenshots
git add docs/screenshots/

# Commit with descriptive message
git commit -m "docs: Add visual annotations to all 38 screenshots

- Red arrows for primary actions and clickable elements
- Yellow highlights for important information
- Blue boxes for section groupings
- Sequential numbers for multi-step processes
- Text callouts for guidance
- All 4 roles: Admin, Admissions, Cashier, Sponsor
- Consistent annotation standards across all images"

# Push to repository
git push
```

---

## Alternative: Enhanced Text Descriptions

If you cannot annotate images, enhance the manual text to be more explicit:

**Before:**
```markdown
![Login Page](screenshots/admin/logs/admin_01_login_page.png)
```

**After:**
```markdown
![Login Page](screenshots/admin/logs/admin_01_login_page.png)  
*Figure 1.1: Login page - Enter username in top field, password in bottom field, then click blue "Login" button*

**What you'll see:**
- **Username field** (top): Enter `admin`
- **Password field** (below username): Enter `Admin@123`
- **Login button** (blue button at bottom): Click this to proceed
- **"Forgot Password" link** (if you need to reset): Contact IT support
```

This approach works when images cannot be edited but requires more detailed text.

---

## Estimated Time

- **Per screenshot:** 3-5 minutes for basic annotations
- **Total for 38 screenshots:** 2-3 hours
- **If doing all enhancements:** Allow 4-5 hours total

---

## Tips for Best Results

1. **Use consistent arrow styles** - all the same thickness and color
2. **Keep text brief** - 3-5 words max per callout
3. **Don't overlap annotations** - space them out
4. **Test readability** - view at actual manual size
5. **Use templates** - create reusable styles for arrows/boxes
6. **Backup originals** - save unannotated versions separately
7. **Get feedback** - test with representative users before finalizing

---

## Next Steps

1. Review this guide
2. Choose annotation tool
3. Start with login screenshots (all 4 roles) - practice
4. Move through each role systematically  
5. Commit and push annotated screenshots
6. Review in user manuals to verify display

The manuals are already comprehensive with good text - adding visual annotations will make them significantly more user-friendly and reduce training time!
