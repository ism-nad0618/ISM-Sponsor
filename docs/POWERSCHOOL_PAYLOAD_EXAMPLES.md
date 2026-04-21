# PowerSchool Sponsor_OrgName - Example Payloads & Outputs

## 📤 Example Request Payloads

### Scenario 1: Initial School Setup (10 Sponsors)

#### Database State
```sql
SELECT SponsorId, SponsorName, IsActive, ApprovalStatus, IsMerged
FROM Sponsors
ORDER BY SponsorName;
```

| SponsorId | SponsorName | IsActive | ApprovalStatus | IsMerged |
|-----------|-------------|----------|----------------|----------|
| ABC001 | ABC Company | true | Approved | false |
| ACME001 | ACME Corporation | true | Approved | false |
| BANK001 | First National Bank | true | Approved | false |
| EDU001 | Global Education Fund | true | Approved | false |
| PEND001 | Pending Sponsor Inc | true | **PendingApproval** | false |
| REJ001 | Rejected Corp | true | **Rejected** | false |
| INACT001 | Inactive Foundation | **false** | Approved | false |
| MERGE001 | Old Merged Sponsor | true | Approved | **true** |
| TECH001 | Tech Solutions Ltd | true | Approved | false |
| XYZ001 | XYZ Bank | true | Approved | false |

#### PowerSchool Sponsor_OrgName Popup Payload
```
ABC Company
ACME Corporation
First National Bank
Global Education Fund
Tech Solutions Ltd
XYZ Bank
```

**Notes:**
- ✅ 6 sponsors included (active + approved + not merged)
- ❌ "Pending Sponsor Inc" excluded (PendingApproval status)
- ❌ "Rejected Corp" excluded (Rejected status)
- ❌ "Inactive Foundation" excluded (IsActive = false)
- ❌ "Old Merged Sponsor" excluded (IsMerged = true)
- ✓ Alphabetically sorted
- ✓ Newline-separated format

---

### Scenario 2: After New Sponsor Created

#### Event Sequence
```
1. Admin creates "Global Tech Foundation" (GTF001)
2. Status: ApprovalStatus = "PendingApproval" (auto-set)
3. PowerSchool publish queued (EventType = "PublishSponsorOrgList:SponsorCreate")
4. Sponsor list generated (GTF001 NOT included - pending approval)
```

#### PowerSchool Payload (Same as Scenario 1)
```
ABC Company
ACME Corporation
First National Bank
Global Education Fund
Tech Solutions Ltd
XYZ Bank
```

**Why no change?** New sponsor is `PendingApproval` - not yet included.

---

#### After Admin Approves "Global Tech Foundation"
```
5. Admin clicks "Approve Sponsor"
6. ApprovalStatus changed to "Approved"
7. PowerSchool publish queued (EventType = "PublishSponsorOrgList:SponsorApprove")
8. Sponsor list regenerated (GTF001 NOW included)
```

#### PowerSchool Payload (Updated)
```
ABC Company
ACME Corporation
First National Bank
Global Education Fund
Global Tech Foundation
Tech Solutions Ltd
XYZ Bank
```

**Change detected**: "Global Tech Foundation" added in alphabetical position.

---

### Scenario 3: Sponsor Name Updated

#### Event Sequence
```
1. Admin renames "ACME Corporation" → "ACME Global Services"
2. PowerSchool publish queued (EventType = "PublishSponsorOrgList:SponsorUpdate")
```

#### PowerSchool Payload (After Update)
```
ABC Company
ACME Global Services
First National Bank
Global Education Fund
Global Tech Foundation
Tech Solutions Ltd
XYZ Bank
```

**Change detected**: "ACME Corporation" replaced with "ACME Global Services".

---

### Scenario 4: Sponsor Deactivated

#### Event Sequence
```
1. Admin deactivates "Tech Solutions Ltd"
2. IsActive set to false
3. PowerSchool publish queued (EventType = "PublishSponsorOrgList:SponsorDeactivate")
```

#### PowerSchool Payload (After Deactivation)
```
ABC Company
ACME Global Services
First National Bank
Global Education Fund
Global Tech Foundation
XYZ Bank
```

**Change detected**: "Tech Solutions Ltd" removed from list.

---

### Scenario 5: Sponsor Merge

#### Event Sequence
```
1. Duplicate detected: "XYZ Bank" and "XYZ Banking Corporation" are same entity
2. Admin merges XYZ002 into XYZ001 (survivor)
3. XYZ002 marked as IsMerged = true
4. XYZ001 remains active
5. PowerSchool publish queued (EventType = "PublishSponsorOrgList:SponsorMerge")
```

#### PowerSchool Payload (After Merge)
```
ABC Company
ACME Global Services
First National Bank
Global Education Fund
Global Tech Foundation
XYZ Bank
```

**Change detected**: "XYZ Banking Corporation" removed (merged sponsor excluded).

---

## 📥 Example SyncLog Entries

### Successful Publish

```json
{
  "SyncLogId": 1523,
  "EntityType": "SponsorMaster",
  "EntityId": "GTF001",
  "TargetSystem": "PowerSchool",
  "EventType": "PublishSponsorOrgList:SponsorApprove",
  "PayloadVersion": "1.0",
  "AttemptedAt": "2026-03-30T14:23:45.123Z",
  "LastSucceededAt": "2026-03-30T14:23:45.456Z",
  "RetryCount": 0,
  "Status": "Succeeded",
  "ErrorMessage": null,
  "CorrelationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "RequestPayload": "ABC Company\nACME Global Services\nFirst National Bank\nGlobal Education Fund\nGlobal Tech Foundation\nTech Solutions Ltd\nXYZ Bank",
  "ResponsePayload": "PowerSchool: Published 7 sponsor names to Sponsor_OrgName custom field popup menu",
  "ExternalReferenceId": "PS-ORGLIST-20260330142345",
  "CreatedOn": "2026-03-30T14:23:45.123Z",
  "ModifiedOn": "2026-03-30T14:23:45.456Z"
}
```

---

### Failed Publish (PowerSchool API Down)

```json
{
  "SyncLogId": 1524,
  "EntityType": "SponsorMaster",
  "EntityId": "TECH001",
  "TargetSystem": "PowerSchool",
  "EventType": "PublishSponsorOrgList:SponsorUpdate",
  "PayloadVersion": "1.0",
  "AttemptedAt": "2026-03-30T15:10:22.789Z",
  "LastSucceededAt": null,
  "RetryCount": 0,
  "Status": "Failed",
  "ErrorMessage": "PowerSchool Custom Field API: Unable to update Sponsor_OrgName field data. CHECK: Field exists and API has write permissions.",
  "CorrelationId": "b2c3d4e5-f6a7-8901-bcde-f2345678901a",
  "RequestPayload": "ABC Company\nACME Global Services\nFirst National Bank\nGlobal Education Fund\nGlobal Tech Foundation\nTech Solutions Update\nXYZ Bank",
  "ResponsePayload": null,
  "ExternalReferenceId": null,
  "CreatedOn": "2026-03-30T15:10:22.789Z",
  "ModifiedOn": "2026-03-30T15:10:23.012Z"
}
```

**Admin Action**: Clicks "Retry" in Integration Retry Dashboard.

---

### Retry Success

```json
{
  "SyncLogId": 1524,
  "EntityType": "SponsorMaster",
  "EntityId": "TECH001",
  "TargetSystem": "PowerSchool",
  "EventType": "PublishSponsorOrgList:SponsorUpdate",
  "PayloadVersion": "1.0",
  "AttemptedAt": "2026-03-30T15:10:22.789Z",
  "LastSucceededAt": "2026-03-30T15:20:15.234Z",
  "RetryCount": 1,
  "Status": "Succeeded",
  "ErrorMessage": null,
  "CorrelationId": "b2c3d4e5-f6a7-8901-bcde-f2345678901a",
  "RequestPayload": "ABC Company\nACME Global Services\nFirst National Bank\nGlobal Education Fund\nGlobal Tech Foundation\nTech Solutions Update\nXYZ Bank",
  "ResponsePayload": "PowerSchool: Published 7 sponsor names to Sponsor_OrgName custom field popup menu",
  "ExternalReferenceId": "PS-ORGLIST-20260330152015",
  "CreatedOn": "2026-03-30T15:10:22.789Z",
  "ModifiedOn": "2026-03-30T15:20:15.567Z"
}
```

---

## 🧹 Sanitization Examples

### Input with Special Characters

| Original Sponsor Name | Sanitized Output |
|----------------------|------------------|
| `"Smith & Associates"` | `"Smith & Associates"` (✓ Allowed) |
| `"O'Brien Corporation"` | `"O'Brien Corporation"` (✓ Allowed) |
| `"Test\nNewline Corp"` | `"Test Newline Corp"` (❌ Newline removed) |
| `"  Extra   Spaces  "` | `"Extra Spaces"` (✓ Normalized) |
| `"\|Pipe Corp\|"` | `"Pipe Corp"` (❌ Leading/trailing pipes removed) |
| `"Sponsor\r\nWith\r\nBreaks"` | `"Sponsor With Breaks"` (❌ All breaks removed) |
| `"Duplicate Inc"` (appears 2x) | `"Duplicate Inc"` (✓ Deduplicated) |

### Live Code Example
```csharp
private string SanitizeSponsorName(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return string.Empty;

    // Trim and normalize whitespace
    name = name.Trim();
    name = System.Text.RegularExpressions.Regex.Replace(name, @"\s+", " ");

    // Remove newlines and carriage returns (would break popup format)
    name = name.Replace("\n", " ").Replace("\r", " ");

    // Remove leading/trailing pipes and semicolons (common delimiters)
    name = name.Trim('|', ';');

    return name;
}
```

---

## 📊 Admin Dashboard Integration

### Integration Retry Dashboard View

#### Filter Settings
```
Status: All
Target System: PowerSchool
Entity Type: SponsorMaster
```

#### Example Table View
| ID | Event Type | Entity ID | Attempted At | Status | Retry Count | Actions |
|----|-----------|-----------|--------------|--------|-------------|---------|
| 1524 | PublishSponsorOrgList:SponsorUpdate | TECH001 | 2026-03-30 15:10:22 | ⚠️ Failed | 0 | [Retry] [Details] |
| 1523 | PublishSponsorOrgList:SponsorApprove | GTF001 | 2026-03-30 14:23:45 | ✅ Succeeded | 0 | [Details] |
| 1522 | PublishSponsorOrgList:SponsorCreate | GTF001 | 2026-03-30 14:20:10 | ✅ Succeeded | 0 | [Details] |
| 1521 | PublishSponsorOrgList:SponsorMerge | XYZ001 | 2026-03-30 12:45:33 | ✅ Succeeded | 0 | [Details] |

#### Details Modal (Click on ID 1524)
```
Sync Log Details
═══════════════════════════════════════════════
Sync Log ID:        1524
Entity Type:        SponsorMaster
Entity ID:          TECH001
Target System:      PowerSchool
Event Type:         PublishSponsorOrgList:SponsorUpdate
Status:             Failed
Attempted At:       2026-03-30 15:10:22
Last Succeeded At:  (null)
Retry Count:        0
Correlation ID:     b2c3d4e5-f6a7-8901-bcde-f2345678901a

Error Message:
PowerSchool Custom Field API: Unable to update Sponsor_OrgName 
field data. CHECK: Field exists and API has write permissions.

Request Payload (sent to PowerSchool):
ABC Company
ACME Global Services
First National Bank
Global Education Fund
Global Tech Foundation
Tech Solutions Update
XYZ Bank

Response Payload:
(null)

[Retry Now] [Close]
```

---

## 🔍 SQL Queries for Troubleshooting

### Get Current Publishable Sponsor List
```sql
-- This matches exactly what the service sends to PowerSchool
SELECT SponsorName
FROM Sponsors
WHERE IsActive = 1
  AND IsMerged = 0
  AND (ApprovalStatus IS NULL OR ApprovalStatus = 'Approved')
ORDER BY SponsorName;
```

### Get Last 10 Publish Attempts
```sql
SELECT TOP 10
    SyncLogId,
    EventType,
    EntityId,
    Status,
    AttemptedAt,
    RetryCount,
    ErrorMessage
FROM SyncLogs
WHERE EntityType = 'SponsorMaster'
  AND TargetSystem = 'PowerSchool'
ORDER BY AttemptedAt DESC;
```

### Get Failed Publishes Needing Retry
```sql
SELECT 
    SyncLogId,
    EventType,
    EntityId,
    AttemptedAt,
    ErrorMessage,
    RetryCount
FROM SyncLogs
WHERE EntityType = 'SponsorMaster'
  AND TargetSystem = 'PowerSchool'
  AND Status = 'Failed'
  AND RetryCount < 5
ORDER BY AttemptedAt DESC;
```

### Get Sponsor List Sent in Specific Publish
```sql
SELECT RequestPayload
FROM SyncLogs
WHERE SyncLogId = 1523;
```

**Result:**
```
ABC Company
ACME Global Services
First National Bank
Global Education Fund
Global Tech Foundation
Tech Solutions Ltd
XYZ Bank
```

---

## 🧪 Testing Data

### Test Database Setup Script
```sql
-- Create test sponsors with various states
INSERT INTO Sponsors (SponsorId, SponsorName, LegalName, IsActive, ApprovalStatus, IsMerged, CreatedOn)
VALUES 
    ('TEST001', 'Active Approved Sponsor', 'Active Approved Sponsor LLC', 1, 'Approved', 0, GETUTCDATE()),
    ('TEST002', 'Pending Approval Sponsor', 'Pending Approval Sponsor Inc', 1, 'PendingApproval', 0, GETUTCDATE()),
    ('TEST003', 'Rejected Sponsor', 'Rejected Sponsor Corp', 1, 'Rejected', 0, GETUTCDATE()),
    ('TEST004', 'Inactive Sponsor', 'Inactive Sponsor LLC', 0, 'Approved', 0, GETUTCDATE()),
    ('TEST005', 'Merged Sponsor', 'Merged Sponsor Inc', 1, 'Approved', 1, GETUTCDATE()),
    ('TEST006', 'Special Chars & Co', 'Special Chars & Company', 1, 'Approved', 0, GETUTCDATE()),
    ('TEST007', 'O''Brien Foundation', 'O''Brien Education Foundation', 1, 'Approved', 0, GETUTCDATE());

-- Expected PowerSchool output (only TEST001, TEST006, TEST007):
-- Active Approved Sponsor
-- O'Brien Foundation
-- Special Chars & Co
```

### Expected Test Payload
```
Active Approved Sponsor
O'Brien Foundation
Special Chars & Co
```

**Verification:**
- ✓ TEST001: Included (active + approved + not merged)
- ✗ TEST002: Excluded (pending approval)
- ✗ TEST003: Excluded (rejected)
- ✗ TEST004: Excluded (inactive)
- ✗ TEST005: Excluded (merged)
- ✓ TEST006: Included (special chars preserved)
- ✓ TEST007: Included (apostrophe preserved)

---

## 🎯 PowerSchool Custom Field Configuration

### Field Setup (PowerSchool Admin)

1. **Navigate To**: Setup > Data Entry > Student Custom Fields
2. **Field Name**: `Sponsor_OrgName`
3. **Field Type**: `Text (255 characters)`
4. **Control Type**: `Popup Menu`
5. **Data Source**: `Static List` (updated via API)
6. **Data for Popup or Radio Buttons**:
```
[This content is automatically managed by ISM Sponsor Management System]
[Do NOT edit manually - changes will be overwritten]

ABC Company
ACME Global Services
First National Bank
Global Education Fund
Global Tech Foundation
Tech Solutions Ltd
XYZ Bank
```

### API Integration Settings
- **Endpoint**: `/ws/v1/custom_fields/student/Sponsor_OrgName`
- **Method**: `PUT`
- **Authentication**: OAuth 2.0 (Client Credentials)
- **Content-Type**: `application/json`
- **Body**:
```json
{
  "field_name": "Sponsor_OrgName",
  "field_type": "popup",
  "options": [
    "ABC Company",
    "ACME Global Services",
    "First National Bank",
    "Global Education Fund",
    "Global Tech Foundation",
    "Tech Solutions Ltd",
    "XYZ Bank"
  ]
}
```

---

## 📋 Deployment Verification

### Checklist After Deployment

1. **Test Sponsor Create**
   ```
   Expected Result: SyncLog entry with EventType = "PublishSponsorOrgList:SponsorCreate"
   ```

2. **Test Sponsor Update**
   ```
   Expected Result: SyncLog entry with EventType = "PublishSponsorOrgList:SponsorUpdate"
   Expected Payload: Updated sponsor name in alphabetical position
   ```

3. **Test Sponsor Deactivate**
   ```
   Expected Result: SyncLog entry with EventType = "PublishSponsorOrgList:SponsorDeactivate"
   Expected Payload: Deactivated sponsor removed from list
   ```

4. **Test Sponsor Approve**
   ```
   Expected Result: SyncLog entry with EventType = "PublishSponsorOrgList:SponsorApprove"
   Expected Payload: Newly approved sponsor added to list
   ```

5. **Test Sponsor Merge**
   ```
   Expected Result: SyncLog entry with EventType = "PublishSponsorOrgList:SponsorMerge"
   Expected Payload: Merged sponsor removed, survivor remains
   ```

6. **Verify Integration Retry Dashboard**
   ```
   Navigate to: /Operations/SyncRetry
   Filter: TargetSystem = PowerSchool, EntityType = SponsorMaster
   Expected: All publish attempts visible with details
   ```

---

**Document Updated**: March 30, 2026  
**Version**: 1.0  
**Status**: Production Ready
