# ISM Sponsor - Diagnostic Tools Guide

## Purpose
This guide explains how to use the diagnostic tools to troubleshoot ItemId format mismatches in Letter of Guarantee coverage rules.

## Problem Background
Coverage rules are not being saved despite success messages. Root cause: The `BuildCoverageRulesForSaveAsync` method returns an empty list when ItemIds from the client don't match ItemIds in the database (after case-insensitive, whitespace-trimmed comparison).

---

## Tool 1: SQL Diagnostic Script

**Location:** `scripts/diagnose_items_format.sql`

### How to Use:
1. Open your database management tool (Azure Data Studio, SSMS, etc.)
2. Connect to your production database
3. Open the `diagnose_items_format.sql` file
4. Run each query separately (9 queries total)
5. Copy the results from each query

### Key Queries:
- **Query 1:** All Items for STUD-10 grade level - Shows exact format of Grade 10 items
- **Query 2:** All TUITION-related items - Finds items containing "TUITION"
- **Query 3:** Specific ItemId check - Tests for exact match: `MAJOR-TUITION-PHP-G10 TUITION FULL`
- **Query 4:** Case-insensitive check - Tests with case-insensitive comparison
- **Query 7:** Space detection - Identifies items with leading/trailing spaces
- **Query 9:** Character breakdown - Shows exact byte representation

### What to Look For:
- **ItemId format:** Hyphens, spaces, uppercase/lowercase patterns
- **ItemIdLength:** Compare with what client is sending
- **Leading/trailing spaces:** Query 7 will flag these
- **First/Last characters:** Hidden characters or encoding issues

### Expected Results:
You should see the ItemId "MAJOR-TUITION-PHP-G10 TUITION FULL" in the results. If not, check for:
- Different spacing (e.g., single vs double spaces)
- Different casing (e.g., "Major" vs "MAJOR")
- Missing or extra hyphens
- Trailing spaces

---

## Tool 2: Enhanced Server-Side Logging

**Location:** `Controllers/LetterOfGuaranteeController.cs` - `BuildCoverageRulesForSaveAsync` method

### How to Use:
1. Deploy the updated code to your environment
2. Open Browser Developer Tools (F12)
3. Go to Letter of Guarantee edit page
4. Add a coverage rule with ItemId: "MAJOR-TUITION-PHP-G10 TUITION FULL"
5. Click Save
6. Check **Debug Output** in Visual Studio OR **Application logs** in Azure

### What Gets Logged:
```
=== BuildCoverageRulesForSaveAsync DIAGNOSTIC START ===
Requested 1 ItemIds:
  REQ: [MAJOR-TUITION-PHP-G10 TUITION FULL] Length=36, First='M', Last='L'
Database has 450 active Items
  NO MATCH for: [MAJOR-TUITION-PHP-G10 TUITION FULL]
    Similar items in DB:
      DB: [MAJOR-TUITION-PHP-G10-TUITION-FULL] Length=35
      DB: [major-tuition-php-g10 tuition full] Length=36
BuildCoverageRulesForSaveAsync - Found 0 matching items
=== BuildCoverageRulesForSaveAsync DIAGNOSTIC END ===
```

### What to Look For:
- **Length mismatch:** Client sends 36 chars, DB has 35 chars (missing space?)
- **Character comparison:** First/Last characters should match
- **Similar items:** Shows DB items that contain "TUITION" or "MAJOR" for comparison
- **Match count:** Should be > 0 if ItemId exists

### How to Access Logs:

#### Local Development:
- Visual Studio: `View > Output > Show output from: Debug`
- Rider: `Run > View > Output`

#### Azure App Service:
1. Go to Azure Portal
2. Navigate to your App Service
3. Select `Monitoring > Log stream` OR
4. Select `Monitoring > Diagnose and solve problems > Application Logs`

---

## Tool 3: Diagnostic Test Endpoint

**Endpoint:** `POST /Settings/LetterOfGuarantee/TestItemIdMatch`

**Access:** Admin role only

### How to Use:

#### Option A: Browser Console
```javascript
// Open Letter of Guarantee page, then open Developer Tools Console (F12)
fetch('/Settings/LetterOfGuarantee/TestItemIdMatch', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
    },
    body: JSON.stringify({
        itemId: 'MAJOR-TUITION-PHP-G10 TUITION FULL',
        gradeLevel: 'STUD-10'
    })
})
.then(r => r.json())
.then(data => console.log(data));
```

#### Option B: Postman/Insomnia
1. Create new POST request
2. URL: `https://your-site.azurewebsites.net/Settings/LetterOfGuarantee/TestItemIdMatch`
3. Headers: `Content-Type: application/json`
4. Body (JSON):
```json
{
    "itemId": "MAJOR-TUITION-PHP-G10 TUITION FULL",
    "gradeLevel": "STUD-10"
}
```
5. Send request (must be authenticated as admin)

### Response Format:
```json
{
    "success": true,
    "testItemId": "MAJOR-TUITION-PHP-G10 TUITION FULL",
    "testItemIdLength": 36,
    "testItemIdFirstChar": "M",
    "testItemIdLastChar": "L",
    "exactMatchFound": false,
    "exactMatch": null,
    "similarItemsCount": 3,
    "similarItems": [
        {
            "itemId": "MAJOR-TUITION-PHP-G10-TUITION-FULL",
            "itemName": "Grade 10 Tuition Full",
            "categoryId": "TUITION PHP FULL",
            "gradeLevel": "STUD-10",
            "length": 35,
            "firstChar": "M",
            "lastChar": "L"
        }
    ],
    "gradeItemsCount": 15,
    "gradeItems": [...],
    "totalActiveItemsInDb": 450
}
```

### What to Look For:
- **exactMatchFound:** Should be `true` if ItemId exists
- **similarItems:** Shows closest matches if exact match fails
- **Length comparison:** Compare `testItemIdLength` with `length` in similarItems
- **Character comparison:** Compare first/last chars to identify hidden characters

---

## Diagnostic Workflow

### Step 1: Run SQL Queries
1. Run `scripts/diagnose_items_format.sql` queries 1, 2, 3
2. Verify "MAJOR-TUITION-PHP-G10 TUITION FULL" exists in results
3. Note the exact format, length, and any spaces

### Step 2: Test with Diagnostic Endpoint
1. Use browser console or Postman to call `/TestItemIdMatch`
2. Pass the ItemId from SQL results
3. Check if `exactMatchFound = true`
4. If false, compare `similarItems` to identify the mismatch

### Step 3: Attempt Save with Enhanced Logging
1. Edit a Letter of Guarantee
2. Add coverage rule with the ItemId
3. Save and check Debug Output logs
4. Review the diagnostic output showing match attempts

### Step 4: Analyze Results
Compare the three sources:
- **SQL Query:** Actual database ItemId format
- **Diagnostic Endpoint:** What the matching logic sees
- **Debug Logs:** What the save operation receives from client

Identify the difference:
- Extra/missing spaces?
- Different casing (shouldn't matter with OrdinalIgnoreCase)?
- Hidden characters (tabs, newlines)?
- Different hyphen characters (en-dash vs hyphen)?

---

## Common Issues and Solutions

### Issue: Leading/Trailing Spaces
**Symptom:** SQL Query 7 shows "HAS LEADING/TRAILING SPACES"  
**Solution:** Database ItemIds need cleaning:
```sql
UPDATE Items SET ItemId = LTRIM(RTRIM(ItemId));
```

### Issue: Case Mismatch
**Symptom:** ItemId exists but different case (e.g., "Major-Tuition" vs "MAJOR-TUITION")  
**Solution:** Should work with OrdinalIgnoreCase, but verify in diagnostic endpoint

### Issue: Hyphen Character Mismatch
**Symptom:** ItemId looks identical but length differs by character encoding  
**Solution:** Check for en-dash (U+2013) vs hyphen-minus (U+002D):
```sql
SELECT ItemId, UNICODE(SUBSTRING(ItemId, 6, 1)) as HyphenUnicode
FROM Items WHERE ItemId LIKE 'MAJOR%TUITION%';
-- Should return 45 (hyphen-minus), not 8211 (en-dash)
```

### Issue: Double Spaces
**Symptom:** ItemId has "TUITION  FULL" (two spaces) vs "TUITION FULL" (one space)  
**Solution:** Normalize spaces in database:
```sql
UPDATE Items SET ItemId = REPLACE(ItemId, '  ', ' ') WHERE ItemId LIKE '%  %';
```

---

## Next Steps After Diagnosis

Once you identify the exact mismatch:

1. **Update Database:** If production ItemIds have formatting issues, clean them
2. **Update Code:** If code needs to handle multiple formats, add normalization
3. **Update Seed Data:** Ensure demo/test data matches production format
4. **Document Format:** Add ItemId format specification to documentation

---

## Support
If diagnostic tools don't reveal the issue, provide:
- Results from SQL Query 3 (specific ItemId check)
- Output from Diagnostic Endpoint (JSON response)
- Debug logs from save attempt (console output)
