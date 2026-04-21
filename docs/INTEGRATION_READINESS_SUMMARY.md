# Integration Readiness Summary - Quick Reference

## Current State: All Mocked ⚠️

| Integration Target | Status | File | Line | Priority |
|-------------------|--------|------|------|----------|
| PowerSchool | 🟡 **MOCKED** | `Integration/Adapters/MockIntegrationAdapters.cs` | 9-90 | **HIGH** |
| Student Charging Portal | 🟡 **MOCKED** | `Integration/Adapters/MockIntegrationAdapters.cs` | 94-147 | **HIGH** |
| NetSuite | 🟡 **MOCKED** | `Integration/Adapters/MockIntegrationAdapters.cs` | 150-203 | MEDIUM |
| Online Billing System | 🟡 **MOCKED** | `Integration/Adapters/MockIntegrationAdapters.cs` | 206-285 | MEDIUM |

**DI Registration:** `Program.cs` Lines 175-178

---

## Implementation Roadmap (3 Weeks)

### Week 1: PowerSchool + SCP
- **Days 1-3:** PowerSchool adapter development & testing
- **Days 4-5:** PowerSchool Pilot deployment

### Week 2: Full Integration Layer
- **Days 1-2:** PowerSchool Production + SCP development
- **Days 3-4:** SCP Pilot deployment
- **Days 5-6:** NetSuite development (OAuth 1.0a complexity)

### Week 3: NetSuite + OBS
- **Days 1-2:** NetSuite Pilot deployment (sandbox)
- **Days 3-4:** OBS development (database + credentials)
- **Day 5:** OBS Pilot deployment (security review)

---

## Files to Create (4 New Adapters)

```
Integration/Adapters/
├── PowerSchoolAdapter.cs              (NEW - 350 lines)
├── StudentChargingPortalAdapter.cs    (NEW - 280 lines)
├── NetSuiteAdapter.cs                 (NEW - 420 lines)
└── OnlineBillingSystemAdapter.cs      (NEW - 380 lines)

Data/
└── ObsDbContext.cs                     (NEW - 80 lines)
```

---

## Files to Edit

### 1. Program.cs (Line 175-178)
**Current:**
```csharp
builder.Services.AddScoped<IPowerSchoolAdapter, MockPowerSchoolAdapter>();
builder.Services.AddScoped<IStudentChargingPortalAdapter, MockStudentChargingPortalAdapter>();
builder.Services.AddScoped<INetSuiteAdapter, MockNetSuiteAdapter>();
builder.Services.AddScoped<IOnlineBillingSystemAdapter, MockOnlineBillingSystemAdapter>();
```

**New:**
```csharp
var useMockIntegrations = builder.Configuration.GetValue<bool>("IntegrationEndpoints:UseMocks", true);

if (useMockIntegrations)
{
    // Development: Use mocks
    builder.Services.AddScoped<IPowerSchoolAdapter, MockPowerSchoolAdapter>();
    builder.Services.AddScoped<IStudentChargingPortalAdapter, MockStudentChargingPortalAdapter>();
    builder.Services.AddScoped<INetSuiteAdapter, MockNetSuiteAdapter>();
    builder.Services.AddScoped<IOnlineBillingSystemAdapter, MockOnlineBillingSystemAdapter>();
}
else
{
    // Pilot/Production: Use real adapters with HttpClient + Polly
    builder.Services.AddHttpClient("PowerSchool", client => { /* config */ });
    builder.Services.AddHttpClient("StudentChargingPortal", client => { /* config */ });
    builder.Services.AddHttpClient("NetSuite", client => { /* config */ });
    builder.Services.AddHttpClient("OnlineBillingSystem", client => { /* config */ });
    
    builder.Services.AddScoped<IPowerSchoolAdapter, PowerSchoolAdapter>();
    builder.Services.AddScoped<IStudentChargingPortalAdapter, StudentChargingPortalAdapter>();
    builder.Services.AddScoped<INetSuiteAdapter, NetSuiteAdapter>();
    builder.Services.AddScoped<IOnlineBillingSystemAdapter, OnlineBillingSystemAdapter>();
}
```

### 2. appsettings.Development.json
**Add:**
```json
"IntegrationEndpoints": {
  "UseMocks": true,
  "SyncEnabled": false
}
```

### 3. appsettings.Pilot.json
**Add:**
```json
"IntegrationEndpoints": {
  "UseMocks": false,
  "SyncEnabled": true,
  "PowerSchoolApiUrl": "#{Integration:PowerSchoolApiUrl}#",
  "StudentChargingPortalApiUrl": "#{Integration:StudentChargingPortalApiUrl}#",
  "NetSuiteApiUrl": "#{Integration:NetSuiteApiUrl}#",
  "OnlineBillingSystemApiUrl": "#{Integration:OnlineBillingSystemApiUrl}#"
},
"Integration": {
  "PowerSchool": {
    "ClientId": "#{Integration:PowerSchool:ClientId}#",
    "ClientSecret": "#{Integration:PowerSchool:ClientSecret}#",
    "TokenUrl": "#{Integration:PowerSchool:TokenUrl}#"
  },
  "StudentChargingPortal": {
    "ApiKey": "#{Integration:StudentChargingPortal:ApiKey}#"
  },
  "NetSuite": {
    "AccountId": "#{Integration:NetSuite:AccountId}#",
    "ConsumerKey": "#{Integration:NetSuite:ConsumerKey}#",
    "ConsumerSecret": "#{Integration:NetSuite:ConsumerSecret}#",
    "TokenId": "#{Integration:NetSuite:TokenId}#",
    "TokenSecret": "#{Integration:NetSuite:TokenSecret}#"
  },
  "OnlineBillingSystem": {
    "ConnectionString": "#{Integration:OnlineBillingSystem:ConnectionString}#",
    "UseDatabaseDirect": true
  }
}
```

---

## Authentication Methods

| Target | Method | Complexity | Caching |
|--------|--------|------------|---------|
| **PowerSchool** | OAuth 2.0 Client Credentials | Medium | ✅ Token cached (1 hour) |
| **Student Charging Portal** | API Key | Low | N/A (static key) |
| **NetSuite** | OAuth 1.0a (TBA) | **HIGH** | ✅ Signature per request |
| **Online Billing System** | Direct DB Connection | Medium | N/A (EF Core) |

---

## Retry & Error Handling (Polly)

### Retry Policy (All Adapters)
- **Attempts:** 3 retries
- **Backoff:** Exponential (2s → 4s → 8s)
- **Triggers:** 408 (Timeout), 429 (Rate Limit), 503 (Unavailable), 504 (Gateway Timeout)

### Circuit Breaker (All Adapters)
- **Threshold:** 5 consecutive failures
- **Break Duration:** 30 seconds (60s for NetSuite)
- **Reset:** Automatic on first success after break

### Error Code Taxonomy

| Error Code | Retry? | Severity | User Action |
|------------|--------|----------|-------------|
| `{TARGET}_ERR_001` | ❌ | Warning | Check API configuration |
| `{TARGET}_ERR_AUTH` | ❌ | Critical | Fix credentials in Key Vault |
| `{TARGET}_ERR_RATE_LIMIT` | ✅ | Info | Automatic backoff |
| `{TARGET}_ERR_TIMEOUT` | ✅ | Warning | Automatic retry |
| `{TARGET}_ERR_CIRCUIT_OPEN` | ⏸️ | Critical | Wait 30s or contact IT |

---

## Azure Key Vault Secrets (Required)

### PowerSchool (3 secrets)
```
Integration--PowerSchool--ClientId
Integration--PowerSchool--ClientSecret
Integration--PowerSchool--TokenUrl
```

### Student Charging Portal (1 secret)
```
Integration--StudentChargingPortal--ApiKey
```

### NetSuite (5 secrets)
```
Integration--NetSuite--AccountId
Integration--NetSuite--ConsumerKey
Integration--NetSuite--ConsumerSecret
Integration--NetSuite--TokenId
Integration--NetSuite--TokenSecret
```

### Online Billing System (2 secrets)
```
Integration--OnlineBillingSystem--ConnectionString
Integration--OnlineBillingSystem--ApiKey
```

**Total:** 11 secrets to configure per environment (Pilot, Production)

---

## Payloads & Contracts

### PowerSchool: Publish Sponsor Org List
**Endpoint:** `POST /api/custom_fields/sponsor_orgname/update_values`  
**Request:**
```json
{
  "fieldName": "Sponsor_OrgName",
  "values": ["ABC Company", "XYZ Bank", "..."],
  "correlationId": "uuid",
  "source": "ISMSponsor",
  "timestamp": "2026-04-08T14:23:45.123Z"
}
```

### Student Charging Portal: Sync Sponsor + Rules
**Endpoint:** `POST /api/sponsors/upsert`  
**Request:**
```json
{
  "sponsorId": "ABC001",
  "sponsorName": "ABC Company",
  "isActive": true,
  "coverageRules": [
    {"itemId": "TUITION", "category": "Tuition", "decision": "FullyCovered"}
  ]
}
```

### NetSuite: Upsert Customer
**Endpoint:** `POST /api/record/customer`  
**Auth:** OAuth 1.0a (complex signature generation)  
**Request:**
```json
{
  "externalId": "ABC001",
  "companyName": "ABC Company",
  "legalName": "ABC Company LLC",
  "taxIdNum": "12-3456789",
  "billAddr": {"addr1": "...", "city": "Manila", "country": "PH"},
  "terms": {"id": "2", "name": "Net 30"}
}
```

### Online Billing System: Direct Database
**Table:** `FINDB01.OnlineBillingPortal.CompanySponsors`  
**Method:** Entity Framework Core MERGE (upsert)

---

## Testing Requirements

### Unit Tests (Per Adapter - 120 tests total)
- ✅ Authentication (OAuth, API Key)
- ✅ Retry logic (429, 503, 504)
- ✅ Circuit breaker (5 failures → open)
- ✅ Timeout handling
- ✅ Error deserialization

### Integration Tests (Per Adapter - 30 tests total)
- ✅ End-to-end sync with WireMock
- ✅ SyncLog creation
- ✅ Manual retry from dashboard
- ✅ Concurrent syncs (5+ sponsors)

### Smoke Tests (Per Deployment)
1. Create test sponsor: `TEST_PILOT_001`
2. Verify PowerSchool Sponsor_OrgName updated
3. Verify SCP sponsor exists
4. Verify NetSuite customer created (sandbox)
5. Verify OBS CompanySponsors record exists
6. Delete test sponsor
7. Verify all targets reflect deletion

---

## Pilot Deployment Checklist

### Pre-Deployment
- [ ] All 11 secrets added to Azure Key Vault (Pilot)
- [ ] PowerSchool API endpoint confirmed with IT
- [ ] NetSuite sandbox account configured
- [ ] OBS database VPN tunnel tested
- [ ] Application Insights alerts configured
- [ ] Integration Retry Dashboard tested

### Deployment Day
- [ ] Deploy adapters to Pilot (feature flag `UseMocks = false`)
- [ ] Run smoke tests
- [ ] Monitor Application Insights for 2 hours
- [ ] Test manual retry for failed syncs
- [ ] Verify circuit breaker behavior (simulate downtime)

### Post-Deployment
- [ ] UAT with integration partners (PowerSchool, NetSuite, OBS)
- [ ] Performance testing (100+ sponsors)
- [ ] Security audit (OBS credentials)
- [ ] Documentation updated
- [ ] Production deployment approval

---

## Risk Mitigation

### High-Risk: OAuth 1.0a (NetSuite)
**Risk:** Signature generation errors → 401 failures  
**Mitigation:**
- Use RestSharp with OAuth extension
- Test with NetSuite sandbox first
- Have NetSuite signature examples ready

### Critical-Risk: OBS Credentials
**Risk:** Credential leakage → security breach  
**Mitigation:**
- Security code review required
- Never log passwords/hashes
- Audit all CompanySponsorAccount writes
- Use ASP.NET Core Identity PasswordHasher

### Medium-Risk: Circuit Breaker False Positive
**Risk:** Users blocked from saving sponsors  
**Mitigation:**
- Clear error message: "Integration temporarily unavailable - retry in 30s"
- Manual override for admins
- Monitor circuit breaker state in real-time

---

## Rollback Strategy

### If Integration Fails in Pilot/Production:
1. Set `IntegrationEndpoints:UseMocks = true` via Azure App Configuration
2. Restart app service
3. Mocks resume operation (no user impact)
4. Fix integration issue offline
5. Re-deploy real adapter after fix

**Rollback Time:** <5 minutes (config change + restart)

---

## Success Metrics

### Performance
- ✅ PowerSchool sync: <2 seconds
- ✅ SCP sync: <2 seconds
- ✅ NetSuite sync: <10 seconds
- ✅ OBS sync: <2 seconds

### Reliability
- ✅ Success rate: >98% (excluding external system downtime)
- ✅ Retry success rate: >80% (transient errors resolved)
- ✅ Circuit breaker false positive rate: <1%

### Security
- ✅ Zero credential leaks in logs
- ✅ All OBS account writes audited
- ✅ OAuth tokens cached (not requested per API call)

---

## Next Steps

1. **Review:** [Full Integration Readiness Plan](./INTEGRATION_READINESS_PLAN.md) - 100+ pages with complete implementation details
2. **Start:** Week 1, Day 1 - PowerSchool adapter development
3. **Coordinate:** Schedule kick-off meeting with PowerSchool, NetSuite, OBS teams
4. **Prepare:** Obtain API credentials and sandbox accounts

---

## Related Documentation
- [Integration Readiness Plan](./INTEGRATION_READINESS_PLAN.md) - Complete implementation guide
- [API Contract Plan](./API_CONTRACT_PLAN.md) - API specifications
- [PowerSchool Payload Examples](./POWERSCHOOL_PAYLOAD_EXAMPLES.md) - PowerSchool-specific details
- [Deployment Hardening Summary](./DEPLOYMENT_HARDENING_SUMMARY.md) - Infrastructure setup
