# Azure Configuration Fixes

## Problems Summary

1. **Swagger/API Documentation (404):** `/api/docs` returns 404 - Swagger UI not accessible
2. **Evaluate Endpoint (404):** `/api/v1/coverage/evaluate` returns 404, while `/api/v1/coverage/preview` works

### Root Cause

- **`/evaluate` endpoint:** Attempts to save audit records to the database (`IsPreview = false`)
- **`/preview` endpoint:** Skips all database writes (`IsPreview = true`)
- **Azure configuration:** Connection string is still the untokenized placeholder `#{DatabaseConnectionString}#`
- **Result:** Database operations fail silently, resulting in 404 responses

**Code Location:** [Services/CoverageEvaluationService.cs](../Services/CoverageEvaluationService.cs#L49)
```csharp
if (!request.IsPreview)
{
    _context.CoverageEvaluationAudits.Add(audit);
    await _context.SaveChangesAsync();
    response.AuditRecordId = audit.AuditId;
}
```

## Solutions

### Fix 1: Enable Swagger by Setting Environment Variable

Swagger is disabled because `ASPNETCORE_ENVIRONMENT` is not set in Azure.

**AIn **Configuration**, scroll to **Connection strings** section
2. Click **+ New connection string**
3. Configure:
   - **Name:** `DefaultConnection`
   - **Value:** `Server=ism-sandbox.database.windows.net,1433;Database=ISMSponsor;User Id=ISMSponsorUser;Password=7$N7UE+y0Zl;TrustServerCertificate=true;MultipleActiveResultSets=true`
   - **Type:** `SQLAzure`
4. Click **OK**, then **Save**

**Result:** `/evaluate` endpoint will work and save audit records to database

---

### Final Step: Restart the App Service

After making both configuration changes, click **Restart** at the top of the page.
**Result:** Swagger will be accessible at https://ismsponsor.azurewebsites.net/api/docs

---

### Fix 2: Configure Database Connection String

The `/evaluate` endpoint needs database access to save audit records.

**Azure Portal Steps:**

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to: **App Services** → **ismsponsor** → **Configuration**
3. Under **Connection strings** section, click **+ New connection string**
4. Configure:
   -Alternative: Azure CLI

If you have Azure CLI installed, you can apply both fixes with these commands:

```bash
# Set environment variable
az webapp config appsettings set \
  --resource-group ISM-Sponsor-RG \
  --name ismsponsor \
  --settings ASPNETCORE_ENVIRONMENT="Production"

# Set connection string  e**
6. **Restart** the app service

### Option 2: Azure CLI

```bash
# Set the connection string
### 1. Verify Swagger UI is Accessible

Open in browser: https://ismsponsor.azurewebsites.net/api/docs

**Expected:** Swagger UI page loads showing all API endpoints

### 2. Test the `/evaluate` Endpoint
  --resource-group ISM-Sponsor-RG \
  --name ismsponsor \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=ism-sandbox.database.windows.net,1433;Database=ISMSponsor;User Id=ISMSponsorUser;Password=7\$N7UE+y0Zl;TrustServerCertificate=true;MultipleActiveResultSets=true"

# Restart the app
az webapp restart --resource-group ISM-Sponsor-RG --name ismsponsor
```

**Note:** The `$` in the password must be escaped as `\$` in bash.

## Verification

After applying the fix, test the `/evaluate` endpoint:

```bash
curl -X POST "https://ismsponsor.azurewebsites.net/api/v1/coverage/evaluate" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "STUD006",
    "schoolYearId": "25-26",
    "itemId": "MAJOR-SLSP-G06 SLSP FULL",
    "chargeDescription": "Test Charge",
    "amount": 800.00,
    "currency": "US Dollar",
    "chargeDate": "2026-05-06"
  }'
```

**Expected Response:** HTTP 200 with valid JSON
```json
{
  "success": true,
  "decision": "Covered",
  "billTo": "Sponsor",
  "sponsorAmount": 800.00,
  "parentAmount": 0,
  "allocations": [
    {
      "partyType": "Sponsor",
      "partyId": "SP002",
      "partyName": "Ayala Holdings",
      "amount": 800.00,
      "currency": "US Dollar",
      "billTo": "Sponsor",
      "chargeCode": "MAJOR-SLSP-G06 SLSP FULL",
      "chargeDescription": "Test Charge"
    }
  ],
  "auditRecordId": 123
}
```

## Run Smoke Tests

Once configured, verify all endpoints:

```bash
export DEPLOYED_API_BASE_URL="https://ismsponsor.azurewebsites.net"
cd "/Users/cruzr/Documents/ISM Sponsor"
dotnet test --filter "DeployedApiSmokeTests"
```

**Expected:** All 6 tests passing ✅

## Technical Details

### Why `/preview` Works

The Preview endpoint explicitly sets `request.IsPreview = true` before calling the evaluation service:

**[Controllers/Api/CoverageController.cs](../Controllers/Api/CoverageController.cs#L226)**
```csharp
public async Task<ActionResult<CoverageEvaluationResponse>> Preview([FromBody] CoverageEvaluationRequest request)
{
    // ...
    request.IsPreview = true;  // Skips database writes
    
    var response = await _evaluationService.EvaluateAsync(request, userId, userDisplay, userRole);
    return Ok(response);
}
```

### Why `/evaluate` Fails Without Database

The Evaluate endpoint leaves `IsPreview` as `false` (default), which triggers database operations:

**[Services/CoverageEvaluationService.cs](../Services/CoverageEvaluationService.cs#L47-L56)**
```csharp
if (!request.IsPreview)
{
    _context.CoverageEvaluationAudits.Add(audit);
    await _context.SaveChangesAsync();
    response.AuditRecordId = audit.AuditId;
}
else
{
    response.AuditRecordId = 0;
}
```

When the database connection is invalid, `SaveChangesAsync()` throws an exception, which gets caught and converted to a 404 response by the middleware.

## Related Files

- [appsettings.Production.json](../appsettings.Production.json#L10) - Contains tokenized placeholder
- [appsettings.json](../appsettings.json#L2-L4) - Contains actual connection string (used locally)
- [Controllers/Api/CoverageController.cs](../Controllers/Api/CoverageController.cs#L102) - Evaluate endpoint
- [Controllers/Api/CoverageController.cs](../Controllers/Api/CoverageController.cs#L213) - Preview endpoint
- [Services/CoverageEvaluationService.cs](../Services/CoverageEvaluationService.cs) - Evaluation logic with IsPreview checks
- [Tests/Api/DeployedApiSmokeTests.cs](../Tests/Api/DeployedApiSmokeTests.cs) - Smoke tests for deployed API
