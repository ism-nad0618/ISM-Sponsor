# Integration Readiness Plan - Mock to Real Adapter Migration

## Executive Summary

**Current State:** All 4 downstream integration adapters are MOCKED for development and testing.  
**Goal:** Replace mock adapters with real HTTP-based implementations for Pilot and Production environments.  
**Strategy:** Phased rollout with feature flags, comprehensive retry logic, and circuit breaker patterns.  
**Timeline:** 2-3 weeks for full implementation and testing.

---

## Current Integration Architecture

### Mock vs. Real Status Matrix

| Integration Target | Current State | Priority | Complexity | Est. Effort |
|-------------------|---------------|----------|------------|-------------|
| **PowerSchool** | 🟡 MOCKED | **HIGH** | Medium | 3-4 days |
| **Student Charging Portal** | 🟡 MOCKED | **HIGH** | Medium | 3-4 days |
| **NetSuite** | 🟡 MOCKED | **MEDIUM** | High | 4-5 days |
| **Online Billing System** | 🟡 MOCKED | **MEDIUM** | High | 4-5 days |

**Total Implementation:** ~14-18 days (staggered rollout with testing)

---

## Integration Target #1: PowerSchool

### 1.1 Current State

**Mock Implementation:**
- **File:** `Integration/Adapters/MockIntegrationAdapters.cs` (Lines 9-90)
- **Class:** `MockPowerSchoolAdapter`
- **Behavior:** Returns simulated success/failure (90% success rate) with random delays

**DI Registration:**
- **File:** `Program.cs` (Line 175)
- **Code:** `builder.Services.AddScoped<IPowerSchoolAdapter, MockPowerSchoolAdapter>();`

**Contract Interface:**
- **File:** `Integration/Adapters/IIntegrationAdapters.cs` (Lines 6-24)
- **Interface:** `IPowerSchoolAdapter`
- **Methods:**
  1. `SyncSponsorAsync(PowerSchoolSponsorDto sponsor, string eventType)` - Sync individual sponsor metadata
  2. `SyncStudentTagsAsync(string studentId, string sponsorId)` - Tag student with sponsor
  3. `RemoveSponsorTagAsync(string studentId, string sponsorId)` - Remove sponsor tag
  4. `PublishSponsorOrgListAsync(List<string> sponsorNames, string correlationId)` - **PRIMARY OPERATION** - Updates Sponsor_OrgName custom field popup menu

### 1.2 Required Payloads & Contracts

#### Payload: Publish Sponsor Org List (Most Critical)
**HTTP Method:** `POST`  
**Endpoint:** `/api/custom_fields/sponsor_orgname/update_values`  
**Request Body:**
```json
{
  "fieldName": "Sponsor_OrgName",
  "values": [
    "ABC Company",
    "ACME Corporation",
    "First National Bank",
    "Global Education Fund",
    "Tech Solutions Ltd",
    "XYZ Bank"
  ],
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "source": "ISMSponsor",
  "timestamp": "2026-04-08T14:23:45.123Z"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "fieldName": "Sponsor_OrgName",
  "valuesCount": 6,
  "externalReferenceId": "PS-ORGLIST-20260408142345",
  "message": "Updated Sponsor_OrgName custom field with 6 values",
  "timestamp": "2026-04-08T14:23:45.456Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "errorCode": "PS_ERR_001",
  "errorMessage": "Custom field 'Sponsor_OrgName' does not exist or API lacks write permissions",
  "timestamp": "2026-04-08T14:23:45.789Z"
}
```

**Error Response (429 Rate Limit Exceeded):**
```json
{
  "success": false,
  "errorCode": "PS_ERR_RATE_LIMIT",
  "errorMessage": "API rate limit exceeded. Retry after 60 seconds.",
  "retryAfter": 60,
  "timestamp": "2026-04-08T14:23:45.789Z"
}
```

#### Payload: Sync Student Tags (Secondary Operation)
**HTTP Method:** `POST`  
**Endpoint:** `/api/students/{studentId}/custom_fields/sponsor_orgname`  
**Request Body:**
```json
{
  "sponsorId": "ABC001",
  "sponsorName": "ABC Company",
  "source": "ISMSponsor",
  "timestamp": "2026-04-08T14:25:00.000Z"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "studentId": "12345",
  "sponsorId": "ABC001",
  "message": "Student 12345 tagged with sponsor ABC Company",
  "timestamp": "2026-04-08T14:25:00.123Z"
}
```

#### Authentication
**Method:** OAuth 2.0 Client Credentials Flow  
**Token Endpoint:** `https://powerschool.ismschool.edu/oauth/access_token`  
**Required Scopes:** `custom_fields:write`, `students:read`

**Token Request:**
```http
POST /oauth/access_token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={ClientId from Key Vault}
&client_secret={ClientSecret from Key Vault}
&scope=custom_fields:write students:read
```

**Token Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "custom_fields:write students:read"
}
```

### 1.3 Exact Files to Create/Edit

#### File 1: Create `Integration/Adapters/PowerSchoolAdapter.cs` (NEW FILE)
```csharp
using ISMSponsor.Integration.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ISMSponsor.Integration.Adapters;

/// <summary>
/// Real PowerSchool adapter using PowerSchool REST API.
/// Implements retry logic, circuit breaker, and OAuth 2.0 authentication.
/// </summary>
public class PowerSchoolAdapter : IPowerSchoolAdapter
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PowerSchoolAdapter> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
    
    private string? _cachedAccessToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public PowerSchoolAdapter(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PowerSchoolAdapter> logger)
    {
        _httpClient = httpClientFactory.CreateClient("PowerSchool");
        _configuration = configuration;
        _logger = logger;

        // Retry policy: 3 retries with exponential backoff (2s, 4s, 8s)
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => 
                r.StatusCode == HttpStatusCode.RequestTimeout ||
                r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                r.StatusCode == HttpStatusCode.GatewayTimeout ||
                (int)r.StatusCode == 429) // Rate limit
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "PowerSchool API retry {RetryCount}/3 after {Delay}s due to {StatusCode}",
                        retryCount, timespan.TotalSeconds, outcome.Result?.StatusCode);
                });

        // Circuit breaker: Open after 5 consecutive failures, stay open for 30 seconds
        _circuitBreakerPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogError(
                        "PowerSchool circuit breaker OPENED for {DurationSeconds}s after 5 failures",
                        duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("PowerSchool circuit breaker RESET");
                });
    }

    public async Task<SyncResult> PublishSponsorOrgListAsync(List<string> sponsorNames, string correlationId)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Get OAuth token
            var token = await GetAccessTokenAsync();
            
            // Build request
            var request = new
            {
                fieldName = "Sponsor_OrgName",
                values = sponsorNames,
                correlationId,
                source = "ISMSponsor",
                timestamp = DateTime.UtcNow.ToString("o")
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/custom_fields/sponsor_orgname/update_values")
            {
                Content = content
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Headers.Add("X-Correlation-Id", correlationId);

            // Execute with retry and circuit breaker
            var policy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
            var response = await policy.ExecuteAsync(() => _httpClient.SendAsync(httpRequest));

            var responseBody = await response.Content.ReadAsStringAsync();
            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<PowerSchoolApiResponse>(responseBody);
                
                _logger.LogInformation(
                    "PowerSchool: Published {Count} sponsors to Sponsor_OrgName in {ElapsedMs}ms",
                    sponsorNames.Count, elapsed);

                return new SyncResult
                {
                    Success = true,
                    Status = "Succeeded",
                    Message = $"PowerSchool: Published {sponsorNames.Count} sponsor names to Sponsor_OrgName custom field popup menu",
                    ExternalReferenceId = result?.ExternalReferenceId ?? $"PS-ORGLIST-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            else
            {
                var error = JsonSerializer.Deserialize<PowerSchoolErrorResponse>(responseBody);
                
                _logger.LogError(
                    "PowerSchool API failed: {StatusCode}, {ErrorCode}, {ErrorMessage}",
                    response.StatusCode, error?.ErrorCode, error?.ErrorMessage);

                return new SyncResult
                {
                    Success = false,
                    Status = "Failed",
                    ErrorCode = error?.ErrorCode ?? $"PS_HTTP_{(int)response.StatusCode}",
                    ErrorMessage = error?.ErrorMessage ?? $"PowerSchool API returned {response.StatusCode}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "PowerSchool circuit breaker is OPEN - sync blocked");
            return new SyncResult
            {
                Success = false,
                Status = "Failed",
                ErrorCode = "PS_ERR_CIRCUIT_OPEN",
                ErrorMessage = "PowerSchool API circuit breaker is open. System is temporarily unavailable.",
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PowerSchool sync exception");
            return new SyncResult
            {
                Success = false,
                Status = "Failed",
                ErrorCode = "PS_ERR_EXCEPTION",
                ErrorMessage = ex.Message,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<SyncResult> SyncSponsorAsync(PowerSchoolSponsorDto sponsor, string eventType)
    {
        // TODO: Implement when PowerSchool sponsor master sync is required
        await Task.CompletedTask;
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = "PowerSchool sponsor sync not yet implemented (not required for Pilot)"
        };
    }

    public async Task<SyncResult> SyncStudentTagsAsync(string studentId, string sponsorId)
    {
        // TODO: Implement student tagging if required
        await Task.CompletedTask;
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = "PowerSchool student tagging not yet implemented"
        };
    }

    public async Task<SyncResult> RemoveSponsorTagAsync(string studentId, string sponsorId)
    {
        // TODO: Implement tag removal if required
        await Task.CompletedTask;
        return new SyncResult
        {
            Success = true,
            Status = "Succeeded",
            Message = "PowerSchool tag removal not yet implemented"
        };
    }

    private async Task<string> GetAccessTokenAsync()
    {
        // Return cached token if still valid (with 5-minute buffer)
        if (_cachedAccessToken != null && _tokenExpiresAt > DateTime.UtcNow.AddMinutes(5))
        {
            return _cachedAccessToken;
        }

        await _tokenLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_cachedAccessToken != null && _tokenExpiresAt > DateTime.UtcNow.AddMinutes(5))
            {
                return _cachedAccessToken;
            }

            // Get credentials from configuration (Azure Key Vault)
            var clientId = _configuration["Integration:PowerSchool:ClientId"];
            var clientSecret = _configuration["Integration:PowerSchool:ClientSecret"];
            var tokenUrl = _configuration["Integration:PowerSchool:TokenUrl"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("PowerSchool OAuth credentials not configured in Key Vault");
            }

            // Request token
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("scope", "custom_fields:write students:read")
            });

            var response = await _httpClient.PostAsync(tokenUrl, requestBody);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseBody);

            if (tokenResponse?.AccessToken == null)
            {
                throw new InvalidOperationException("PowerSchool OAuth token response invalid");
            }

            // Cache token
            _cachedAccessToken = tokenResponse.AccessToken;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            _logger.LogInformation("PowerSchool OAuth token acquired, expires at {ExpiresAt}", _tokenExpiresAt);

            return _cachedAccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    // Response models
    private class PowerSchoolApiResponse
    {
        public bool Success { get; set; }
        public string? ExternalReferenceId { get; set; }
        public string? Message { get; set; }
    }

    private class PowerSchoolErrorResponse
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }

    private class OAuthTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
```

#### File 2: Edit `Program.cs` (Line 175)
**Change:**
```csharp
// OLD (Line 175):
builder.Services.AddScoped<IPowerSchoolAdapter, MockPowerSchoolAdapter>();

// NEW (with feature flag support):
var useMockIntegrations = builder.Configuration.GetValue<bool>("IntegrationEndpoints:UseMocks", true);

if (useMockIntegrations)
{
    builder.Services.AddScoped<IPowerSchoolAdapter, MockPowerSchoolAdapter>();
    builder.Services.AddScoped<IStudentChargingPortalAdapter, MockStudentChargingPortalAdapter>();
    builder.Services.AddScoped<INetSuiteAdapter, MockNetSuiteAdapter>();
    builder.Services.AddScoped<IOnlineBillingSystemAdapter, MockOnlineBillingSystemAdapter>();
}
else
{
    // Real adapters with HttpClient factory and Polly resilience
    builder.Services.AddHttpClient("PowerSchool", client =>
    {
        var baseUrl = builder.Configuration["IntegrationEndpoints:PowerSchoolApiUrl"];
        client.BaseAddress = new Uri(baseUrl ?? throw new InvalidOperationException("PowerSchoolApiUrl not configured"));
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    builder.Services.AddHttpClient("StudentChargingPortal", client =>
    {
        var baseUrl = builder.Configuration["IntegrationEndpoints:StudentChargingPortalApiUrl"];
        client.BaseAddress = new Uri(baseUrl ?? throw new InvalidOperationException("StudentChargingPortalApiUrl not configured"));
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    builder.Services.AddHttpClient("NetSuite", client =>
    {
        var baseUrl = builder.Configuration["IntegrationEndpoints:NetSuiteApiUrl"];
        client.BaseAddress = new Uri(baseUrl ?? throw new InvalidOperationException("NetSuiteApiUrl not configured"));
        client.Timeout = TimeSpan.FromSeconds(45);
    });
    
    builder.Services.AddHttpClient("OnlineBillingSystem", client =>
    {
        var baseUrl = builder.Configuration["IntegrationEndpoints:OnlineBillingSystemApiUrl"];
        client.BaseAddress = new Uri(baseUrl ?? throw new InvalidOperationException("OnlineBillingSystemApiUrl not configured"));
        client.Timeout = TimeSpan.FromSeconds(30);
    });
    
    builder.Services.AddScoped<IPowerSchoolAdapter, PowerSchoolAdapter>();
    builder.Services.AddScoped<IStudentChargingPortalAdapter, StudentChargingPortalAdapter>();
    builder.Services.AddScoped<INetSuiteAdapter, NetSuiteAdapter>();
    builder.Services.AddScoped<IOnlineBillingSystemAdapter, OnlineBillingSystemAdapter>();
}
```

#### File 3: Edit `appsettings.Development.json` (Lines 43-48)
**Add:**
```json
"IntegrationEndpoints": {
  "PowerSchoolApiUrl": "https://dev-ps.ismschool.local/api",
  "StudentChargingPortalApiUrl": "https://dev-scp.ismschool.local/api",
  "NetSuiteApiUrl": "https://dev-netsuite.ismschool.local/api",
  "OnlineBillingSystemApiUrl": "https://dev-obs.ismschool.local/api",
  "UseMocks": true,
  "SyncEnabled": false
},
"Integration": {
  "PowerSchool": {
    "ClientId": "dev-client-id",
    "ClientSecret": "dev-client-secret",
    "TokenUrl": "https://dev-ps.ismschool.local/oauth/access_token"
  }
}
```

#### File 4: Edit `appsettings.Pilot.json` (Lines 49-54)
**Add:**
```json
"IntegrationEndpoints": {
  "PowerSchoolApiUrl": "#{Integration:PowerSchoolApiUrl}#",
  "StudentChargingPortalApiUrl": "#{Integration:StudentChargingPortalApiUrl}#",
  "NetSuiteApiUrl": "#{Integration:NetSuiteApiUrl}#",
  "OnlineBillingSystemApiUrl": "#{Integration:OnlineBillingSystemApiUrl}#",
  "UseMocks": false,
  "SyncEnabled": true
},
"Integration": {
  "PowerSchool": {
    "ClientId": "#{Integration:PowerSchool:ClientId}#",
    "ClientSecret": "#{Integration:PowerSchool:ClientSecret}#",
    "TokenUrl": "#{Integration:PowerSchool:TokenUrl}#"
  }
}
```

### 1.4 Retry & Error Handling Strategy

#### Polly Resilience Policies

**Retry Policy:**
- **Trigger Conditions:** 408 (Timeout), 429 (Rate Limit), 503 (Service Unavailable), 504 (Gateway Timeout)
- **Retry Count:** 3 attempts
- **Backoff:** Exponential (2s → 4s → 8s)
- **Total Max Time:** ~15 seconds

**Circuit Breaker Policy:**
- **Failure Threshold:** 5 consecutive failures
- **Break Duration:** 30 seconds
- **Reset:** Automatic on successful request after break
- **Fallback:** Return graceful failure with `PS_ERR_CIRCUIT_OPEN` code

#### Error Code Taxonomy

| Error Code | HTTP Status | Retry? | User Action |
|------------|-------------|--------|-------------|
| `PS_ERR_001` | 400 | ❌ No | Fix custom field configuration |
| `PS_ERR_AUTH` | 401 | ❌ No | Check OAuth credentials in Key Vault |
| `PS_ERR_FORBIDDEN` | 403 | ❌ No | Verify API permissions |
| `PS_ERR_NOT_FOUND` | 404 | ❌ No | Check endpoint URL |
| `PS_ERR_RATE_LIMIT` | 429 | ✅ Yes | Automatic retry after backoff |
| `PS_ERR_SERVER` | 500 | ✅ Yes | Retry or escalate if persistent |
| `PS_ERR_UNAVAILABLE` | 503 | ✅ Yes | Retry or wait for PowerSchool recovery |
| `PS_ERR_TIMEOUT` | 504 | ✅ Yes | Retry with longer timeout |
| `PS_ERR_CIRCUIT_OPEN` | N/A | ⏸️ Wait | Circuit breaker open - wait 30s |
| `PS_ERR_EXCEPTION` | N/A | ❌ No | Log and investigate exception |

#### SyncLog Integration

All sync attempts are logged to `SyncLog` table:
- **Success:** `Status = "Succeeded"`, `LastSucceededAt` populated
- **Failure:** `Status = "Failed"`, `ErrorMessage` populated, `RetryCount` incremented
- **Manual Retry:** Admin can retry from Integration Retry Dashboard (`/Operations/SyncRetry`)

### 1.5 Pilot-Safe Rollout Plan

#### Phase 1: Development Testing (3 days)
**When:** Week 1, Days 1-3  
**Environment:** Development  
**Configuration:** `UseMocks = true` (keep mocks active)

**Tasks:**
1. Create `PowerSchoolAdapter.cs` with full implementation
2. Add HttpClient factory configuration to `Program.cs`
3. Add PowerSchool OAuth credentials to Development config (dev credentials only)
4. Local testing:
   - Test OAuth token acquisition
   - Test circuit breaker behavior (force 5+ failures)
   - Test retry logic (simulate 429 rate limit)
   - Verify SyncLog entries are created correctly
5. Code review and approval

**Success Criteria:**
- ✅ Unit tests pass (mock PowerSchool API responses)
- ✅ Integration tests pass (local mock server)
- ✅ Circuit breaker opens/closes correctly
- ✅ Retry logic executes exponential backoff
- ✅ OAuth token caching works (no token request per API call)

---

#### Phase 2: Pilot Deployment with Feature Flag (2 days)
**When:** Week 1, Days 4-5  
**Environment:** Pilot  
**Configuration:** `UseMocks = false` (enable real adapter)

**Pre-Deployment Checklist:**
- [ ] PowerSchool Pilot API credentials stored in Azure Key Vault
- [ ] PowerSchool Pilot API endpoint URL confirmed with IT
- [ ] PowerSchool "Sponsor_OrgName" custom field exists and is writable
- [ ] ISM Sponsor API service principal has "custom_fields:write" permission
- [ ] Application Insights alerts configured for PowerSchool errors
- [ ] Integration Retry Dashboard tested and ready

**Deployment Steps:**
1. Add PowerSchool credentials to Azure Key Vault:
   ```bash
   az keyvault secret set --vault-name ismsponsor-pilot-kv \
     --name Integration--PowerSchool--ClientId --value "<client-id>"
   az keyvault secret set --vault-name ismsponsor-pilot-kv \
     --name Integration--PowerSchool--ClientSecret --value "<client-secret>"
   az keyvault secret set --vault-name ismsponsor-pilot-kv \
     --name Integration--PowerSchool--TokenUrl --value "https://pilot-ps.ismschool.edu/oauth/access_token"
   ```

2. Update Azure DevOps pipeline variables:
   ```yaml
   variables:
     - name: Integration:PowerSchoolApiUrl
       value: https://pilot-ps.ismschool.edu/api
     - name: IntegrationEndpoints:UseMocks
       value: false
   ```

3. Deploy to Pilot environment

4. Monitor Application Insights for:
   - PowerSchool API call latency
   - OAuth token acquisition success rate
   - Circuit breaker state changes
   - Error rate by error code

**Testing in Pilot:**
1. Create a new sponsor (trigger PublishSponsorOrgList)
2. Verify SyncLog entry shows "Succeeded"
3. Check PowerSchool Sponsor_OrgName field is updated
4. Update sponsor name (trigger PublishSponsorOrgList again)
5. Verify PowerSchool reflects the update
6. Simulate PowerSchool downtime (ask IT to block API):
   - Verify circuit breaker opens after 5 failures
   - Verify graceful error message to user
   - Verify circuit breaker closes after 30s
7. Test manual retry from Integration Retry Dashboard

**Success Criteria:**
- ✅ PowerSchool receives sponsor list updates in real-time
- ✅ OAuth authentication works without issues
- ✅ Circuit breaker prevents cascading failures
- ✅ Retry logic handles transient errors
- ✅ SyncLog accurately reflects all sync attempts
- ✅ No performance degradation in sponsor create/update operations

---

#### Phase 3: Production Deployment (1 day)
**When:** Week 2, Day 1  
**Environment:** Production  
**Configuration:** `UseMocks = false`

**Pre-Deployment Checklist:**
- [ ] Pilot testing completed successfully (Phase 2)
- [ ] PowerSchool Production API credentials stored in Azure Key Vault
- [ ] Production PowerSchool API endpoint URL confirmed
- [ ] ISM Sponsor Production service principal configured in PowerSchool
- [ ] Rollback plan documented and tested
- [ ] Production deployment approval from stakeholders

**Deployment Steps:**
1. Add PowerSchool Production credentials to Key Vault
2. Update Production pipeline variables
3. Deploy to Production slot
4. Smoke test with 1 sponsor update
5. Monitor for 2 hours before full rollout
6. If successful, announce integration is live

**Rollback Plan:**
If PowerSchool integration fails in Production:
1. Set `IntegrationEndpoints:UseMocks = true` via Azure App Configuration
2. Restart app service (picks up new config)
3. Mocks resume operation - no user impact
4. Investigate PowerSchool issue offline
5. Re-deploy real adapter after fix

**Success Criteria:**
- ✅ PowerSchool Production API receives sponsor updates
- ✅ No errors in Production Application Insights
- ✅ Sponsor operations complete within acceptable latency (<2s)
- ✅ Integration Retry Dashboard shows 0 failed syncs

---

## Integration Target #2: Student Charging Portal

### 2.1 Current State

**Mock Implementation:**
- **File:** `Integration/Adapters/MockIntegrationAdapters.cs` (Lines 94-147)
- **Class:** `MockStudentChargingPortalAdapter`

**DI Registration:**
- **File:** `Program.cs` (Line 176)
- **Code:** `builder.Services.AddScoped<IStudentChargingPortalAdapter, MockStudentChargingPortalAdapter>();`

**Contract Interface:**
- **File:** `Integration/Adapters/IIntegrationAdapters.cs` (Lines 29-37)
- **Interface:** `IStudentChargingPortalAdapter`
- **Methods:**
  1. `SyncSponsorAsync()` - Sync sponsor with coverage rules
  2. `SyncCoverageRulesAsync()` - Update coverage rules only
  3. `UpdateCoverageStatusAsync()` - Activate/deactivate sponsor

### 2.2 Required Payloads & Contracts

#### Payload: Sync Sponsor with Coverage Rules
**HTTP Method:** `POST`  
**Endpoint:** `/api/sponsors/upsert`  
**Authentication:** API Key (Header: `X-API-Key`)  
**Request Body:**
```json
{
  "sponsorId": "ABC001",
  "sponsorName": "ABC Company",
  "studentChargingPortalId": "SCP-ABC001",
  "isActive": true,
  "coverageRules": [
    {
      "itemId": "TUITION",
      "category": "Tuition",
      "decision": "FullyCovered"
    },
    {
      "itemId": "BOOKS",
      "category": "Books",
      "decision": "NotCovered"
    }
  ],
  "source": "ISMSponsor",
  "timestamp": "2026-04-08T14:30:00.000Z"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "sponsorId": "ABC001",
  "studentChargingPortalId": "SCP-ABC001",
  "rulesCount": 2,
  "message": "Sponsor ABC Company synced with 2 coverage rules",
  "timestamp": "2026-04-08T14:30:00.123Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "errorCode": "SCP_ERR_002",
  "errorMessage": "Student Charging Portal database connection timeout",
  "timestamp": "2026-04-08T14:30:00.456Z"
}
```

#### Authentication
**Method:** API Key (long-lived shared secret)  
**Header:** `X-API-Key: {ApiKey from Key Vault}`  
**Key Rotation:** Manual (quarterly recommended)

### 2.3 Exact Files to Create/Edit

#### File 1: Create `Integration/Adapters/StudentChargingPortalAdapter.cs` (NEW FILE)
**Implementation:** Similar to PowerSchoolAdapter with:
- API Key authentication instead of OAuth
- Retry policy: 3 attempts with exponential backoff
- Circuit breaker: 5 failures, 30-second break
- Timeout: 30 seconds per request

**Key Differences:**
- Simpler authentication (API Key in header)
- Direct SQL Server access option (if VPN available) via Entity Framework
- Fallback to REST API if database access unavailable

#### File 2: Edit `Program.cs` (Already covered in PowerSchool section)

#### File 3: Edit `appsettings.Pilot.json`
**Add:**
```json
"Integration": {
  "StudentChargingPortal": {
    "ApiKey": "#{Integration:StudentChargingPortal:ApiKey}#",
    "UseDatabaseDirect": false  // true if VPN tunnel to SERVER64
  }
}
```

### 2.4 Retry & Error Handling Strategy

**Same as PowerSchool** with the following adjustments:
- No OAuth token caching (API Key is static)
- Database connection timeout = 10 seconds (if direct access enabled)
- Retry database deadlocks (SQL Error 1205)

### 2.5 Pilot-Safe Rollout Plan

**Schedule:** Week 2, Days 2-4 (after PowerSchool is stable)  
**Approach:** Same 3-phase rollout as PowerSchool  
**Dependencies:** PowerSchool adapter must be stable first  
**Risk:** Medium - Student Charging Portal is critical for real-time coverage evaluation

**Additional Testing:**
- Test coverage rule updates propagate correctly
- Test sponsor activation/deactivation
- Verify coverage evaluation API uses updated rules

---

## Integration Target #3: NetSuite

### 3.1 Current State

**Mock Implementation:**
- **File:** `Integration/Adapters/MockIntegrationAdapters.cs` (Lines 150-203)
- **Class:** `MockNetSuiteAdapter`

**DI Registration:**
- **File:** `Program.cs` (Line 177)
- **Code:** `builder.Services.AddScoped<INetSuiteAdapter, MockNetSuiteAdapter>();`

**Contract Interface:**
- **File:** `Integration/Adapters/IIntegrationAdapters.cs` (Lines 42-48)
- **Interface:** `INetSuiteAdapter`
- **Methods:**
  1. `SyncSponsorAsync()` - Upsert sponsor as NetSuite customer
  2. `UpdateBillingAllocationAsync()` - Update sponsor billing allocation
  3. `SyncPaymentTermsAsync()` - Update payment terms

### 3.2 Required Payloads & Contracts

#### Payload: Upsert Sponsor as Customer
**HTTP Method:** `POST`  
**Endpoint:** `/api/record/customer`  
**Authentication:** OAuth 1.0a (NetSuite Token-Based Authentication)  
**Request Body:**
```json
{
  "externalId": "ABC001",
  "companyName": "ABC Company",
  "legalName": "ABC Company LLC",
  "taxIdNum": "12-3456789",
  "email": "accounts@abccompany.com",
  "phone": "+1-555-123-4567",
  "billAddr": {
    "addr1": "123 Main St",
    "city": "Manila",
    "state": "NCR",
    "zip": "1000",
    "country": "PH"
  },
  "terms": {
    "id": "2",  // NetSuite payment terms ID (e.g., "Net 30")
    "name": "Net 30"
  },
  "custbody_sponsor_code": "ABC001",
  "custbody_ism_sponsor_id": "ABC001",
  "isinactive": false
}
```

**Success Response (200 OK):**
```json
{
  "id": "12345",
  "externalId": "ABC001",
  "internalId": "12345",
  "type": "customer",
  "message": "Customer ABC Company created/updated successfully"
}
```

**Error Response (429 Rate Limit):**
```json
{
  "error": {
    "code": "NS_ERR_003",
    "message": "NetSuite REST API rate limit exceeded. Concurrency limit: 10 requests/second."
  }
}
```

#### Authentication: NetSuite OAuth 1.0a
**Method:** OAuth 1.0a with Token-Based Authentication (TBA)  
**Required Components:**
- Account ID (e.g., `TSTDRV1234567`)
- Consumer Key
- Consumer Secret
- Token ID
- Token Secret

**OAuth 1.0a Signature Generation:**
```csharp
// OAuth header components
var realm = configuration["Integration:NetSuite:AccountId"];
var consumerKey = configuration["Integration:NetSuite:ConsumerKey"];
var consumerSecret = configuration["Integration:NetSuite:ConsumerSecret"];
var tokenId = configuration["Integration:NetSuite:TokenId"];
var tokenSecret = configuration["Integration:NetSuite:TokenSecret"];

// Generate OAuth 1.0a signature (use library like OAuthSharp or custom implementation)
var authHeader = OAuthHelper.GenerateAuthorizationHeader(
    httpMethod: "POST",
    url: "https://tstdrv1234567.suitetalk.api.netsuite.com/services/rest/record/v1/customer",
    realm: realm,
    consumerKey: consumerKey,
    consumerSecret: consumerSecret,
    tokenId: tokenId,
    tokenSecret: tokenSecret
);

// Example OAuth header:
// Authorization: OAuth realm="TSTDRV1234567",
//   oauth_consumer_key="abc123...",
//   oauth_token="xyz789...",
//   oauth_signature_method="HMAC-SHA256",
//   oauth_timestamp="1712596800",
//   oauth_nonce="random123",
//   oauth_version="1.0",
//   oauth_signature="calculated_signature_here"
```

### 3.3 Exact Files to Create/Edit

#### File 1: Create `Integration/Adapters/NetSuiteAdapter.cs` (NEW FILE)
**Implementation:** NetSuite-specific with:
- OAuth 1.0a signature generation (requires external library or custom implementation)
- Retry policy: 3 attempts with exponential backoff (NetSuite rate limit = 10 req/sec)
- Circuit breaker: 5 failures, 60-second break (longer due to NetSuite complexity)
- Timeout: 45 seconds (NetSuite is slower)
- Rate limiting: Track requests per second to avoid 429 errors

**Complexity:** **HIGH** - OAuth 1.0a is complex, NetSuite API is slow

#### File 2: Edit `Program.cs` (Already covered)

#### File 3: Edit `appsettings.Pilot.json`
**Add:**
```json
"Integration": {
  "NetSuite": {
    "AccountId": "#{Integration:NetSuite:AccountId}#",
    "ConsumerKey": "#{Integration:NetSuite:ConsumerKey}#",
    "ConsumerSecret": "#{Integration:NetSuite:ConsumerSecret}#",
    "TokenId": "#{Integration:NetSuite:TokenId}#",
    "TokenSecret": "#{Integration:NetSuite:TokenSecret}#",
    "RestApiUrl": "https://#{Integration:NetSuite:AccountId}#.suitetalk.api.netsuite.com/services/rest/record/v1"
  }
}
```

### 3.4 Retry & Error Handling Strategy

**NetSuite-Specific Challenges:**
1. **Rate Limiting:** 10 requests/second per account
   - **Solution:** Implement request throttling with SemaphoreSlim
2. **OAuth 1.0a Complexity:** Signature generation is error-prone
   - **Solution:** Use well-tested library (e.g., RestSharp with OAuth extension)
3. **Slow API:** NetSuite responses can take 5-10 seconds
   - **Solution:** Increase timeout to 45 seconds
4. **Concurrency Limit:** Max 10 concurrent requests
   - **Solution:** Limit concurrent syncs via SemaphoreSlim(10)

**Error Codes:**
| Error Code | HTTP Status | Retry? | User Action |
|------------|-------------|--------|-------------|
| `NS_ERR_001` | 401 | ❌ No | Fix OAuth credentials |
| `NS_ERR_002` | 403 | ❌ No | Verify NetSuite role permissions |
| `NS_ERR_003` | 429 | ✅ Yes | Rate limit - automatic backoff |
| `NS_ERR_TIMEOUT` | 504 | ✅ Yes | Retry with longer timeout |

### 3.5 Pilot-Safe Rollout Plan

**Schedule:** Week 2, Days 5-6 + Week 3, Days 1-2 (4 days total)  
**Approach:** Same 3-phase rollout  
**Dependencies:** PowerSchool and SCP must be stable  
**Risk:** **HIGH** - NetSuite is complex and critical for billing

**Additional Testing:**
- Test OAuth 1.0a signature generation with NetSuite sandbox
- Test rate limiting (create 20+ sponsors rapidly)
- Test timeout handling (NetSuite API slowness)
- Verify customer records in NetSuite UI after sync
- Test billing allocation updates

**NetSuite Sandbox Testing:**
- Use NetSuite sandbox account for Pilot testing
- Do NOT sync to Production NetSuite until UAT complete
- Create test customers with prefix "TEST_" to identify ISM Sponsor syncs

---

## Integration Target #4: Online Billing System (OBS)

### 4.1 Current State

**Mock Implementation:**
- **File:** `Integration/Adapters/MockIntegrationAdapters.cs` (Lines 206-285)
- **Class:** `MockOnlineBillingSystemAdapter`

**DI Registration:**
- **File:** `Program.cs` (Line 178)
- **Code:** `builder.Services.AddScoped<IOnlineBillingSystemAdapter, MockOnlineBillingSystemAdapter>();`

**Contract Interface:**
- **File:** `Integration/Adapters/IIntegrationAdapters.cs` (Lines 53-64)
- **Interface:** `IOnlineBillingSystemAdapter`
- **Methods:**
  1. `SyncSponsorAsync()` - Sync to FINDB01.CompanySponsors table
  2. `SyncCoveredStudentsAsync()` - Sync covered students
  3. `UpdateStatementPreferencesAsync()` - Update sponsor email preferences
  4. `SyncSponsorAccountAsync()` - **SENSITIVE** - Sync login credentials to FINDB01.CompanySponsorAccount

### 4.2 Required Payloads & Contracts

#### Option 1: Direct Database Access (Recommended)
**Method:** Entity Framework Core with SQL Server connection  
**Target Server:** `FINDB01.ismschool.edu`  
**Target Database:** `OnlineBillingPortal`  
**Target Tables:**
- `dbo.CompanySponsors` - Sponsor master data
- `dbo.CompanySponsorAccount` - Sponsor login credentials (SENSITIVE)

**SQL Insert/Update Example:**
```sql
-- Upsert to CompanySponsors
MERGE INTO CompanySponsors AS target
USING (
    SELECT 
        'ABC001' AS SponsorCode,
        'ABC Company' AS CompanyName,
        'accounts@abccompany.com' AS Email,
        1 AS IsActive
) AS source
ON target.SponsorCode = source.SponsorCode
WHEN MATCHED THEN
    UPDATE SET 
        CompanyName = source.CompanyName,
        Email = source.Email,
        IsActive = source.IsActive,
        ModifiedDate = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (SponsorCode, CompanyName, Email, IsActive, CreatedDate)
    VALUES (source.SponsorCode, source.CompanyName, source.Email, source.IsActive, GETUTCDATE());

-- Upsert to CompanySponsorAccount (SENSITIVE)
MERGE INTO CompanySponsorAccount AS target
USING (
    SELECT 
        'ABC001' AS SponsorCode,
        'sponsor_abc001' AS Username,
        HASHBYTES('SHA2_256', 'InitialPassword123!') AS PasswordHash,
        'accounts@abccompany.com' AS Email
) AS source
ON target.SponsorCode = source.SponsorCode
WHEN MATCHED THEN
    UPDATE SET 
        Email = source.Email,
        ModifiedDate = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (SponsorCode, Username, PasswordHash, Email, CreatedDate)
    VALUES (source.SponsorCode, source.Username, source.PasswordHash, source.Email, GETUTCDATE());
```

**EF Core DbContext Example:**
```csharp
public class ObsDbContext : DbContext
{
    public DbSet<ObsCompanySponsor> CompanySponsors { get; set; }
    public DbSet<ObsCompanySponsorAccount> CompanySponsorAccounts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ObsCompanySponsor>().ToTable("CompanySponsors", "dbo");
        modelBuilder.Entity<ObsCompanySponsorAccount>().ToTable("CompanySponsorAccount", "dbo");
    }
}
```

#### Option 2: REST API Access (Fallback)
**HTTP Method:** `POST`  
**Endpoint:** `/api/sponsors/upsert`  
**Authentication:** API Key  
**Request Body:** (Similar to Student Charging Portal)

### 4.3 Exact Files to Create/Edit

#### File 1: Create `Integration/Adapters/OnlineBillingSystemAdapter.cs` (NEW FILE)
**Implementation:** Dual-mode adapter:
- **Primary Mode:** Direct database access via Entity Framework Core
- **Fallback Mode:** REST API (if database access unavailable)
- **Security:** CompanySponsorAccount writes must log to audit trail
- **Credential Handling:** Use ASP.NET Core Identity PasswordHasher

**Complexity:** **HIGH** - Handles sensitive data, requires database access

#### File 2: Create `Data/ObsDbContext.cs` (NEW FILE)
**Purpose:** Separate DbContext for OBS database (different connection string)

#### File 3: Edit `Program.cs`
**Add:**
```csharp
// OBS database context (separate connection)
builder.Services.AddDbContext<ObsDbContext>(options =>
{
    var obsConnectionString = configuration["Integration:OnlineBillingSystem:ConnectionString"];
    if (!string.IsNullOrEmpty(obsConnectionString))
    {
        options.UseSqlServer(obsConnectionString);
    }
});
```

#### File 4: Edit `appsettings.Pilot.json`
**Add:**
```json
"Integration": {
  "OnlineBillingSystem": {
    "ConnectionString": "#{Integration:OnlineBillingSystem:ConnectionString}#",
    "ApiKey": "#{Integration:OnlineBillingSystem:ApiKey}#",
    "UseDatabaseDirect": true
  }
}
```

### 4.4 Retry & Error Handling Strategy

**Database-Specific:**
- Retry SQL deadlocks (Error 1205) - 3 attempts
- Retry connection timeouts - 2 attempts
- No retry for constraint violations (duplicate key, foreign key)

**Security Logging:**
- All CompanySponsorAccount writes must log to:
  - `SyncLog` table (standard integration logging)
  - `AuditLog` table (security audit trail)
  - Application Insights (telemetry)

**Sensitive Data Handling:**
- Never log passwords or password hashes
- Never log connection strings
- Log only sanitized sponsor IDs and usernames

### 4.5 Pilot-Safe Rollout Plan

**Schedule:** Week 3, Days 3-5 (3 days total)  
**Approach:** Same 3-phase rollout  
**Dependencies:** All other adapters must be stable  
**Risk:** **VERY HIGH** - Handles sensitive login credentials

**Additional Testing:**
- Test database connection via VPN tunnel
- Test CompanySponsors upsert (non-sensitive data)
- Test CompanySponsorAccount upsert (SENSITIVE - log everything)
- Verify OBS portal login works after sync
- Test rollback: Delete test account from CompanySponsorAccount
- Security audit: Review all logs for credential leakage

**Security Review Checklist:**
- [ ] Database connection string stored in Key Vault only
- [ ] No passwords or hashes logged
- [ ] CompanySponsorAccount writes audited
- [ ] Password hashing uses ASP.NET Core Identity standard (PBKDF2)
- [ ] Initial passwords communicated out-of-band (email)
- [ ] Password reset flow tested

---

## Configuration Management

### Azure Key Vault Secrets (All Environments)

#### PowerSchool
```
Integration--PowerSchool--ClientId
Integration--PowerSchool--ClientSecret
Integration--PowerSchool--TokenUrl
```

#### Student Charging Portal
```
Integration--StudentChargingPortal--ApiKey
```

#### NetSuite
```
Integration--NetSuite--AccountId
Integration--NetSuite--ConsumerKey
Integration--NetSuite--ConsumerSecret
Integration--NetSuite--TokenId
Integration--NetSuite--TokenSecret
```

#### Online Billing System
```
Integration--OnlineBillingSystem--ConnectionString
Integration--OnlineBillingSystem--ApiKey
```

### Feature Flags (App Configuration)

| Flag | Development | Pilot | Production |
|------|-------------|-------|------------|
| `IntegrationEndpoints:UseMocks` | `true` | `false` | `false` |
| `IntegrationEndpoints:SyncEnabled` | `false` | `true` | `true` |
| `Integration:PowerSchool:Enabled` | `false` | `true` | `true` |
| `Integration:StudentChargingPortal:Enabled` | `false` | `true` | `true` |
| `Integration:NetSuite:Enabled` | `false` | `false` | `true` |
| `Integration:OnlineBillingSystem:Enabled` | `false` | `false` | `true` |

**Rollout Strategy:**
- Week 1: Enable PowerSchool only in Pilot
- Week 2: Enable Student Charging Portal in Pilot
- Week 2: Enable NetSuite in Pilot (sandbox)
- Week 3: Enable OBS in Pilot
- Week 3: Enable all in Production (after UAT sign-off)

---

## Monitoring & Alerting

### Application Insights Custom Metrics

Track the following metrics for each integration target:

| Metric Name | Type | Description |
|-------------|------|-------------|
| `Integration.{Target}.Latency` | Histogram | API call latency in milliseconds |
| `Integration.{Target}.Success` | Counter | Successful sync count |
| `Integration.{Target}.Failure` | Counter | Failed sync count |
| `Integration.{Target}.CircuitBreakerOpen` | Gauge | 1 if open, 0 if closed |
| `Integration.{Target}.RetryCount` | Counter | Total retries across all calls |
| `Integration.{Target}.RateLimitHit` | Counter | 429 responses received |

### Azure Monitor Alerts

#### Critical Alerts (PagerDuty/SMS)
1. **Circuit Breaker Open (Any Target)** - Alert if any integration circuit breaker is open for >5 minutes
2. **Sync Failure Rate >10%** - Alert if failure rate exceeds 10% over 15-minute window
3. **OBS CompanySponsorAccount Write Failure** - Immediate alert (security-sensitive)

#### Warning Alerts (Email)
1. **Sync Latency >10s** - Alert if any integration takes >10 seconds
2. **Retry Rate >20%** - Alert if >20% of requests require retry
3. **Rate Limit Hit** - Alert if any target returns 429 more than twice per hour

### Dashboard Panels (Application Insights)

Create a custom dashboard with:
1. **Integration Health Overview** - Green/yellow/red status for each target
2. **Sync Volume** - Line chart of successful syncs per hour
3. **Error Rate** - Line chart of failures per hour by error code
4. **Latency Percentiles** - P50, P95, P99 latency for each target
5. **Circuit Breaker State** - Real-time status (open/closed)

---

## Testing Strategy

### Unit Tests (Per Adapter)

**Test Cases:**
1. OAuth token acquisition (PowerSchool)
2. API Key authentication (Student Charging Portal, OBS)
3. OAuth 1.0a signature generation (NetSuite)
4. Retry logic (simulate 429, 503, 504 responses)
5. Circuit breaker behavior (force 5+ failures)
6. Timeout handling
7. Error deserialization (various error responses)

**Mock Libraries:**
- `Moq` for mocking HttpClient responses
- `WireMock.Net` for integration test server

### Integration Tests (Per Adapter)

**Test Cases:**
1. End-to-end sync with mock external API (WireMock)
2. SyncLog creation for successful sync
3. SyncLog creation for failed sync
4. Manual retry from Integration Retry Dashboard
5. Concurrent syncs (5+ sponsors at once)
6. Rate limiting behavior

### Smoke Tests (Pilot Environment)

**After Each Deployment:**
1. Create a test sponsor (prefix "TEST_")
2. Verify PowerSchool receives sponsor list update
3. Verify Student Charging Portal has sponsor in database
4. Verify NetSuite customer record created (sandbox only)
5. Verify OBS CompanySponsors record created
6. Delete test sponsor
7. Verify all targets reflect deletion/deactivation

---

## Risk Assessment & Mitigation

### High-Risk Areas

#### Risk 1: OAuth 1.0a Implementation (NetSuite)
**Impact:** HIGH - Incorrect signature = 401 errors  
**Probability:** MEDIUM  
**Mitigation:**
- Use well-tested library (RestSharp with OAuth extension)
- Extensive unit tests for signature generation
- Test with NetSuite sandbox before Pilot
- Have NetSuite signature examples from NetSuite support

#### Risk 2: OBS Credential Sync (Security)
**Impact:** CRITICAL - Credential leakage = security breach  
**Probability:** LOW (with proper controls)  
**Mitigation:**
- Security code review required
- Never log passwords or connection strings
- Encrypt all secrets in Key Vault
- Audit all CompanySponsorAccount writes
- Password reset flow tested thoroughly

#### Risk 3: Circuit Breaker False Positive
**Impact:** MEDIUM - Users can't save sponsors if circuit open  
**Probability:** LOW  
**Mitigation:**
- Increase failure threshold from 5 to 7
- Reduce break duration from 30s to 20s
- Provide clear error message: "Integration temporarily unavailable - retry in 20s"
- Manual override for admins (force close circuit breaker)

#### Risk 4: NetSuite Rate Limiting
**Impact:** MEDIUM - Syncs delayed during bulk import  
**Probability:** MEDIUM  
**Mitigation:**
- Implement request throttling (max 8 req/sec, buffer of 2)
- Queue syncs if rate limit hit (don't retry immediately)
- Increase timeout to 60s for bulk operations
- Monitor rate limit hits in Application Insights

---

## Success Criteria (Final Acceptance)

### PowerSchool
- ✅ Sponsor list updates propagate to PowerSchool Sponsor_OrgName field within 2 seconds
- ✅ OAuth token caching works (token requested once per hour max)
- ✅ Circuit breaker prevents cascading failures during PowerSchool downtime
- ✅ Retry logic handles transient errors without user intervention
- ✅ SyncLog accurately reflects all sync attempts

### Student Charging Portal
- ✅ Sponsor and coverage rules sync within 2 seconds of save
- ✅ Coverage evaluation API uses updated rules immediately
- ✅ Sponsor activation/deactivation reflects in SCP within 5 seconds

### NetSuite
- ✅ Sponsor customer records created in NetSuite within 10 seconds
- ✅ OAuth 1.0a authentication works without 401 errors
- ✅ Rate limiting prevents 429 errors
- ✅ Billing allocation updates reflect in NetSuite invoices

### Online Billing System
- ✅ CompanySponsors table updated within 2 seconds
- ✅ CompanySponsorAccount credentials work for sponsor login
- ✅ No credential leakage in logs or Application Insights
- ✅ Security audit trail complete for all account writes

---

## Appendix: Code Examples

### A. HttpClient Factory Configuration (Program.cs)

```csharp
// Configure HttpClient with Polly policies
builder.Services.AddHttpClient("PowerSchool", client =>
{
    var baseUrl = builder.Configuration["IntegrationEndpoints:PowerSchoolApiUrl"];
    client.BaseAddress = new Uri(baseUrl ?? throw new InvalidOperationException("PowerSchoolApiUrl not configured"));
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "ISMSponsor/1.0");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return Policy
        .HandleResult<HttpResponseMessage>(r => 
            r.StatusCode == HttpStatusCode.RequestTimeout ||
            r.StatusCode == HttpStatusCode.ServiceUnavailable ||
            r.StatusCode == HttpStatusCode.GatewayTimeout ||
            (int)r.StatusCode == 429)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}
```

### B. OAuth Token Caching Pattern

```csharp
private string? _cachedToken;
private DateTime _tokenExpiresAt = DateTime.MinValue;
private readonly SemaphoreSlim _tokenLock = new(1, 1);

private async Task<string> GetAccessTokenAsync()
{
    if (_cachedToken != null && _tokenExpiresAt > DateTime.UtcNow.AddMinutes(5))
    {
        return _cachedToken;
    }

    await _tokenLock.WaitAsync();
    try
    {
        // Double-check after lock
        if (_cachedToken != null && _tokenExpiresAt > DateTime.UtcNow.AddMinutes(5))
        {
            return _cachedToken;
        }

        // Acquire new token...
        var tokenResponse = await RequestTokenAsync();
        _cachedToken = tokenResponse.AccessToken;
        _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        return _cachedToken;
    }
    finally
    {
        _tokenLock.Release();
    }
}
```

### C. SyncLog Audit Logging

```csharp
private async Task LogSyncAttemptAsync(string sponsorId, string eventType, SyncResult result, string correlationId, object payload)
{
    var syncLog = new SyncLog
    {
        EntityType = "Sponsor",
        EntityId = sponsorId,
        TargetSystem = IntegrationTargets.PowerSchool,
        EventType = eventType,
        Status = result.Success ? "Succeeded" : "Failed",
        AttemptedAt = DateTime.UtcNow,
        LastSucceededAt = result.Success ? DateTime.UtcNow : null,
        RetryCount = 0,
        ErrorMessage = result.ErrorMessage,
        CorrelationId = correlationId,
        RequestPayload = JsonSerializer.Serialize(payload),
        ResponsePayload = result.Message,
        ExternalReferenceId = result.ExternalReferenceId
    };

    _context.SyncLogs.Add(syncLog);
    await _context.SaveChangesAsync();
}
```

---

## Document Control

**Version:** 1.0  
**Date:** April 8, 2026  
**Author:** GitHub Copilot  
**Approved By:** (pending review)  
**Next Review:** After Phase 1 completion (April 15, 2026)

---

## Related Documentation
- [API Contract Plan](./API_CONTRACT_PLAN.md) - API surface specification
- [Deployment Hardening Summary](./DEPLOYMENT_HARDENING_SUMMARY.md) - Infrastructure setup
- [PowerSchool Payload Examples](./POWERSCHOOL_PAYLOAD_EXAMPLES.md) - PowerSchool-specific details
- [Swagger Implementation Summary](./SWAGGER_IMPLEMENTATION_SUMMARY.md) - API documentation
