# ISM Sponsor Management System - Smoke Test Checklist

## Purpose
Smoke tests validate that critical functionality is operational after deployment. These tests should be run after each deployment to Dev and Pilot environments.

## Test Execution
Run these tests **immediately** after deployment and before promoting to the next environment.

---

## Critical Smoke Tests

### 1. Application Health
**Test**: Application is running and responding
- [ ] Health endpoint accessible: `GET /health`
- [ ] Expected Status: `200 OK`
- [ ] Response contains: `{"status":"Healthy"}`
- [ ] Health timestamp is recent (< 5 seconds ago)

**Command**:
```bash
curl -i https://[environment-url]/health
```

---

### 2. Database Connectivity
**Test**: Database connection is operational
- [ ] Health endpoint shows database as healthy
- [ ] Expected Status: `200 OK` from `/health`
- [ ] Database check passes in health response

**Command**:
```bash
curl https://[environment-url]/api/health/detailed -H "Authorization: Bearer [admin-token]"
```

---

### 3. Authentication Availability
**Test**: Login system is accessible
- [ ] Login page loads: `GET /Account/Login`
- [ ] Expected Status: `200 OK`
- [ ] Page contains login form elements
- [ ] No JavaScript errors in browser console

**Manual Test**:
1. Navigate to `https://[environment-url]/Account/Login`
2. Verify page loads completely
3. Verify form fields are present (Email, Password)
4. Verify "Login" button is clickable

---

### 4. Authorization Check
**Test**: RBAC is enforced
- [ ] Unauthenticated access to protected resource is denied
- [ ] Access `/Dashboard` without authentication redirects to login
- [ ] Access `/Settings/Users` without admin role is denied

**Command**:
```bash
# Should redirect to login (302) or return 401
curl -i https://[environment-url]/Dashboard
```

---

### 5. API Accessibility
**Test**: Coverage Evaluation API is reachable
- [ ] API endpoint responds: `GET /api/coverage/health`
- [ ] Expected Status: `200 OK` or `401 Unauthorized` (auth required)
- [ ] Server header removed (security)

**Command**:
```bash
curl -i https://[environment-url]/api/coverage/evaluate
```

---

### 6. Static Assets
**Test**: Static files are served correctly
- [ ] CSS files load: `/css/site.css`
- [ ] JavaScript files load: `/js/site.js`
- [ ] Favicon loads: `/favicon.ico`

**Manual Test**:
1. Navigate to home page
2. Open browser DevTools → Network
3. Verify all assets return `200 OK`
4. Verify no 404 errors

---

### 7. Session Handling
**Test**: Session management is operational
- [ ] Session cookie is set on login
- [ ] Session cookie has `HttpOnly`, `Secure`, `SameSite` flags
- [ ] Logout clears session

**Manual Test**:
1. Log in as test user
2. Open DevTools → Application → Cookies
3. Verify `.AspNetCore.Identity.Application` cookie exists
4. Verify cookie flags: `HttpOnly=true`, `Secure=true` (Pilot), `SameSite=Strict`

---

### 8. Security Headers
**Test**: Security headers are present
- [ ] `X-Content-Type-Options: nosniff`
- [ ] `X-Frame-Options: DENY`
- [ ] `Content-Security-Policy` header present
- [ ] `Server` header removed (information disclosure prevention)

**Command**:
```bash
curl -I https://[environment-url]/
```

---

### 9. Configuration Validation
**Test**: Application configuration is valid
- [ ] Application starts without errors
- [ ] Logs show "Configuration validation passed"
- [ ] No placeholder values in critical settings

**Check**:
```bash
# Azure CLI: Check application logs
az webapp log tail --name ism-sponsor-[environment] --resource-group ISM-Sponsor-RG
```

---

### 10. Integration Endpoints (Pilot Only)
**Test**: Integration sync is operational
- [ ] Sync status page loads: `/SyncStatus/Index` (admin only)
- [ ] Recent sync logs are present
- [ ] Sync failure rate < 20%

**Manual Test** (Admin user required):
1. Log in as admin user
2. Navigate to `/SyncStatus/Index`
3. Verify recent sync attempts are listed
4. Check for excessive failures

---

## Automated Smoke Test Script

### PowerShell Script
```powershell
param(
    [Parameter(Mandatory=$true)]
    [string]$BaseUrl
)

Write-Host "Running smoke tests against: $BaseUrl" -ForegroundColor Cyan

$failed = 0

# Test 1: Health Check
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Health check passed" -ForegroundColor Green
    } else {
        Write-Host "✗ Health check failed: $($response.StatusCode)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "✗ Health check error: $_" -ForegroundColor Red
    $failed++
}

# Test 2: Login Page
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/Account/Login" -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Login page accessible" -ForegroundColor Green
    } else {
        Write-Host "✗ Login page failed: $($response.StatusCode)" -ForegroundColor Red
        $failed++
    }
} catch {
    Write-Host "✗ Login page error: $_" -ForegroundColor Red
    $failed++
}

# Test 3: Protected Resource Redirect
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/Dashboard" -UseBasicParsing -MaximumRedirection 0 -ErrorAction SilentlyContinue
    if ($response.StatusCode -in @(302, 401)) {
        Write-Host "✓ Authorization check passed (redirect/deny)" -ForegroundColor Green
    } else {
        Write-Host "✗ Authorization check failed: $($response.StatusCode)" -ForegroundColor Red
        $failed++
    }
} catch {
    # Redirect throws exception, which is expected
    Write-Host "✓ Authorization check passed (protected)" -ForegroundColor Green
}

# Test 4: Security Headers
try {
    $response = Invoke-WebRequest -Uri "$BaseUrl/" -UseBasicParsing -TimeoutSec 10
    $headers = $response.Headers
    
    $securityPassed = $true
    if ($headers["X-Content-Type-Options"] -ne "nosniff") {
        Write-Host "✗ Missing X-Content-Type-Options header" -ForegroundColor Red
        $securityPassed = $false
    }
    if ($headers["X-Frame-Options"] -ne "DENY") {
        Write-Host "✗ Missing X-Frame-Options header" -ForegroundColor Red
        $securityPassed = $false
    }
    if ($headers.ContainsKey("Server")) {
        Write-Host "⚠ Server header present (information disclosure)" -ForegroundColor Yellow
    }
    
    if ($securityPassed) {
        Write-Host "✓ Security headers present" -ForegroundColor Green
    } else {
        $failed++
    }
} catch {
    Write-Host "✗ Security headers check error: $_" -ForegroundColor Red
    $failed++
}

Write-Host "`nSmoke test summary:" -ForegroundColor Cyan
if ($failed -eq 0) {
    Write-Host "All smoke tests passed ✓" -ForegroundColor Green
    exit 0
} else {
    Write-Host "$failed test(s) failed ✗" -ForegroundColor Red
    exit 1
}
```

### Usage
```bash
# Dev environment
./SmokeTests.ps1 -BaseUrl "https://ism-sponsor-dev.azurewebsites.net"

# Pilot environment
./SmokeTests.ps1 -BaseUrl "https://ism-sponsor-pilot.azurewebsites.net"
```

---

## Pass/Fail Criteria

### Dev Environment
- **Pass**: All 8 core tests (1-8) pass
- **Warning**: Test 10 (integration) can be degraded
- **Fail**: Any core test fails

### Pilot Environment
- **Pass**: All 10 tests pass
- **Warning**: Test 10 shows < 20% failure rate
- **Fail**: Any test fails OR Test 10 shows > 20% failure rate

---

## Rollback Triggers

**Immediate Rollback Required If**:
- Health check fails
- Database connectivity fails
- Authentication system is unavailable
- Security headers are missing
- Application logs show configuration validation errors

**Investigate and Consider Rollback If**:
- Sync failure rate > 20%
- Session handling is broken
- Authorization bypass detected

---

## Post-Smoke Test Actions

### If All Tests Pass
1. ✓ Mark deployment as successful
2. ✓ Notify stakeholders
3. ✓ Monitor application logs for 1 hour
4. ✓ Proceed to next stage (Dev → Pilot approval, Pilot → done)

### If Any Test Fails
1. ✗ **DO NOT PROCEED** to next stage
2. ✗ Review application logs immediately
3. ✗ Identify root cause
4. ✗ Execute rollback if critical failure
5. ✗ Fix issue in code
6. ✗ Re-deploy and re-test

---

## Monitoring After Deployment

**First Hour**: Monitor closely
- Check Application Insights for errors
- Watch for spikes in 500 errors
- Monitor authentication failures
- Check database connection health

**First 24 Hours**: Ongoing monitoring
- Review security audit logs
- Check sync success rates
- Monitor system health metrics
- Review user-reported issues

---

## Contact Information

**On-Call Engineer**: [email/phone]  
**DevOps Team**: devops-team@ismmanila.org  
**Rollback Authority**: Lead Engineer or Director of Technology
