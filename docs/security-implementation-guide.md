# Security Implementation Guide - ISM Sponsor Management System

## Purpose
This document maps implemented security controls to OWASP Top 10 2021 risks and provides configuration guidance for secure deployment.

---

## OWASP Top 10 2021 Coverage

### A01: Broken Access Control
**Risk**: Unauthorized users accessing protected resources or performing unauthorized actions.

**Implemented Controls**:
1. **Role-Based Access Control (RBAC)**
   - Authorization attributes: `[AdminOnly]`, `[StaffOnly]`, `[SponsorOnly]`, `[AdminOrAdmissions]`
   - Location: `/Security/AuthorizationAttributes.cs`
   - Enforced at controller level for all protected actions

2. **Authorization Filters**
   - Custom authorization filters log access denial attempts
   - Failed authorization logged to security audit trail
   - Location: `SecurityAuditService.LogAuthorizationFailureAsync()`

3. **UI Access Control**
   - View-level role checks: `@if (User.IsInRole("admin"))`
   - Navigation elements hidden for unauthorized roles
   - Backend enforcement remains even if UI is bypassed

4. **Sponsor Isolation**
   - Sponsor users explicitly denied access to staff functions
   - `SponsorOnlyFilter` validates role and denies staff access
   - Staff roles cannot access sponsor-only resources

**Testing**:
```csharp
// Tests/Security/AuthorizationTests.cs
[Fact]
public async Task SponsorUser_CannotAccess_AdminSettings()
[Fact]
public async Task UnauthenticatedUser_CannotAccess_Dashboard()
[Fact]
public async Task CashierUser_CannotAccess_MergeOperation()
```

**Verification Steps**:
1. Attempt to access `/Settings/Users` as non-admin user → Expect 403 Forbidden
2. Attempt to access `/Duplicates*` as non-admin user → Expect 403 Forbidden
3. Attempt sponsor user access to `/Dashboard` → Expect 403 Forbidden

---

### A02: Cryptographic Failures
**Risk**: Sensitive data exposure through weak encryption or transmission.

**Implemented Controls**:
1. **Transport Security**
   - HTTPS enforced in production/pilot: `UseHttpsRedirection()`
   - HSTS enabled: `UseHsts()` with 1-year max-age
   - Configuration: `appsettings.Pilot.json` → `Security:UseHttpsRedirection=true`

2. **Secure Cookies**
   - `HttpOnly=true`: Prevents JavaScript access
   - `Secure=Always` (Pilot/Prod): Requires HTTPS
   - `SameSite=Strict`: CSRF protection
   - Configuration: `Security:CookieSecurePolicy`

3. **Password Storage**
   - ASP.NET Core Identity default: PBKDF2 with 10,000 iterations
   - Salt automatically generated per user
   - No plaintext password storage

4. **Secrets Management**
   - Azure Key Vault integration prepared
   - Configuration validation prevents placeholder secrets in production
   - Secrets injected via environment variables or Key Vault references

**Configuration**:
```json
// appsettings.Pilot.json
"Security": {
  "UseHttpsRedirection": true,
  "UseHsts": true,
  "CookieSecurePolicy": "Always"
}
```

**Verification Steps**:
1. Check response headers: `curl -I https://[url]` → Expect `Strict-Transport-Security` header
2. Inspect auth cookie: `Secure=true`, `HttpOnly=true`, `SameSite=Strict`
3. Confirm HTTPS redirect: `curl http://[url]` → Expect 307 redirect to HTTPS

---

### A03: Injection
**Risk**: SQL injection, command injection, or other injection attacks.

**Implemented Controls**:
1. **Parameterized Queries**
   - Entity Framework Core used for all database access
   - Automatic SQL parameterization
   - No raw SQL queries with string concatenation

2. **Input Validation**
   - Model validation with Data Annotations
   - Anti-forgery tokens on all form posts
   - Content-Type validation on API requests

3. **Output Encoding**
   - Razor automatic HTML encoding: `@Model.SponsorName`
   - JavaScript encoding where needed
   - JSON serialization via System.Text.Json (safe defaults)

**Code Examples**:
```csharp
// SAFE: Parameterized query via EF Core
var sponsor = await _context.Sponsors
    .Where(s => s.SponsorId == sponsorId)
    .FirstOrDefaultAsync();

// SAFE: Anti-forgery protection
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(SponsorViewModel model)
```

**Verification Steps**:
1. Attempt SQL injection in search: `' OR 1=1--` → Expect safe parameterized query
2. Attempt XSS in sponsor name: `<script>alert('XSS')</script>` → Expect HTML-encoded output
3. Verify anti-forgery token required on POST actions

---

### A04: Insecure Design
**Risk**: Missing or ineffective security controls in application design.

**Implemented Controls**:
1. **Fail-Safe Defaults**
   - Configuration validation on startup (fail fast)
   - Default deny for protected resources
   - Lockout after failed authentication attempts

2. **Separation of Duties**
   - Clear role separation: Admin, Admissions, Cashier, Sponsor
   - Merge operations require Admin role
   - Sponsor approval requires Admissions role

3. **Audit Trail Design**
   - All security events logged (authentication, authorization, admin actions)
   - Tamper-evident: ActivityLog entries not editable through app
   - Timestamps in UTC for consistency

4. **Defense in Depth**
   - Multiple security layers: authentication → authorization → input validation
   - UI hides actions, backend still enforces
   - Security headers add additional protection

**Design Principles**:
- **Least Privilege**: Users get minimum required access
- **Complete Mediation**: Every access checked
- **Fail-Safe Defaults**: Deny by default, allow explicitly
- **Separation of Privilege**: Critical actions require elevated roles

---

### A05: Security Misconfiguration
**Risk**: Insecure default configurations or missing security hardening.

**Implemented Controls**:
1. **Security Headers Middleware**
   - `X-Content-Type-Options: nosniff`
   - `X-Frame-Options: DENY`
   - `X-XSS-Protection: 1; mode=block`
   - `Content-Security-Policy` configurable
   - `Referrer-Policy: strict-origin-when-cross-origin`
   - `Server` header removed (information disclosure prevention)
   - Location: `/Middleware/SecurityHeadersMiddleware.cs`

2. **Error Handling**
   - Generic error messages in production
   - Detailed errors only in development
   - Stack traces never exposed in production
   - Location: `/Middleware/GlobalExceptionHandlerMiddleware.cs`

3. **Configuration Validation**
   - Startup validation for required settings
   - Fails fast if misconfigured
   - Validates: database connection, auth settings, security settings
   - Location: `/Services/ConfigurationValidationService.cs`

4. **Environment-Specific Settings**
   - Development: relaxed settings, detailed errors
   - Pilot/Production: strict settings, generic errors
   - Configuration files: `appsettings.Development.json`, `appsettings.Pilot.json`

**Production Requirements** (enforced by ConfigurationValidationService):
```json
{
  "Security": {
    "UseHttpsRedirection": true,  // MUST be true
    "UseHsts": true,              // MUST be true
    "CookieSecurePolicy": "Always", // MUST be Always
    "AntiForgeryEnabled": true    // MUST be true
  }
}
```

**Verification Steps**:
1. Start app with invalid config → Expect startup failure with clear error message
2. Check response headers → Expect all security headers present
3. Trigger error in production → Expect generic message, no stack trace

---

### A06: Vulnerable and Outdated Components
**Risk**: Using components with known vulnerabilities.

**Implemented Controls**:
1. **Dependency Scanning in CI/CD**
   - OWASP Dependency Check in pipeline
   - Scans all NuGet packages
   - Fails build on CVSS >= 7.0
   - Location: `azure-pipelines.yml` → SecurityScan stage

2. **Regular Updates**
   - .NET 8.0 LTS with latest patches
   - NuGet packages updated regularly
   - Dependency review in pull requests

3. **Automated Scanning**
   - Every build runs dependency check
   - Results published to Azure DevOps
   - Security team notified of new vulnerabilities

**Pipeline Configuration**:
```yaml
- task: dependency-check-build-task@6
  displayName: 'OWASP Dependency Check'
  inputs:
    projectName: 'ISM Sponsor'
    failOnCVSS: 7
```

**Verification Steps**:
1. Run pipeline → Check SecurityScan stage passes
2. Review dependency-check report in build artifacts
3. Verify no high/critical vulnerabilities (CVSS >= 7.0)

---

### A07: Identification and Authentication Failures
**Risk**: Weak authentication mechanisms or poor session management.

**Implemented Controls**:
1. **Strong Password Requirements**
   - Minimum 8 characters (configurable)
   - Requires digit, uppercase, non-alphanumeric
   - Configuration: `SponsorAuth:PasswordRequirements`

2. **Account Lockout**
   - 5 failed attempts → 15-30 minute lockout
   - Prevents brute force attacks
   - Configuration: `SponsorAuth:LockoutSettings`

3. **Session Security**
   - 8-hour authentication timeout
   - Sliding expiration (resets on activity)
   - Secure session cookies
   - Session cleared on logout

4. **Authentication Event Logging**
   - All login attempts logged
   - Failed attempts logged with IP address
   - Suspicious activity detection
   - Location: `SecurityAuditService.LogAuthenticationSuccessAsync/FailureAsync()`

5. **Multi-Factor Authentication Ready**
   - ASP.NET Core Identity supports 2FA
   - Can be enabled via configuration
   - TOTP token support built-in

**Configuration**:
```json
"SponsorAuth": {
  "PasswordRequirements": {
    "RequireDigit": true,
    "RequireNonAlphanumeric": true,
    "RequireUppercase": true,
    "RequiredLength": 8
  },
  "LockoutSettings": {
    "MaxFailedAccessAttempts": 5,
    "LockoutDurationMinutes": 30
  }
}
```

**Verification Steps**:
1. Attempt 5 failed logins → Expect account lockout
2. Check ActivityLog table → Expect failed login entries with IP address
3. Log in and check cookie expiration → Expect 8-hour timeout
4. Log out → Expect session cleared, auth cookie removed

---

### A08: Software and Data Integrity Failures
**Risk**: Unsigned code, CI/CD pipeline compromise, tampering with audit logs.

**Implemented Controls**:
1. **Audit Trail Integrity**
   - ActivityLog entries created only
   - No update or delete operations through normal app flow
   - All admin actions logged with timestamp, user, IP
   - Location: `SecurityAuditService`

2. **Coverage Evaluation Audit**
   - CoverageEvaluationAudit stores: rule version, decision, timestamp
   - Immutable after creation
   - Traces decision logic for compliance

3. **Merge Operation Audit**
   - MergeOperation stores before/after snapshots
   - Full record of what changed, when, by whom
   - Cannot be modified after completion

4. **CI/CD Pipeline Security**
   - Code review required before merge
   - Automated tests must pass
   - Manual approval for pilot deployment
   - Artifact integrity via Azure DevOps

**Audit Logging**:
```csharp
// All security events logged
await _securityAudit.LogAuthenticationSuccessAsync(userId, email, "AzureAD");
await _securityAudit.LogAuthorizationFailureAsync(userId, email, resource, role);
await _securityAudit.LogAdminActionAsync(userId, email, "Merge", "Sponsor", sponsorId, details);
```

**Verification Steps**:
1. Perform admin action → Check ActivityLog for entry
2. Attempt to modify ActivityLog via app → Expect no update method available
3. Review merge operation → Verify before/after snapshots present

---

### A09: Security Logging and Monitoring Failures
**Risk**: Insufficient logging to detect breaches or diagnose security incidents.

**Implemented Controls**:
1. **Comprehensive Security Logging**
   - Authentication events (success/failure)
   - Authorization failures
   - Admin actions
   - Security configuration changes
   - Data integrity alerts
   - Suspicious activity
   - Location: `/Services/SecurityAuditService.cs`

2. **Structured Logging**
   - ASP.NET Core logging framework
   - Application Insights integration ready
   - Log levels: Critical, Error, Warning, Information
   - Contextual data: User, IP, timestamp, resource

3. **Log Retention**
   - ActivityLog table persists indefinitely
   - Database backup retention per compliance policy
   - Azure Monitor for real-time alerts

4. **Health Monitoring**
   - Health check endpoints: `/health`, `/api/health/detailed`
   - Monitors: database, configuration, sync, audit
   - Location: `/HealthChecks/ApplicationHealthChecks.cs`

**Logged Events**:
- ✓ User login (success/failure)
- ✓ User logout
- ✓ Authorization denied
- ✓ Admin action performed
- ✓ Security config changed
- ✓ Data integrity concern
- ✓ Suspicious activity
- ✓ Merge operation executed
- ✓ Sync failure

**Verification Steps**:
1. Log in → Check ActivityLog for authentication success entry
2. Fail login → Check ActivityLog for authentication failure withIP
3. Access denied → Check ActivityLog for authorization failure
4. Check health endpoint → `/api/health/detailed` shows component status

---

## Authentication Architecture

### Staff Authentication (Azure AD)
**Primary Method**: Azure AD OAuth 2.0 / OpenID Connect

**Configuration**:
```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "[from Azure Key Vault]",
  "ClientId": "[from Azure Key Vault]",
  "ClientSecret": "[from Azure Key Vault]",
  "Domain": "ismschool.onmicrosoft.com",
  "AllowedDomains": ["ismschool.onmicrosoft.com", "ismmanila.org"]
}
```

**Flow**:
1. User clicks "Staff Login"
2. Redirect to Azure AD
3. User authenticates with Azure AD credentials
4. Azure AD returns ID token
5. Application validates token
6. User role assigned based on Azure AD group membership
7. Session created

**Roles**: Admin, Admissions, Cashier (from Azure AD groups)

---

### Sponsor Authentication (Local Identity)
**Method**: ASP.NET Core Identity with local database

**Configuration**:
```json
"SponsorAuth": {
  "RequireEmailConfirmation": true,
  "PasswordRequirements": {
    "RequireDigit": true,
    "RequireNonAlphanumeric": true,
    "RequireUppercase": true,
    "RequiredLength": 8
  }
}
```

**Flow**:
1. Sponsor navigates to sponsor portal
2. Uses email/password credentials
3. ASP.NET Core Identity validates
4. Role = "sponsor" assigned
5. SponsorId claim added (links to Sponsor record)
6. Session created

**Sponsor Isolation**: Sponsor users have `SponsorId` claim, can only access own data.

---

## Secrets Management

### Azure Key Vault Integration (Production)
**Configuration**:
```csharp
// Program.cs (to be added for production)
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

**Secrets to Store in Key Vault**:
- `DatabaseConnectionString`: SQL Server connection string
- `AzureAd--ClientSecret`: Azure AD app secret
- `ApplicationInsights--ConnectionString`: App Insights connection
- `Integration--*`: External API keys/secrets

**Secret Naming Convention**:
- Key Vault: `ISMSponsor-Pilot-DbConnectionString`
- Configuration: `ConnectionStrings:DefaultConnection`
- Mapping: `--` in secret name = `:` in config path

**Verification**:
```bash
# List secrets in Key Vault
az keyvault secret list --vault-name ism-sponsor-pilot-kv

# Get secret value
az keyvault secret show --vault-name ism-sponsor-pilot-kv --name ISMSponsor-Pilot-DbConnectionString
```

---

## Security Testing

### Unit Tests
Location: `/Tests/Security/`

**Test Coverage**:
- `AuthorizationTests.cs`: RBAC enforcement
- `AuthenticationTests.cs`: Login/logout behavior
- `SessionTests.cs`: Session handling
- `SecurityHeadersTests.cs`: Headers applied correctly
- `AntiForgeryTests.cs`: CSRF protection

**Example**:
```csharp
[Fact]
public async Task AdminOnlyAttribute_DeniesNonAdminUser()
{
    // Arrange
    var user = CreateTestUser(role: "admissions");
    
    // Act
    var result = await InvokeAdminOnlyAction(user);
    
    // Assert
    result.Should().BeOfType<ForbidResult>();
}
```

### Integration Tests
**Test Scenarios**:
1. Full authentication flow (Azure AD mock)
2. Authorization at API level
3. Session expiration and renewal
4. Security headers in responses
5. Health check endpoint availability

### Security Scan Tests
**CI/CD Pipeline**:
- OWASP Dependency Check (vulnerabilities)
- Credential Scanner (secrets in code)
- Static code analysis

---

## Deployment Checklist

### Pre-Deployment
- [ ] Configuration validated (no placeholders)
- [ ] Secrets stored in Azure Key Vault
- [ ] Database migration tested
- [ ] Security scans passed (no high/critical issues)
- [ ] Unit tests passed
- [ ] Integration tests passed

### Deployment
- [ ] Deploy to staging slot first
- [ ] Run smoke tests
- [ ] Check health endpoints
- [ ] Verify security headers
- [ ] Test authentication/authorization
- [ ] Swap to production slot

### Post-Deployment
- [ ] Monitor Application Insights for errors
- [ ] Check security audit logs
- [ ] Verify no configuration errors logged
- [ ] Test critical user flows
- [ ] Monitor for 1 hour before declaring success

---

## Incident Response

### Security Incident Triggers
1. Account lockout disabled unexpectedly
2. Multiple authorization failures from same IP
3. Audit log entries missing or tampered
4. Unusual admin actions
5. High volume of authentication failures
6. Sync failure rate > 50%

### Response Actions
1. **Detect**: Monitor alerts in Application Insights
2. **Assess**: Review security audit logs
3. **Contain**: Disable affected accounts if needed
4. **Eradicate**: Fix vulnerability or misconfiguration
5. **Recover**: Restore to known good state
6. **Learn**: Update security controls and tests

---

## Compliance Mapping

| Control | OWASP | Location | Test Coverage |
|---------|-------|----------|---------------|
| RBAC Enforcement | A01 | `Security/AuthorizationAttributes.cs` | `AuthorizationTests.cs` |
| HTTPS/HSTS | A02 | `Program.cs` (middleware) | `SecurityHeadersTests.cs` |
| Parameterized Queries | A03 | All EF Core queries | Integration tests |
| Configuration Validation | A05 | `ConfigurationValidationService.cs` | `ConfigurationTests.cs` |
| Dependency Scanning | A06 | `azure-pipelines.yml` | Pipeline stage |
| Password Policy | A07 | `Program.cs` (Identity config) | `AuthenticationTests.cs` |
| Audit Trail | A08 | `SecurityAuditService.cs` | `AuditIntegrityTests.cs` |
| Security Logging | A09 | `SecurityAuditService.cs` | `LoggingTests.cs` |

---

## Additional Resources

- [OWASP Top 10 2021](https://owasp.org/Top10/)
- [ASP.NET Core Security Best Practices](https://learn.microsoft.com/en-us/aspnet/core/security/)
- [Azure Key Vault Documentation](https://learn.microsoft.com/en-us/azure/key-vault/)
- [Application Insights Security](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

---

**Document Version**: 1.0  
**Last Updated**: March 2026  
**Owner**: ISM Technology Team
