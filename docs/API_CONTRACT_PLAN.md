# ISM Sponsor Management System - API Contract Plan

**Date:** April 8, 2026  
**Purpose:** Define and validate REST API surface for external system integration  
**Status:** Review - No coding until approved

---

## API Contract Overview

This document defines the complete REST API surface for the ISM Sponsor Management System, serving as the authoritative source for:
- Student Charging Portal (SCP) - Real-time coverage decisions
- PowerSchool - Student-sponsor link synchronization
- NetSuite - Billing allocation posting
- Online Billing System (OBS) - Statement data
- External monitoring and audit systems

---

## Design Principles

1. **Versioned Routes:** All endpoints support `/api/v1/*` for versioning
2. **Backward Compatibility:** Legacy routes (`/api/coverage/*`) maintained
3. **RESTful:** Standard HTTP verbs (GET, POST) with appropriate status codes
4. **Authorized:** All endpoints require authentication + role-based authorization
5. **Idempotent Operations:** Coverage preview is read-only; commit is idempotent by correlationId
6. **Structured Errors:** Consistent error responses with ProblemDetails format
7. **Telemetry:** All endpoints emit Application Insights telemetry

---

## API Groups

### A. Coverage API (Real-Time Evaluation)

**Purpose:** Student Charging Portal calls these endpoints when entering charges to determine sponsor vs parent responsibility.

| Endpoint | Method | Purpose | Request | Response | Auth Roles |
|----------|--------|---------|---------|----------|------------|
| `/api/v1/coverage/preview` | POST | Preview coverage decision (no audit) | CoverageEvaluationRequest | CoverageEvaluationResponse | admin, admissions, cashier |
| `/api/v1/coverage/commit` | POST | Commit coverage decision (with audit) | CoverageEvaluationRequest | CoverageEvaluationResponse | admin, admissions, cashier |
| `/api/v1/coverage/evaluate` | POST | Legacy alias for commit | CoverageEvaluationRequest | CoverageEvaluationResponse | admin, admissions, cashier |
| `/api/v1/coverage/decisions/{auditId}` | GET | Get decision by ID | - | CoverageDecisionDetailDto | admin, admissions, cashier |
| `/api/v1/coverage/decisions?correlationId=...` | GET | Query decisions by correlation ID | correlationId, studentId, from, to | List\<CoverageDecisionDetailDto\> | admin, admissions, cashier |
| `/api/v1/coverage/reasons` | GET | Get all reason codes | - | List\<ReasonCodeInfo\> | admin, admissions, cashier |

**Status:** ✅ **COMPLETE** - All endpoints implemented in `CoverageController.cs`

---

### B. PowerSchool Integration API

**Purpose:** PowerSchool calls these endpoints to sync student-sponsor links and publish sponsor master list.

| Endpoint | Method | Purpose | Request | Response | Auth Roles |
|----------|--------|---------|---------|----------|------------|
| `/api/v1/integrations/powerschool/student-sponsor-sync` | POST | Sync student-sponsor links from PowerSchool | StudentSponsorSyncRequest | StudentSponsorSyncResponse | admin, cashier |

**Status:** ✅ **COMPLETE** - Implemented in `IntegrationController.cs`

**Additional PowerSchool Endpoints (Future):**
- [ ] `POST /api/v1/integrations/powerschool/sponsor-master-publish` - Publish sponsor list to Sponsor_OrgName field (not yet exposed as API, handled internally)

---

### C. NetSuite Integration API

**Purpose:** ISM Sponsor system posts billing allocation decisions to NetSuite for receivables.

| Endpoint | Method | Purpose | Request | Response | Auth Roles |
|----------|--------|---------|---------|----------|------------|
| `/api/v1/integrations/netsuite/allocation-post` | POST | Post coverage allocation to NetSuite | CoverageDecisionSyncRequestDto | SyncResult | admin, cashier |
| `/api/v1/integrations/netsuite/posting-status/{correlationId}` | GET | Get posting status by correlation ID | - | SyncStatusDto | admin, cashier |

**Legacy Routes (Maintained):**
- `POST /api/netsuite/post-decision` (alias for allocation-post)
- `GET /api/sync-status/{correlationId}` (alias for posting-status)

**Status:** ✅ **COMPLETE** - Implemented in `IntegrationController.cs`

---

### D. Online Billing System (OBS) Integration API

**Purpose:** ISM Sponsor system posts statement-ready data to OBS for parent/sponsor statement generation.

| Endpoint | Method | Purpose | Request | Response | Auth Roles |
|----------|--------|---------|---------|----------|------------|
| `/api/v1/integrations/obs/statement-update` | POST | Post coverage decision to OBS | CoverageDecisionSyncRequestDto | SyncResult | admin, cashier |
| `/api/v1/statements/students/{studentId}` | GET | Get statement data for student | schoolYearId, periodStart, periodEnd | StatementQueryDto | admin, cashier, sponsor |
| `/api/v1/statements/sponsors/{sponsorId}` | GET | Get statement data for sponsor | schoolYearId, periodStart, periodEnd | StatementQueryDto | admin, cashier, sponsor |

**Legacy Routes:**
- `POST /api/obs/post-decision` (alias for statement-update)

**Status:** ✅ **COMPLETE** - Implemented in `IntegrationController.cs` and `StatementsController.cs`

---

### E. Audit & Reference API

**Purpose:** External systems query audit trail and reference data.

| Endpoint | Method | Purpose | Request | Response | Auth Roles |
|----------|--------|---------|---------|----------|------------|
| `/api/v1/audit/decisions/{decisionId}` | GET | Get decision audit by ID | - | CoverageDecisionDetailDto | admin, cashier |
| `/api/v1/audit/integrations/{correlationId}` | GET | Get integration sync history | - | SyncStatusDto | admin, cashier |
| `/api/v1/reference/sponsors` | GET | Get active sponsors | schoolYearId, search, limit | List\<SponsorDto\> | admin, admissions, cashier, sponsor |
| `/api/v1/reference/items` | GET | Get charge items | categoryId, gradeLevel, search, limit | List\<ItemDto\> | admin, admissions, cashier, sponsor |
| `/api/v1/reference/categories` | GET | Get item categories | search, limit | List\<ItemCategoryDto\> | admin, admissions, cashier, sponsor |
| `/api/v1/reference/active-sponsor-links` | GET | Get student-sponsor links | sponsorId, studentId, schoolYearId, limit | List\<ActiveSponsorLinkDto\> | admin, admissions, cashier, sponsor |
| `/api/v1/sponsors/{sponsorId}` | GET | Get sponsor detail by ID | - | SponsorDto | admin, admissions, cashier, sponsor |

**Status:** ✅ **COMPLETE** - Implemented in `AuditApiController.cs`, `ReferenceController.cs`, and `SponsorsApiController.cs`

---

## Request/Response Contract Details

### Coverage Evaluation Request

```json
{
  "schoolYearId": "2025-2026",
  "studentId": "ST12345",
  "sponsorId": "ACME-CORP",
  "logId": 123,
  "itemId": "TUITION-G10",
  "categoryId": "TUITION",
  "amount": 25000.00,
  "chargeDate": "2026-04-08T00:00:00Z",
  "correlationId": "SCP-TX-20260408-001",
  "chargeLineId": "CL-789",
  "isPreview": false,
  "studentOverrideId": null,
  "requiresCoverageDocumentation": false
}
```

### Coverage Evaluation Response

```json
{
  "decision": "Split",
  "billTo": "Split",
  "sponsorAmount": 15000.00,
  "parentAmount": 10000.00,
  "sponsorPercent": 0.60,
  "parentPercent": 0.40,
  "reasonCode": "PERCENTAGE_SPLIT_60",
  "explanation": "LoG covers 60% of tuition charges",
  "matchedRuleId": 45,
  "ruleVersion": "2025-2026-v3",
  "auditRecordId": 7890,
  "decisionId": "7890",
  "success": true,
  "errorMessage": null,
  "correlationId": "SCP-TX-20260408-001",
  "evaluatedAt": "2026-04-08T14:23:45Z",
  "ruleSnapshot": "{\"ruleId\":45,\"itemId\":\"TUITION-G10\",\"coveragePercent\":0.60,...}"
}
```

### Student-Sponsor Sync Request

```json
{
  "mappings": [
    {
      "studentId": "ST12345",
      "sponsorId": "ACME-CORP",
      "effectiveFrom": "2025-08-01T00:00:00Z",
      "effectiveTo": null,
      "studentName": "John Doe"
    }
  ],
  "schoolYearId": "2025-2026",
  "correlationId": "PS-SYNC-20260408-001",
  "sourceSystem": "PowerSchool",
  "syncTimestamp": "2026-04-08T14:00:00Z"
}
```

### Sync Status Response

```json
{
  "correlationId": "SCP-TX-20260408-001",
  "targetSystems": [
    {
      "system": "PowerSchool",
      "status": "Succeeded",
      "lastAttemptedAt": "2026-04-08T14:23:50Z",
      "lastSucceededAt": "2026-04-08T14:23:50Z",
      "retryCount": 0,
      "errorMessage": null,
      "externalReferenceId": "PS-REF-12345"
    },
    {
      "system": "NetSuite",
      "status": "Succeeded",
      "lastAttemptedAt": "2026-04-08T14:23:52Z",
      "lastSucceededAt": "2026-04-08T14:23:52Z",
      "retryCount": 0,
      "errorMessage": null,
      "externalReferenceId": "NS-TRX-67890"
    }
  ],
  "overallStatus": "Succeeded",
  "lastUpdatedAt": "2026-04-08T14:23:52Z"
}
```

---

## Error Response Contract

All API endpoints return consistent error responses using ProblemDetails format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Amount must be greater than zero",
  "traceId": "00-abc123...-def456...-00",
  "errors": {
    "Amount": ["The field Amount must be between 0.01 and 1.79769313486232E+308."]
  }
}
```

**HTTP Status Codes:**
- `200 OK` - Success
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Authenticated but insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Unexpected server error

---

## Authentication & Authorization

### Authentication Methods

1. **Cookie-based (for MVC UI):** ASP.NET Core Identity with session cookies
2. **Bearer Token (for API):** JWT tokens issued by Azure AD or API Management
3. **API Key (optional future):** Via Azure API Management subscription keys

### Authorization Roles

| Role | Permissions |
|------|-------------|
| `admin` | Full access to all endpoints |
| `admissions` | Coverage preview/commit, reference data |
| `cashier` | Coverage, integrations, audit queries |
| `sponsor` | Read-only access to own data (statements, reference) |

**Role Matrix:**

| Endpoint Group | admin | admissions | cashier | sponsor |
|----------------|-------|------------|---------|---------|
| Coverage | ✅ | ✅ | ✅ | ❌ |
| Integrations | ✅ | ❌ | ✅ | ❌ |
| Audit | ✅ | ❌ | ✅ | ❌ |
| Reference | ✅ | ✅ | ✅ | ✅ (read-only) |
| Statements | ✅ | ❌ | ✅ | ✅ (own data only) |
| Sponsors | ✅ | ✅ | ✅ | ✅ (read-only) |

---

## Rate Limiting & Quotas (Azure API Management)

**Recommended Policies:**

| Consumer | Rate Limit | Burst | Quota (per day) |
|----------|------------|-------|-----------------|
| Student Charging Portal | 100 req/min | 150 | 50,000 |
| PowerSchool | 20 req/min | 30 | 10,000 |
| NetSuite | 50 req/min | 75 | 25,000 |
| OBS | 50 req/min | 75 | 25,000 |
| Internal Apps | 200 req/min | 300 | Unlimited |

---

## API Versioning Strategy

**Current Version:** v1

**Deprecation Policy:**
- New versions announced 90 days in advance
- Old versions supported for 180 days after new version release
- Breaking changes require new version (v2, v3, etc.)

**Non-Breaking Changes (no version bump):**
- Adding optional request fields
- Adding response fields
- Adding new endpoints
- Relaxing validation rules

**Breaking Changes (require version bump):**
- Removing request/response fields
- Renaming fields
- Changing field types
- Tightening validation rules
- Changing HTTP status codes

---

## API Documentation (Swagger/OpenAPI)

**Status:** ⚠️ **MISSING** - Needs implementation

**Recommended:**
- Add Swashbuckle.AspNetCore package
- Generate OpenAPI 3.0 spec
- Expose Swagger UI at `/swagger`
- Publish OpenAPI spec to API Management developer portal

**Implementation Required:**
1. Add XML documentation comments to all API controllers
2. Configure Swashbuckle in Program.cs
3. Add examples to request/response models
4. Configure security definitions (Bearer, Cookie)

---

## Missing or Incomplete Items

### 1. API Documentation (Swagger)
**Status:** ❌ **Not Implemented**  
**Priority:** High  
**Effort:** 2-3 hours  
**Files to Create/Edit:**
- Program.cs - Add Swashbuckle services
- ISMSponsor.csproj - Add Swashbuckle package
- Controllers/Api/*.cs - Add XML doc comments

### 2. Structured Error Responses (ProblemDetails)
**Status:** ⚠️ **Partial** - Some endpoints return anonymous objects  
**Priority:** Medium  
**Effort:** 1-2 hours  
**Files to Edit:**
- Controllers/Api/IntegrationController.cs
- Controllers/Api/CoverageController.cs
- Middleware/GlobalExceptionHandlerMiddleware.cs

### 3. PowerSchool Sponsor Master Publish API
**Status:** ⚠️ **Service exists but not exposed as API**  
**Priority:** Low (can be called internally)  
**Effort:** 1 hour  
**Files to Edit:**
- Controllers/Api/IntegrationController.cs (add endpoint)

### 4. API Throttling
**Status:** ❌ **Not Implemented**  
**Priority:** Low (Azure API Management should handle)  
**Effort:** N/A (defer to APIM)

### 5. API Key Authentication
**Status:** ❌ **Not Implemented**  
**Priority:** Low (Azure API Management should handle)  
**Effort:** N/A (defer to APIM)

---

## Recommendation: Add Swagger Documentation

**Justification:**
- External consumers (SCP, PowerSchool, NetSuite, OBS) need interactive API documentation
- Reduces integration support burden
- Auto-generates client SDKs
- Self-documenting contracts

**Implementation Plan:**

1. Add Swashbuckle package to ISMSponsor.csproj
2. Configure Swagger in Program.cs
3. Add XML documentation comments to API controllers
4. Set security schemes (Bearer token, Cookie auth)
5. Add example request/response payloads
6. Deploy Swagger UI at `/swagger` (dev/staging only, not production)
7. Export OpenAPI spec for Azure API Management import

**Sample Swagger Configuration:**

```csharp
// Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ISM Sponsor Management API",
        Version = "v1",
        Description = "REST API for sponsor management and LoG coverage evaluation",
        Contact = new OpenApiContact
        {
            Name = "ISM DevOps",
            Email = "devops@ismmanila.org"
        }
    });
    
    // Add XML documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    // Add security definitions
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using Bearer scheme"
    });
});

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Staging")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

## API Testing Strategy

**Unit Tests:**
- Request validation
- Authorization checks
- Response serialization

**Integration Tests:**
- End-to-end API flows
- Database interactions
- External service mocks

**Load Tests:**
- Coverage API: 100 req/sec sustained
- Integration APIs: 20 req/sec sustained

**Test Tools:**
- xUnit for unit tests
- Microsoft.AspNetCore.Mvc.Testing for integration tests
- Azure Load Testing for performance tests

---

## Next Steps

### Phase 1: API Documentation (Recommended)
1. Add Swashbuckle.AspNetCore package
2. Configure Swagger in Program.cs
3. Add XML doc comments to all API controllers
4. Test Swagger UI in dev environment

### Phase 2: Error Response Standardization (Optional)
1. Review all API endpoints for error response consistency
2. Standardize on ProblemDetails format
3. Add examples to documentation

### Phase 3: Additional Endpoints (As Needed)
1. Add PowerSchool sponsor master publish API if needed
2. Add any missing query endpoints discovered during integration

---

## Approval Required

**Review Checklist:**
- [ ] Coverage API contracts approved by Student Charging Portal team
- [ ] PowerSchool API contracts approved by PowerSchool integration team
- [ ] NetSuite API contracts approved by Finance/ERP team
- [ ] OBS API contracts approved by Billing team
- [ ] Rate limits and quotas approved by DevOps
- [ ] Authentication/authorization strategy approved by Security
- [ ] API versioning strategy approved by Architecture

**Once approved, implementation will proceed with:**
1. Swagger documentation (if approved)
2. Any contract adjustments based on feedback
3. Integration testing with external consumers

---

**Contract Status:** ✅ **READY FOR REVIEW**  
**API Implementation Status:** ✅ **COMPLETE** (except Swagger docs)  
**Estimated Effort for Swagger:** 2-3 hours
