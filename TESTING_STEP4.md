# Step 4: Sponsor Request and Approval Workflow - Testing Guide

## Overview
Step 4 implements a complete change request workflow where Sponsor users can submit change requests for their profile fields, and Admin/Admissions staff can review, approve/reject, and apply approved changes to the sponsor master data.

## Test User Accounts

### Admin User
- **Username:** admin
- **Password:** admin
- **Role:** admin
- **Permissions:** Full access to review queue, can approve, reject, and apply changes

### Admissions User
- **Username:** admissions
- **Password:** admissions
- **Role:** admissions
- **Permissions:** Can review and approve/reject requests, may apply based on configuration

### Cashier User
- **Username:** cashier
- **Password:** cashier
- **Role:** cashier
- **Permissions:** Dashboard view only, no approval rights

### Sponsor Users
**Note:** Sponsor users need to be created through the admin interface with linked SponsorId

## Test Scenarios

### 1. Sponsor: Submit Change Request (UT07)

**Prerequisites:**
- Logged in as sponsor user with linked SponsorId
- Navigate to Sponsor Profile or Dashboard

**Steps:**
1. From Dashboard, click on "Pending Sponsor Change Requests" section or navigate to `/SponsorRequest/Index`
2. Click "Request Change" button
3. Select field to update from dropdown (SponsorName, LegalName, Address, Tin)
4. Observe current value displayed automatically
5. Enter requested value (must be different from current)
6. Optionally enter reason for change
7. Click "Submit Request"

**Expected Result:**
- Success message displayed: "Request submitted successfully"
- Redirected to request details page showing:
  - Request ID
  - Sponsor name
  - Field being changed
  - Current vs Requested value
  - Status: Pending
  - Submitted by and timestamp
- Activity log entry created
- Request appears in sponsor's "My Change Requests" list

**Verify:**
```sql
SELECT * FROM SponsorChangeRequests 
WHERE Status = 'Pending' 
ORDER BY SubmittedOn DESC;

SELECT * FROM ActivityLogs 
WHERE Item = 'SponsorRequest' 
ORDER BY Date DESC;
```

### 2. Sponsor: View Own Request Status

**Steps:**
1. Navigate to `/SponsorRequest/Index`
2. View list of submitted requests with statuses
3. Click "View" on any request
4. Review request details

**Expected Result:**
- All requests for linked sponsor displayed
- Can view details including review notes if reviewed
- Cannot view other sponsors' requests
- Can cancel pending requests

### 3. Sponsor: Cancel Pending Request

**Steps:**
1. Navigate to pending request details
2. Click "Cancel Request" button
3. Confirm cancellation

**Expected Result:**
- Status updated to Cancelled
- Activity log entry created
- Cannot be reactivated

### 4. Sponsor: Duplicate Request Validation

**Steps:**
1. Submit a change request for "Address"
2. Try to submit another change request for "Address" while first is still Pending

**Expected Result:**
- Error message: "A pending request already exists for Address"
- Request not created
- First request remains Pending

### 5. Sponsor: Unchanged Value Validation

**Steps:**
1. Start creating change request
2. Select field
3. Enter requested value identical to current value
4. Submit

**Expected Result:**
- Error message: "Requested value is the same as current value"
- Request not created

### 6. Admin/Admissions: View Review Queue

**Prerequisites:**
- Logged in as admin or admissions
- At least one pending request exists

**Steps:**
1. Navigate to Dashboard
2. Observe "Pending Sponsor Requests" count in stat card
3. Click "View All Pending" or navigate to `/ReviewRequest/Index`
4. Use filter controls:
   - Filter by Status (Pending, Approved, Applied, Rejected)
   - Search by sponsor name or ID
   - Clear filters

**Expected Result:**
- All pending requests visible in table with:
  - Request ID, Sponsor, Field, Current Value, Requested Value
  - Status badge with color coding
  - Submitted by and date
  - "Review" action link
- Filters narrow results correctly
- Search matches sponsor name and ID

### 7. Admin/Admissions: Review Request Details

**Steps:**
1. From review queue, click "Review" on pending request
2. Examine request information:
   - Sponsor details with link to profile
   - Field being changed
   - Current value (gray background)
   - Requested value (blue background)
   - Reason for change (yellow background if provided)

**Expected Result:**
- All request details clearly displayed
- Sponsor link navigates to sponsor profile
- Review action form visible with notes field and approve/reject buttons

### 8. Admin/Admissions: Approve Request

**Steps:**
1. Open pending request details
2. Enter optional review notes
3. Click "Approve" button
4. Confirm approval

**Expected Result:**
- Success message: "Request approved successfully"
- Status updated to Approved
- Reviewed by and reviewed on timestamp populated
- Review notes saved
- Activity log entry created
- Dashboard pending count decremented
- "Apply Change to Master Data" button now visible (Admin only)

**Verify:**
```sql
SELECT RequestId, Status, ReviewedByUserDisplay, ReviewedOn, ReviewNotes
FROM SponsorChangeRequests
WHERE RequestId = [your_request_id];
```

### 9. Admin/Admissions: Reject Request

**Steps:**
1. Open pending request details
2. Enter review notes (recommended for rejections)
3. Click "Reject" button
4. Confirm rejection

**Expected Result:**
- Success message: "Request rejected successfully"
- Status updated to Rejected
- Reviewed by and reviewed on timestamp populated
- Review notes saved and visible
- Activity log entry created
- No further action possible on this request
- Sponsor can see rejection and notes

### 10. Admin: Apply Approved Request (UT08)

**Prerequisites:**
- Logged in as admin
- Request status is Approved

**Steps:**
1. Navigate to approved request details
2. Click "Apply Change to Master Data" button
3. Confirm application

**Expected Result:**
- Success message: "Request applied successfully. Sponsor master data has been updated."
- Status updated to Applied
- Applied by and applied on timestamp populated
- Applied value recorded
- **Sponsor master record actually updated** with new value
- Activity log entry with before/after values
- Green alert indicates change applied

**Critical Verification:**
```sql
-- Verify request status
SELECT RequestId, Status, AppliedByUserDisplay, AppliedOn, AppliedValue
FROM SponsorChangeRequests
WHERE RequestId = [your_request_id];

-- Verify sponsor master data updated
SELECT SponsorId, SponsorName, LegalName, Address, Tin
FROM Sponsors
WHERE SponsorId = [sponsor_id];

-- Check activity log
SELECT Date, Item, Details, UserDisplay
FROM ActivityLogs
WHERE Item = 'SponsorRequest'
ORDER BY Date DESC;
```

**Example:** If request was to change Address from "123 Road" to "456 New Avenue", the Sponsors.Address field should now contain "456 New Avenue".

### 11. Admin: Cannot Apply Pending Request

**Steps:**
1. Navigate to pending request (not yet approved)
2. Examine available actions

**Expected Result:**
- No "Apply" button visible
- Only approval/rejection actions available
- Cannot force-apply unapproved request

### 12. Authorization: Sponsor Cannot Review

**Steps:**
1. Log in as sponsor user
2. Try to navigate to `/ReviewRequest/Index` directly

**Expected Result:**
- 403 Forbidden or redirect to Access Denied page
- Cannot access review queue
- Cannot approve/reject requests

### 13. Authorization: Admissions Can Review

**Steps:**
1. Log in as admissions user
2. Navigate to review queue `/ReviewRequest/Index`
3. Approve or reject a pending request

**Expected Result:**
- Full access to review queue
- Can approve and reject requests
- Activity logs show admissions user as reviewer

### 14. Authorization: Admin Only Apply (Optional)

**Note:** Based on configuration, you may allow admissions to apply or restrict to admin only.

**Steps:**
1. Log in as admissions user
2. Navigate to approved request details
3. Check if "Apply" button is visible

**Expected Result:**
- If restricted: No apply button, only admin can apply
- If allowed: Admissions can apply changes

### 15. Dashboard Integration: Staff View

**Prerequisites:**
- Logged in as admin or admissions
- At least 3 pending requests exist

**Steps:**
1. Navigate to Dashboard
2. Observe "Pending Sponsor Requests" stat card
3. Scroll to "Pending Sponsor Change Requests" table
4. Click "View All Pending" link

**Expected Result:**
- Stat card shows correct pending count
- Table displays up to 5 recent pending requests
- Table shows Request ID, Sponsor, Field, Status, Submitted On, Action
- "View All Pending" link includes count and navigates to review queue

### 16. Dashboard Integration: Sponsor View

**Prerequisites:**
- Logged in as sponsor user
- At least 1 request submitted

**Steps:**
1. Navigate to Dashboard
2. Observe "My Change Requests" section
3. View recent requests
4. Click "View" link

**Expected Result:**
- Table labeled "My Change Requests"
- Shows only this sponsor's requests
- Includes all statuses (Pending, Approved, Applied, Rejected, Cancelled)
- Links navigate to sponsor's request detail view

### 17. Audit Trail Verification

**For each action:**
- Request submission
- Request approval
- Request rejection
- Request applied
- Request cancelled

**Verify:**
```sql
SELECT 
    Date,
    Item,
    Details,
    UserDisplay,
    RoleName,
    SchoolYearId
FROM ActivityLogs
WHERE Item = 'SponsorRequest'
ORDER BY Date DESC;
```

**Expected Log Entries:**
- "Change request submitted for [Sponsor]: [Field]"
- "Change request #[ID] approved for [Sponsor]: [Field]"
- "Change request #[ID] rejected for [Sponsor]: [Field]"
- "Change request #[ID] applied for [Sponsor]: [Field]. Changed from '[old]' to '[new]'"
- "Change request #[ID] cancelled for [Sponsor]: [Field]"

### 18. Field Types Coverage

Test each requestable field:

**SponsorName:**
- Current: "ACME Corp"
- Requested: "ACME Corporation Inc."

**LegalName:**
- Current: "ACME Corporation"
- Requested: "ACME Corporation Limited"

**Address:**
- Current: "123 Road"
- Requested: "456 New Ave, Manila 1000"

**Tin:**
- Current: "123-456"
- Requested: "123-456-789-000"

**Expected Result:**
- All fields supported in dropdown
- Current values retrieved correctly for each field
- Approval and application work for all field types

### 19. Status Transitions

**Valid Transitions:**
- Pending → Approved (Admin/Admissions)
- Pending → Rejected (Admin/Admissions)
- Pending → Cancelled (Sponsor)
- Approved → Applied (Admin)

**Invalid Transitions:**
- Applied → anything (final state)
- Rejected → Applied (cannot apply rejected)
- Cancelled → Applied (cannot apply cancelled)

**Test:**
1. Try to apply a rejected request
2. Try to edit status manually (should fail due to validation)

### 20. Search and Filter

**Review Queue Search:**
- Search by sponsor name: "ACME" → shows only ACME requests
- Search by sponsor ID: "XYZBANK" → shows only XYZBANK requests
- Search by field: "Address" → filters results

**Status Filter:**
- Filter by "Pending" → shows only pending
- Filter by "Approved" → shows only approved
- Filter by "Applied" → shows only applied
- Filter by "Rejected" → shows only rejected

**Clear Filters:**
- Click "Clear" → returns to unfiltered view

### 21. Request Details: Complete Information

**For each request, verify details page shows:**
- Request ID
- Sponsor ID and Name (with link to profile)
- Field being changed
- Current value
- Requested value
- Reason (if provided)
- Status badge with color
- Submitted by user and timestamp
- Reviewed by user and timestamp (if reviewed)
- Review notes (if provided)
- Applied by user and timestamp (if applied)
- Applied value (if applied)
- Appropriate action buttons based on status and role

### 22. Sponsor Profile Integration

**Steps:**
1. Log in as sponsor user
2. Navigate to Sponsor Profile page
3. Observe "Request Change" button
4. Click button

**Expected Result:**
- Button visible in button group
- Clicking navigates to `/SponsorRequest/Create`
- Form pre-populated with sponsor context

### 23. End-to-End Workflow

**Complete Flow:**
1. Sponsor submits request for Address change (Pending)
2. Admin views request in queue
3. Admin approves request (Approved)
4. Admin applies request (Applied)
5. Sponsor master data updated
6. Sponsor views applied request and sees new value

**Verify at each step:**
- Status updates correctly
- Timestamps populated
- User information captured
- Activity logs created
- Dashboard counts update
- Master data changes only on Apply

### 24. Error Handling

**Test error scenarios:**

**Missing Required Fields:**
- Submit without selecting field
- Submit without entering requested value

**Validation Errors:**
- Duplicate pending request
- Unchanged value
- Invalid sponsor ID

**Authorization Errors:**
- Sponsor trying to access review queue
- Unauthenticated user accessing any page
- Sponsor accessing another sponsor's requests

**Business Logic Errors:**
- Apply request that isn't approved
- Approve already-reviewed request

### 25. Performance and UX

**Check:**
- Page load times < 2 seconds
- No JavaScript errors in console
- Responsive layout on different screen sizes
- Status badges color-coded correctly:
  - Pending: Yellow/Warning
  - Approved: Blue/Info
  - Applied: Green/Success
  - Rejected: Red/Danger
  - Cancelled: Gray/Secondary
- Form validation messages clear and user-friendly
- Confirmation dialogs for destructive actions
- Success/error messages display prominently

## Database State Verification

**After running tests, check final state:**

```sql
-- Count requests by status
SELECT Status, COUNT(*) as Count
FROM SponsorChangeRequests
GROUP BY Status;

-- Recent activity
SELECT TOP 10 *
FROM ActivityLogs
WHERE Item = 'SponsorRequest'
ORDER BY Date DESC;

-- Applied changes reflected in master data
SELECT s.SponsorId, s.SponsorName, s.Address, s.Tin,
       COUNT(scr.RequestId) as TotalRequests,
       SUM(CASE WHEN scr.Status = 'Applied' THEN 1 ELSE 0 END) as AppliedRequests
FROM Sponsors s
LEFT JOIN SponsorChangeRequests scr ON s.SponsorId = scr.SponsorId
GROUP BY s.SponsorId, s.SponsorName, s.Address, s.Tin;
```

## Known Limitations (Step 4)

1. Contact field changes (ContactName, ContactEmail, ContactPhone) are defined in enum but not yet implemented in UI/service
2. Attachment upload for requests not yet implemented
3. Status history tracking not implemented (only current status stored)
4. Bulk approval actions not available
5. Email notifications not configured
6. No deadline/SLA tracking for pending requests

## Next Steps (Step 5+)

- Contact field change support
- Request attachments
- Email notifications
- Status history audit
- Bulk operations
- SLA tracking and alerts
- Advanced reporting
