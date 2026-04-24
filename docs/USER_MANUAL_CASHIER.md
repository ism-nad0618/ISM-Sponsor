# Cashier User Manual
## ISM Sponsor Management System

**Version:** 1.0  
**Last Updated:** April 23, 2026  
**Role:** Cashier  
**Access Level:** Read-Only Access, Reports Generation

---

## Table of Contents

1. [Role Overview](#role-overview)
2. [Getting Started](#getting-started)
3. [Dashboard Guide](#dashboard-guide)
4. [Viewing Sponsors](#viewing-sponsors)
5. [Viewing Letters of Guarantee](#viewing-letters-of-guarantee)
6. [Coverage Verification](#coverage-verification)
7. [Reports](#reports)
8. [Common Workflows](#common-workflows)
9. [Troubleshooting](#troubleshooting)
10. [FAQs](#faqs)

---

## Role Overview

### Cashier Responsibilities

As a **Cashier**, your role focuses on verifying sponsor coverage for billing purposes and generating reconciliation reports.

**What You Can Do:**
- ✅ View all sponsor records (read-only)
- ✅ View all Letters of Guarantee (read-only)
- ✅ Search sponsors and LoGs
- ✅ View coverage rules and details
- ✅ Generate Cashier reports
- ✅ Export reports to CSV/Excel
- ✅ Verify coverage for specific charges
- ✅ View contact information for billing inquiries

**What You Cannot Do:**
- ❌ Create or edit sponsors
- ❌ Create or edit LoGs
- ❌ Approve change requests
- ❌ Manage users or system settings
- ❌ Activate or deactivate LoGs
- ❌ Access administrative functions

**Your Typical Daily Workflow:**
1. Look up sponsor information for billing inquiries
2. Verify LoG coverage for specific charges
3. Check coverage rules (what's covered vs. not covered)
4. Generate reconciliation reports
5. Export data for billing system
6. Answer parent/sponsor billing questions

---

## Getting Started

### Initial Login

1. **Navigate to System URL:**
   ```
   Production: https://ismsponsor.azurewebsites.net
   ```

2. **Enter Your Credentials:**
   - Email: your.cashier@ism.edu.ph
   - Password: [Your password]

3. **Security:**
   - Change default password on first login
   - Use strong password
   - Keep credentials confidential

4. **Dashboard:**
   - Upon login, you'll see the **Cashier Dashboard**
   - Shows quick statistics and search access

### Navigation Overview

**Main Navigation Bar:**
```
[Home] [Dashboard] [Sponsors] [Portal] [Reports]
```

**Menu Sections:**
- **Home** → System landing page
- **Dashboard** → Cashier dashboard
- **Sponsors** → Sponsor search (read-only)
- **Portal** → LoG search (read-only)
- **Reports** → Cashier-specific reports

**User Menu (Top-Right):**
```
[Your Name ▼]
  ├─ Profile (change password)
  ├─ Feedback (report issues)
  └─ Logout
```

### Key Concepts

**Sponsors:**
- Organizations providing financial support
- Each has unique Sponsor ID
- Contact information for billing inquiries

**Letters of Guarantee (LoGs):**
- Define coverage for sponsored students
- One LoG per student per sponsor per school year
- Contains coverage rules you need for billing

**Coverage Rules:**
- What expenses are covered
- Coverage levels (100%, split %, not covered)
- Special conditions or limits

**School Years:**
- Academic years (e.g., 2025-2026)
- Controls which LoGs you see
- Always verify correct year selected

---

## Dashboard Guide

### Cashier Dashboard

**URL:** `/Dashboard/CashierDashboard` or `/Dashboard`

Your dashboard provides quick access to search and statistics.

### Dashboard Sections

#### 1. Quick Statistics

```
┌─────────────────┬─────────────────┬─────────────────┐
│ Active Sponsors │ Active LoGs     │ Covered Students│
│     245         │      423        │      412        │
│ 📊 View all     │ 📄 View all     │ 👥 Current year │
└─────────────────┴─────────────────┴─────────────────┘
```

**Metrics Explained:**

1. **Active Sponsors:**
   - Total sponsors with active status
   - Click to view full sponsor list
   - For billing reference

2. **Active LoGs:**
   - Currently active Letters of Guarantee
   - Current school year only
   - Click to view all active LoGs

3. **Covered Students:**
   - Students with active LoG coverage
   - Current school year
   - For reference

#### 2. Quick Search

```
┌──────────────────────────────────────────────────────┐
│ Quick Sponsor Search                                │
│ [Search by sponsor name, ID, or student...] [Search]│
│                                                      │
│ Quick LoG Search                                     │
│ [Search by LoG ID or student ID/name...]   [Search]│
└──────────────────────────────────────────────────────┘
```

- Enter search term
- Press Enter or click Search
- Results appear immediately
- Click result to view details

#### 3. Quick Actions

**One-Click Actions:**
- 🔍 Search Sponsors
- 📄 Search LoGs
- 📊 Generate Reconciliation Report
- 💾 Export Coverage List
- 📞 View Sponsor Contacts

---

## Viewing Sponsors

### Workflow 1: Search for a Sponsor

**Why:** Find sponsor information for billing inquiries

**Step-by-Step:**

1. **Navigate to Sponsors:**
   - Dashboard → Sponsors
   - Or: Main Menu → Sponsors

2. **Search Options:**
   
   **Quick Search:**
   ```
   🔍 [Search sponsors...              ] [Search]
   ```
   
   **Search By:**
   - Sponsor ID (e.g., SP-12345)
   - Sponsor Name (e.g., "Global Tech")
   - Contact Person name
   - Contact Email
   - Student name (finds sponsor supporting that student)

3. **Search Tips:**
   - Partial matches work: "tech" finds "Global Tech Corporation"
   - Case-insensitive: "ABC" = "abc" = "Abc"
   - Student name search: Type student name to find their sponsor

4. **View Results:**
   ```
   Search Results: 2 sponsors found
   
   ┌────────────┬──────────────────┬─────────────────┬────────────┐
   │ Sponsor ID │ Sponsor Name     │ Contact Person  │ Active LoGs│
   ├────────────┼──────────────────┼─────────────────┼────────────┤
   │ SP-12345   │ Global Tech Corp │ John Smith      │ 5          │
   │ SP-12399   │ ABC Corporation  │ Maria Santos    │ 3          │
   └────────────┴──────────────────┴─────────────────┴────────────┘
   ```

5. **Click Sponsor Name:**
   - View complete sponsor details
   - See all associated LoGs
   - Access contact information

### Workflow 2: View Sponsor Details

**URL:** `/Settings/Sponsors/Details/{id}` (Read-only view for Cashiers)

**Sponsor Details Page:**

```
┌───────────────────────────────────────────────────────────┐
│ SP-12345 | Global Tech Corporation                 🟢 Active│
├───────────────────────────────────────────────────────────┤
│ Legal Name:          Global Tech Corporation Pty Ltd      │
│ Contact Person:      John Smith                           │
│ Contact Email:       john.smith@globaltech.com            │
│ Contact Phone:       +63 2 1234 5678                      │
│ Alternative Email:   info@globaltech.com                  │
│ Alternative Phone:   +63 917 123 4567                     │
│                                                            │
│ Address:             123 Ayala Avenue                     │
│                      Makati Finance Center                │
│                      Makati City, Metro Manila 1226       │
│                      Philippines                          │
├───────────────────────────────────────────────────────────┤
│ Associated Letters of Guarantee (Current Year):           │
│                                                            │
│ School Year: [2025-2026 ▼]                                │
│                                                            │
│ ┌────────────┬─────────────┬──────────────┬──────────┐  │
│ │ LoG ID     │ Student     │ Coverage     │ Status   │  │
│ ├────────────┼─────────────┼──────────────┼──────────┤  │
│ │ LOG-2526-01│ Smith, John │ Full Medical │ 🟢 Active│  │
│ │ LOG-2526-02│ Doe, Jane   │ Tuition Only │ 🟢 Active│  │
│ │ LOG-2526-03│ Brown, Bob  │ Partial      │ 🟢 Active│  │
│ └────────────┴─────────────┴──────────────┴──────────┘  │
│                                                            │
│ Total Active LoGs: 3                                      │
└───────────────────────────────────────────────────────────┘
```

**Information Available:**

**Contact Information:**
- Use for billing inquiries
- Primary and alternative contacts
- Email and phone numbers

**Address:**
- For mailing statements or correspondence
- Complete mailing address

**Associated LoGs:**
- Click LoG ID to view coverage details
- Shows which students are covered
- Quick reference for billing verification

---

## Viewing Letters of Guarantee

### Workflow 3: Search for a LoG

**Why:** Find coverage information for specific student or verify what's covered

**Step-by-Step:**

1. **Navigate to Portal:**
   - Dashboard → Portal
   - Or: Main Menu → Portal

2. **Select School Year:**
   ```
   School Year: [2025-2026 ▼]  ← Always verify correct year!
   ```

3. **Search Methods:**
   
   **Search Box:**
   ```
   🔍 [Search LoGs by ID, student, or sponsor...] [Search]
   ```
   
   **Filters:**
   ```
   Status:    [All ▼] [Active] [Draft] [Inactive]
   Sponsor:   [All ▼] [Search sponsors...]
   Student:   [All ▼] [Search students...]
   Coverage:  [All ▼] [Full] [Partial] [None]
   ```

4. **Search Tips:**
   - **By Student:** Type student name or ID
   - **By Sponsor:** Type sponsor name or ID
   - **By LoG ID:** Type LOG-XXXX-XX
   - **By Coverage Type:** Use coverage filter

5. **View Results:**
   ```
   Active LoGs (2025-2026): 423 found
   
   ┌─────────────┬────────────────┬──────────────┬──────────────┐
   │ LoG ID      │ Sponsor        │ Student      │ Coverage     │
   ├─────────────┼────────────────┼──────────────┼──────────────┤
   │ LOG-2526-01 │ Global Tech    │ Smith, John  │ Full Medical │
   │ LOG-2526-02 │ ABC Corp       │ Doe, Jane    │ Tuition Only │
   │ LOG-2526-03 │ XYZ Holdings   │ Brown, Bob   │ Partial      │
   └─────────────┴────────────────┴──────────────┴──────────────┘
   ```

6. **Click LoG ID:**
   - View complete coverage details
   - See coverage rules
   - Review special instructions

### Workflow 4: View LoG Details (Coverage Rules)

**URL:** `/Portal/Details/{id}` (Read-only view for Cashiers)

**LoG Details Page:**

```
┌──────────────────────────────────────────────────────────┐
│ LOG-2526-01                                    🟢 Active  │
├──────────────────────────────────────────────────────────┤
│ School Year:    2025-2026                                │
│ Sponsor:        SP-12345 | Global Tech Corporation       │
│ Student:        123456 | Smith, John - Grade 10          │
│                                                          │
│ Contact for Billing Questions:                           │
│   John Smith                                             │
│   john.smith@globaltech.com                             │
│   +63 2 1234 5678                                        │
│                                                          │
│ Coverage Period:                                         │
│   Effective: August 1, 2025                              │
│   Expires:   June 30, 2026                               │
│                                                          │
│ Coverage Type:  Full Medical                             │
│                                                          │
│ COVERAGE RULES (for Billing Reference):                  │
│ ┌──────────────────┬─────────────────┬──────────────┐  │
│ │ Item Category    │ Coverage Level  │ % Covered    │  │
│ ├──────────────────┼─────────────────┼──────────────┤  │
│ │ Medical          │ ✅ Covered      │ 100%         │  │
│ │ Dental           │ ✅ Covered      │ 100%         │  │
│ │ Tuition          │ ✅ Covered      │ 100%         │  │
│ │ Books/Materials  │ ✅ Covered      │ 100%         │  │
│ │ Activities       │ ✅ Covered      │ 100%         │  │
│ │ Uniforms         │ ✅ Covered      │ 100%         │  │
│ └──────────────────┴─────────────────┴──────────────┘  │
│                                                          │
│ Special Instructions:                                    │
│   • All medical expenses covered except cosmetic         │
│   • Pre-authorization required for expenses >PHP 5,000   │
│   • Contact sponsor before scheduling dental procedures  │
│                                                          │
│ [Print Coverage Summary] [Export to PDF]                 │
└──────────────────────────────────────────────────────────┘
```

**How to Read Coverage Rules:**

✅ **Covered (100%):**
- Sponsor pays 100% of charge
- Bill sponsor directly
- No family payment required

⚖️ **Split Coverage (%):**
- Sponsor pays specified percentage
- Family pays remaining percentage
- Split bill accordingly
- Example: 80% sponsor, 20% family

❌ **Not Covered:**
- Sponsor pays 0%
- Family pays 100%
- Bill family only
- Don't include in sponsor billing

**Coverage Rule Examples:**

**Example 1: Full Medical (Everything Covered)**
```
Medical:      100% covered
Dental:       100% covered
Tuition:      100% covered
Books:        100% covered
Activities:   100% covered

→ Bill sponsor for ALL charges
```

**Example 2: Medical Only**
```
Medical:      100% covered
Dental:       100% covered
Tuition:      NOT covered
Books:        NOT covered
Activities:   NOT covered

→ Bill sponsor for medical/dental only
→ Bill family for tuition, books, activities
```

**Example 3: Cost Sharing**
```
Medical:      80% sponsor, 20% family
Dental:       50% sponsor, 50% family
Tuition:      100% covered
Books:        75% sponsor, 25% family
Activities:   NOT covered

→ Split bills according to percentages
→ Bill family for activities
```

---

## Coverage Verification

### Workflow 5: Verify Coverage for Specific Charge

**Scenario:** Parent at cashier window, need to verify if charge is covered

**Step-by-Step:**

1. **Get Information from Parent:**
   - Student name or ID
   - Type of charge (medical, tuition, etc.)
   - Amount of charge

2. **Search for LoG:**
   - Portal → Quick Search
   - Type student name or ID
   - Find active LoG for current school year

3. **Check Coverage Rules:**
   - Click LoG ID
   - Review Coverage Rules table
   - Find matching item category

4. **Determine Coverage:**
   
   **If 100% Covered:**
   ```
   "This charge is fully covered by [Sponsor Name].
    We will bill the sponsor directly. No payment needed from you."
   ```
   
   **If Split Coverage:**
   ```
   "This charge is partially covered by [Sponsor Name].
    Sponsor pays [X]%, you pay [Y]%.
    Your portion is: PHP [amount]"
   ```
   
   **If Not Covered:**
   ```
   "This charge is not covered by [Sponsor Name].
    Full payment required: PHP [amount]"
   ```

5. **Check Special Instructions:**
   - Review Special Instructions section
   - Look for:
     - Pre-authorization requirements
     - Spending limits
     - Exclusions
     - Contact requirements

6. **Example Special Instructions:**
   ```
   "Pre-authorization required for medical >PHP 5,000"
   
   If charge > 5,000:
   → "This charge requires pre-authorization from sponsor.
      Contact [sponsor contact] before proceeding."
   ```

### Workflow 6: Quick Coverage Lookup

**For Fast Service at Cashier:**

**Step 1: Memorize Common Students**
- Keep list of frequently-seen sponsored students
- Note their sponsor and coverage type
- Quick reference for common scenarios

**Step 2: Use Quick Search**
- Have Portal open in browser tab
- Quick search by student name
- View coverage instantly

**Step 3: Print Coverage Summaries**
- Print coverage summaries for frequent students
- Keep at cashier desk
- Update when LoGs change

**Step 4: Create Cheat Sheet**
- Sponsor Name → Coverage Type
- Example:
  ```
  Global Tech Corp → Full Medical (everything covered)
  ABC Corp → Tuition Only
  XYZ Holdings → Medical 80/20 split
  ```

---

## Reports

### Cashier Reports Overview

**URL:** `/CashierReports/Index`

As Cashier, you have access to reconciliation and coverage reports.

### Available Reports

#### 1. Sponsor Billing Reconciliation Report

**What It Shows:**
- All charges billed to sponsors
- By sponsor, by student, by charge type
- By date range

**Filters:**
- School year
- Date range (billing date)
- Sponsor (specific or all)
- Student (specific or all)
- Charge category

**Usage:**
- Reconcile sponsor billings
- Verify charges sent to sponsors
- Month-end closing
- Sponsor statement preparation

**Export:** CSV, Excel, PDF

---

#### 2. Coverage Summary Report

**What It Shows:**
- All active LoGs
- Coverage breakdown by sponsor
- Coverage type distribution

**Filters:**
- School year
- Sponsor
- Coverage type

**Usage:**
- Quick reference for coverage
- Print for desk reference
- Share with billing team

**Export:** CSV, Excel, PDF

---

#### 3. Sponsor Contact List

**What It Shows:**
- All active sponsors
- Contact information
- Email and phone

**Filters:**
- Active/Inactive
- Has active LoGs

**Usage:**
- Billing inquiries
- Send statements
- Communication

**Export:** CSV, Excel

---

#### 4. Student Coverage List

**What It Shows:**
- Students with active LoGs
- Sponsor for each student
- Coverage type
- Quick reference

**Filters:**
- School year
- Grade level
- Sponsor

**Usage:**
- Quick lookup at cashier
- Print for desk reference
- Verify coverage

**Export:** CSV, Excel

---

### Workflow 7: Generate Reconciliation Report

**Scenario:** Month-end reconciliation of sponsor billings

**Step-by-Step:**

1. **Navigate to Reports:**
   - Main Menu → Reports → Cashier Reports

2. **Select "Sponsor Billing Reconciliation Report"**

3. **Configure Filters:**
   ```
   Sponsor Billing Reconciliation Report
   
   Date Range*:
   From: [04/01/2026]  ← Start of month
   To:   [04/30/2026]  ← End of month
   
   Sponsor:     [All Sponsors ▼]  or select specific sponsor
   
   School Year: [2025-2026 ▼]
   
   Category:    [All ▼] [Medical] [Dental] [Tuition] [Other]
   
   [Clear Filters] [Generate Report]
   ```

4. **Generate Report:**
   - Click "Generate Report"
   - Wait for processing (may take 10-30 seconds)

5. **Review Results:**
   ```
   Sponsor Billing Reconciliation Report
   Period: April 1-30, 2026
   Generated: April 30, 2026 4:45 PM
   
   Summary:
   Total Sponsors Billed: 42
   Total Amount Billed:   PHP 1,245,678.00
   Total Transactions:    234
   
   Breakdown by Sponsor:
   ┌────────────┬──────────────────┬──────────────┬────────────┐
   │ Sponsor ID │ Sponsor Name     │ Transactions │ Total Amt  │
   ├────────────┼──────────────────┼──────────────┼────────────┤
   │ SP-12345   │ Global Tech Corp │ 15           │ 125,450.00 │
   │ SP-12346   │ ABC Corporation  │ 8            │  45,230.00 │
   │ ...        │ ...              │ ...          │ ...        │
   └────────────┴──────────────────┴──────────────┴────────────┘
   
   Showing 1-25 of 42 sponsors
   ```

6. **Drill Down (Optional):**
   - Click sponsor name to see transactions
   - View individual charges
   - Verify amounts

7. **Export:**
   - Click "Export to Excel"
   - File downloads
   - Open in Excel
   - Use for reconciliation

8. **Reconciliation Steps:**
   - Compare report totals with billing system
   - Verify all charges included
   - Check for missing transactions
   - Investigate discrepancies
   - Document reconciliation

---

## Common Workflows

### Workflow 8: Answer Parent Billing Question

**Scenario:** Parent asks "Is tuition covered by our sponsor?"

**Step-by-Step:**

1. **Get Student Information:**
   - "What is your child's name?"
   - Or: "What is your child's student ID?"

2. **Search for LoG:**
   - Portal → Quick Search
   - Type student name/ID
   - Find active LoG

3. **Review Coverage:**
   - Click LoG ID
   - Look at Coverage Rules
   - Find "Tuition" row

4. **Respond:**
   
   **If Tuition Covered:**
   ```
   "Yes, tuition is fully covered by [Sponsor Name].
    You don't need to pay tuition fees.
    We will bill the sponsor directly."
   ```
   
   **If Tuition Not Covered:**
   ```
   "No, tuition is not covered by your sponsor.
    [Sponsor Name] covers [list what IS covered].
    Tuition payment is your responsibility."
   ```
   
   **If Tuition Partially Covered:**
   ```
   "Tuition is partially covered by [Sponsor Name].
    Sponsor pays [X]%, you pay [Y]%.
    Your portion of tuition is: PHP [amount]"
   ```

5. **Provide Sponsor Contact (if needed):**
   - "For questions about coverage, contact [sponsor]:"
   - Name: [Contact Person]
   - Email: [Contact Email]
   - Phone: [Contact Phone]

### Workflow 9: Process Split-Coverage Charge

**Scenario:** Medical charge of PHP 10,000, covered 80/20

**Step-by-Step:**

1. **Verify Coverage:**
   - Look up student's LoG
   - Confirm: Medical 80% sponsor, 20% family

2. **Calculate Amounts:**
   ```
   Total Charge:    PHP 10,000.00
   Sponsor (80%):   PHP  8,000.00
   Family (20%):    PHP  2,000.00
   ```

3. **Check Special Instructions:**
   - Look for pre-authorization requirements
   - Check spending limits
   - Verify if sponsor contact needed

4. **If Pre-Authorization Required:**
   ```
   "This charge requires sponsor pre-authorization.
    Please contact [sponsor name] for approval:
    [Contact information]
    
    Once approved, your portion is PHP 2,000.00"
   ```

5. **If No Pre-Authorization:**
   ```
   "Total charge: PHP 10,000.00
    Sponsor covers: PHP 8,000.00 (80%)
    Your payment: PHP 2,000.00 (20%)
    
    Please pay PHP 2,000.00 now.
    We'll bill sponsor for PHP 8,000.00"
   ```

6. **Process Payment:**
   - Collect PHP 2,000.00 from family
   - Issue receipt for family portion
   - Flag charge for sponsor billing (PHP 8,000.00)

7. **Document:**
   - Note split billing in system
   - Attach to charge record
   - Include for reconciliation report

### Workflow 10: Generate Daily Coverage Reference

**Scenario:** Print daily reference list for cashier desk

**Step-by-Step:**

1. **Morning Routine:**
   - Navigate to Reports → Cashier Reports
   - Select "Student Coverage List"

2. **Configure:**
   ```
   Student Coverage List
   
   School Year:  [2025-2026 ▼]  (current)
   Grade Level:  [All ▼]  or specific grade if needed
   Status:       [Active LoGs only ▼]
   
   Sort By:      [Student Last Name ▼]
   
   [Generate Report]
   ```

3. **Generate:**
   - Click "Generate Report"
   - Wait for results

4. **Review:**
   ```
   Student Coverage List (2025-2026)
   Active LoGs: 423
   
   ┌────────────┬──────────────┬────────────────┬──────────────┐
   │ Student    │ Grade │ Sponsor       │ Coverage        │
   ├────────────┼───────┼───────────────┼─────────────────┤
   │ Brown, Bob │ 10    │ XYZ Holdings  │ Partial Medical │
   │ Doe, Jane  │ 11    │ ABC Corp      │ Tuition Only    │
   │ Smith, John│ 10    │ Global Tech   │ Full Medical    │
   └────────────┴───────┴───────────────┴─────────────────┘
   ```

5. **Print:**
   - Click "Print"
   - Or: Export to PDF, then print
   - Keep at cashier desk

6. **Update:**
   - Generate new list weekly
   - Or when LoGs change
   - Replace old list

7. **Usage:**
   - Quick reference during transactions
   - Reduces search time
   - Faster service for parents

---

## Troubleshooting

### Common Issues & Solutions

#### Issue: Cannot Find Student's LoG

**Cause:** No active LoG, or wrong school year selected

**Solution:**
1. Verify correct school year selected
2. Check if student has any LoG (search by student name)
3. Check status filters (ensure "Active" selected)
4. If no LoG found: Student may not be sponsored
5. Inform parent: "I don't see an active Letter of Guarantee for your child. You may need to contact the sponsor or Admissions office."

---

#### Issue: Coverage Information Unclear

**Scenario:** Coverage rules don't clearly state if charge is covered

**Solution:**
1. Review Special Instructions section
2. Look for similar charge category
3. If still unclear:
   - Contact Admissions: admissions@ism.edu.ph
   - Or: Contact sponsor directly (contact info in LoG)
   - Ask for clarification
4. Document clarification for future reference

---

#### Issue: Parent Disputes Coverage

**Scenario:** Parent says sponsor should cover charge, but LoG says not covered

**Solution:**
1. Show parent the LoG details on screen
2. Point to Coverage Rules table
3. Explain: "According to the Letter of Guarantee from [sponsor], this charge category is [covered/not covered/partially covered]."
4. If parent insists:
   - Provide sponsor contact information
   - Suggest parent contact sponsor directly
   - Do NOT override LoG rules
   - Document interaction

---

#### Issue: Report Shows No Data

**Cause:** Filters too restrictive or date range incorrect

**Solution:**
1. Clear all filters
2. Select broader date range
3. Change sponsor to "All"
4. Verify correct school year
5. If still no data: May be no transactions in that period

---

#### Issue: LoG Expired

**Scenario:** LoG shows "Past" or "Inactive" status

**Solution:**
1. Check Expiry Date in LoG details
2. If expired:
   - Look for new LoG for current school year
   - If no new LoG: Coverage may have ended
3. Inform parent:"The Letter of Guarantee for your child has expired.
    Please contact Admissions or your sponsor to renew coverage."

---

#### Issue: Multiple LoGs for Same Student

**Scenario:** Student has LoGs from different sponsors

**Solution:**
1. This is normal—student can have multiple sponsors
2. Ask parent which sponsorship for this charge
3. Select appropriate LoG
4. Process according to that LoG's coverage rules
5. If parent unsure, show list of sponsors and coverage types

---

## FAQs

### General Questions

**Q: What is my role as Cashier?**
A: You verify sponsor coverage for billing purposes and generate reconciliation reports. You have read-only access to sponsor and LoG information.

---

**Q: Can I edit sponsor or LoG information?**
A: No, you have read-only access. Contact Admissions if you notice incorrect information.

---

**Q: How do I change my password?**
A: Click your name (top-right) → Profile → Change Password.

---

**Q: Who do I contact if I have questions?**
A: 
- Coverage questions: Admissions (admissions@ism.edu.ph)
- System issues: Technical Support (support@ism.edu.ph)
- Billing process: Your supervisor

---

### Coverage Questions

**Q: What if a charge is not explicitly listed in coverage rules?**
A: 
- Look for similar category
- Check Special Instructions for exclusions
- If unclear, contact Admissions or sponsor for clarification
- When in doubt, assume NOT covered and verify

---

**Q: Can I override coverage rules if parent insists?**
A: No, always follow LoG coverage rules. If parent disputes, refer them to sponsor or Admissions. Do not make exceptions without proper authorization.

---

**Q: What if coverage percentage doesn't add to 100%?**
A: Coverage percentage is sponsor's portion. Family pays remainder.
- Example: 80% coverage = sponsor pays 80%, family pays 20%

---

**Q: What does "Pre-authorization required" mean?**
A: Sponsor must approve charge before it's incurred. Parent should contact sponsor first. Don't process charge without authorization confirmation.

---

### Billing Questions

**Q: How do I know if charge was already billed to sponsor?**
A: Check your billing system records. ISM Sponsor System shows coverage rules but doesn't track actual billings. Use Reconciliation Report to verify billings.

---

**Q: What if parent paid but sponsor should have been billed?**
A: 
1. Verify coverage in LoG
2. If sponsor should have been billed:
   - Process refund per your procedures
   - Bill sponsor instead
   - Document for auditing

---

**Q: Can student have multiple sponsors?**
A: Yes, student can have LoGs from different sponsors. Each LoG defines separate coverage from that sponsor.

---

**Q: What if sponsor and LoG information don't match billing system?**
A: ISM Sponsor System is the authoritative source for coverage rules. If discrepancy exists:
1. Use ISM Sponsor coverage rules
2. Report discrepancy to Admissions
3. Update billing system accordingly

---

### Report Questions

**Q: How often should I generate reconciliation reports?**
A: Typically monthly for closing, but can generate anytime needed for auditing or verification.

---

**Q: Can I see billings from other cashiers?**
A: Reports show all billings, regardless of which cashier processed them. This is for complete reconciliation.

---

**Q: What if report totals don't match my billing records?**
A: 
1. Verify date ranges match
2. Check filters (all sponsors included?)
3. Generate detailed report to find discrepancies
4. Report issues to Admissions or IT Support

---

**Q: Can I schedule reports to run automatically?**
A: Not currently. Generate reports manually as needed. Feature planned for future.

---

### Technical Questions

**Q: What browsers are supported?**
A: Chrome, Edge, Firefox (latest versions). Avoid Internet Explorer.

---

**Q: Can I use on my phone?**
A: Limited mobile support. Best experience on desktop/laptop. For quick lookups, mobile works, but reporting requires desktop.

---

**Q: Why is system slow?**
A: 
- Clear browser cache
- Check internet connection
- Close unused browser tabs
- Report persistent slowness to IT Support

---

**Q: What if I get an error?**
A: 
1. Screenshot error message
2. Note what you were doing
3. Click your name → Feedback → Report Bug
4. Or: Contact IT Support

---

## Conclusion

As a Cashier, you play an essential role in accurately processing sponsor-covered charges and maintaining proper billing records.

### Key Takeaways

✅ **Your Focus:**
- Verify coverage before processing charges
- Follow LoG coverage rules precisely
- Generate accurate reconciliation reports
- Provide excellent service to parents

✅ **Best Practices:**
- Always verify correct school year
- Read coverage rules carefully
- Check Special Instructions for conditions
- When unsure, verify before processing
- Document unusual or disputed charges
- Generate regular reconciliation reports

✅ **Quick Reference:**
- Keep printed coverage list at desk
- Update list weekly or when changes occur
- Have Portal open for quick searches
- Know common sponsors and coverage types

✅ **Communication:**
- Explain coverage clearly to parents
- Provide sponsor contact for disputes
- Refer complex questions to Admissions
- Document interactions

### Daily Cashier Checklist

**Start of Day:**
- [ ] Login to system
- [ ] Check for system updates/notices
- [ ] Print/review daily coverage reference
- [ ] Open Portal for quick searches

**During Day:**
- [ ] Look up coverage for each transaction
- [ ] Verify coverage rules before processing
- [ ] Check Special Instructions
- [ ] Document split billings
- [ ] Answer parent questions

**End of Day:**
- [ ] Review day's sponsor-covered transactions
- [ ] Flag any issues or disputes
- [ ] Note LoGs needing clarification

**Month-End:**
- [ ] Generate reconciliation report
- [ ] Compare with billing system records
- [ ] Document discrepancies
- [ ] Submit to supervisor

### Getting Help

**Resources:**
- This User Manual
- In-app help (? icon)
- Your Name → Feedback
- Printed coverage reference lists
- Colleague support

**Support Contacts:**
- Admissions: admissions@ism.edu.ph (coverage questions)
- IT Support: support@ism.edu.ph (system issues)
- Your Supervisor: billing/process questions

---

**Document Information:**
- **Version:** 1.0
- **Last Updated:** April 23, 2026
- **Document Owner:** ISM Sponsor System Team
- **Feedback:** Submit via Your Name → Feedback

---

**Thank you for your attention to accuracy in processing sponsor-covered charges!**
