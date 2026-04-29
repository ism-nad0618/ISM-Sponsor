# Google OAuth Setup - Quick Start Guide

Quick guide for enabling Google Sign-In on the ISM Sponsor Management System login page.

---

## 🚀 **5-Minute Setup**

### Prerequisites
- ✅ Google account with access to [Google Cloud Console](https://console.cloud.google.com)
- ✅ ISM Sponsor application running locally or deployed
- ✅ HTTPS enabled (required for OAuth)

---

## 📋 **Step-by-Step Instructions**

### **1. Google Cloud Console (5 minutes)**

#### Create OAuth Credentials

1. **Go to Google Cloud Console**
   - Navigate to [console.cloud.google.com](https://console.cloud.google.com)

2. **Create or Select Project**
   - Click project dropdown → "New Project"
   - Name: "ISM Sponsor Management"
   - Click "Create"

3. **Enable Google+ API**
   - Left menu → "APIs & Services" → "Library"
   - Search: "Google+ API"
   - Click → Enable

4. **Create OAuth Client ID**
   - Left menu → "APIs & Services" → "Credentials"
   - Click "+ CREATE CREDENTIALS"
   - Select "OAuth 2.0 Client ID"

5. **Configure OAuth Consent Screen** (if first time)
   - User Type: "Internal" (for ISM only) or "External"
   - App name: "ISM Sponsor Management"
   - User support email: your@ismanila.org
   - Developer contact: your@ismanila.org
   - Scopes: email, profile (pre-selected)
   - Click "Save and Continue"

6. **Create Web Application Credentials**
   - Application type: "Web application"
   - Name: "ISM Sponsor Management"
   - Authorized redirect URIs:
     - Development: `https://localhost:7xxx/Account/GoogleCallback`
     - Production: `https://sponsor.ismanila.org/Account/GoogleCallback`
   - Click "Create"

7. **Copy Credentials**
   - You'll see Client ID and Client Secret
   - **COPY BOTH** - you'll need them in next step

---

### **2. Application Configuration (2 minutes)**

#### Add Credentials to appsettings.json

**File**: `appsettings.json` or `appsettings.Development.json`

```json
{
  "ConnectionString": "...",
  "Authentication": {
    "Google": {
      "ClientId": "123456789-abcdefg.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-abcdefghijklmnop"
    }
  }
}
```

**Important**: 
- ✅ DO add to `appsettings.Development.json` for local testing
- ❌ DON'T commit credentials to source control
- ✅ DO use environment variables in production

---

### **3. Enable Google Auth in Code (1 minute)**

#### Uncomment Code in Program.cs

**File**: `Program.cs` (around line 40)

Find this section:
```csharp
// ============================================================================
// GOOGLE OAUTH AUTHENTICATION (Optional)
// ============================================================================

/*   ← REMOVE THIS LINE
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var googleConfig = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleConfig["ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
        options.ClientSecret = googleConfig["ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
        options.CallbackPath = "/Account/GoogleCallback";
        
        // Request email and profile scopes
        options.Scope.Add("email");
        options.Scope.Add("profile");
        
        // Save tokens for potential future use
        options.SaveTokens = true;
    });
*/   ← REMOVE THIS LINE
```

**Action**: Delete the `/*` and `*/` lines to uncomment the code.

---

### **4. Test the Integration (2 minutes)**

#### Build and Run

```bash
dotnet build
dotnet run
```

#### Test Login Flow

1. Navigate to login page: `https://localhost:7xxx/Account/Login`
2. You should see two login sections:
   - ISM Google Login (left)
   - Sponsor Organization Login (right)
3. Click **"Sign in with Google"**
4. Google consent screen should appear
5. Sign in with **@ismanila.org account**
6. Grant permissions
7. You should be redirected back and logged in
8. Check Dashboard - you're authenticated! ✅

---

## 🔐 **Security Configuration**

### Production Deployment

#### Use Environment Variables (Recommended)

**Azure App Service**:
```bash
az webapp config appsettings set \
  --name ism-sponsor-portal \
  --resource-group ISM-Resources \
  --settings \
    Authentication__Google__ClientId="YOUR_CLIENT_ID" \
    Authentication__Google__ClientSecret="YOUR_CLIENT_SECRET"
```

**Docker**:
```yaml
environment:
  - Authentication__Google__ClientId=YOUR_CLIENT_ID
  - Authentication__Google__ClientSecret=YOUR_CLIENT_SECRET
```

**IIS / Windows Server**:
- Add to Environment Variables in server settings
- Format: `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret`

---

### Restrict Access to ISM Domain

**Already Implemented** in `AccountController.GoogleCallback`:

```csharp
// Only allow ISM email addresses (@ismanila.org)
if (!email.EndsWith("@ismanila.org", StringComparison.OrdinalIgnoreCase))
{
    _logger.LogWarning("Google login rejected: Non-ISM email {Email}", email);
    TempData["Error"] = "Only ISM Google accounts (@ismanila.org) are allowed.";
    return RedirectToAction(nameof(Login));
}
```

To allow multiple domains, modify:
```csharp
var allowedDomains = new[] { "@ismanila.org", "@ismanila.edu.ph" };
if (!allowedDomains.Any(domain => email.EndsWith(domain, StringComparison.OrdinalIgnoreCase)))
{
    // Reject
}
```

---

## 🎯 **Customize User Provisioning**

### Change Default Role

By default, Google-authenticated users are assigned the **"admin"** role.

**To change default role**:

Edit `AccountController.cs` → `GoogleCallback` method:

```csharp
// Find this line (around line 115):
await _userManager.AddToRoleAsync(user, "admin");

// Change to:
await _userManager.AddToRoleAsync(user, "admissions"); // or "cashier"
```

### Domain-Based Role Assignment

Assign roles based on email address:

```csharp
// Instead of hardcoded role:
string role = "admin"; // Default

if (email.Contains("cashier"))
    role = "cashier";
else if (email.Contains("admissions"))
    role = "admissions";

await _userManager.AddToRoleAsync(user, role);
```

### Manual Approval Workflow

Disable auto-provisioning:

```csharp
// In GoogleCallback, after creating user:
user.IsActive = false; // Require admin approval

var createResult = await _userManager.CreateAsync(user);

// Send notification to admin
_logger.LogInformation("New user {Email} created. Awaiting approval.", email);

TempData["Message"] = "Account created. Please wait for administrator approval.";
return RedirectToAction(nameof(Login));
```

---

## 🐛 **Common Issues**

### Issue: "Invalid redirect_uri"
**Cause**: Redirect URI mismatch  
**Solution**: 
1. Check URL in Google Console matches exactly
2. Include protocol (https://)
3. Include port if using localhost (e.g., :7001)
4. No trailing slash

### Issue: "Unable to load external login information"
**Cause**: Callback failed  
**Solution**:
1. Ensure Google auth is uncommented in `Program.cs`
2. Check appsettings.json has correct ClientId/Secret
3. Verify redirect URI in Google Console

### Issue: "Only ISM Google accounts are allowed"
**Cause**: Using non-@ismanila.org email  
**Solution**: 
- Use ISM Google account
- Or modify domain check in `AccountController.GoogleCallback`

### Issue: Google button does nothing
**Cause**: OAuth not enabled  
**Solution**: Uncomment code in `Program.cs` and rebuild

### Issue: "This site can't provide a secure connection"
**Cause**: HTTPS not enabled  
**Solution**: 
- Development: Run with `dotnet run` (HTTPS auto-enabled)
- Production: Install SSL certificate

---

## 📊 **Verification Checklist**

### Setup Complete When:
- [ ] Google Cloud project created
- [ ] OAuth client ID created
- [ ] Redirect URI configured correctly
- [ ] ClientId and ClientSecret copied
- [ ] Credentials added to appsettings.json
- [ ] Code uncommented in Program.cs
- [ ] Application builds successfully
- [ ] Login page shows "Sign in with Google" button
- [ ] Clicking button redirects to Google
- [ ] Can sign in with @ismanila.org account
- [ ] Redirected back to application
- [ ] Logged in successfully
- [ ] Dashboard accessible

---

## 🔗 **Useful Links**

- **Google Cloud Console**: https://console.cloud.google.com
- **OAuth 2.0 Guide**: https://developers.google.com/identity/protocols/oauth2
- **ASP.NET Core External Auth**: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/
- **Google OAuth Scopes**: https://developers.google.com/identity/protocols/oauth2/scopes

---

## 📞 **Support**

### Logs Location
Check application logs for Google OAuth errors:
```bash
# Development
dotnet run --verbosity detailed

# Production (Azure)
App Service → Logs → Log stream
```

### Debug Mode
Enable detailed Google auth logging in `Program.cs`:
```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // ... existing config
        options.Events.OnRemoteFailure = context =>
        {
            context.Response.Redirect("/Account/Login?error=" + context.Failure.Message);
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
```

---

**Status**: Ready to Enable  
**Estimated Setup Time**: 5-10 minutes  
**Difficulty**: Easy ⭐⭐☆☆☆
