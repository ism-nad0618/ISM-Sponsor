# Azure Deployment Guide - ISM Sponsor
**Date:** April 23, 2026  
**Target:** https://ismsponsor.azurewebsites.net  
**Purpose:** Deploy security fixes to Azure App Service

---

## 🚀 Quick Deployment Options

### Option 1: Visual Studio (Recommended - Easiest)

**Prerequisites:** Visual Studio 2022 with Azure workload installed

**Steps:**

1. **Open Project in Visual Studio**
   - Open `ISMSponsor.sln`

2. **Right-click on Project**
   - In Solution Explorer, right-click `ISMSponsor` project
   - Select **Publish...**

3. **Select Target**
   - If you have existing publish profile: Select it
   - If no profile exists: 
     - Choose **Azure**
     - Select **Azure App Service (Windows)**
     - Select your subscription
     - Choose `ismsponsor` app service

4. **Configure Settings**
   - Configuration: **Release**
   - Target Runtime: **Portable**
   - Deployment Mode: **Framework-Dependent**

5. **Publish**
   - Click **Publish** button
   - Wait for deployment (2-5 minutes)
   - Visual Studio will show progress

6. **Verify Deployment**
   - Browser will automatically open to site
   - Check that login page loads

**Expected Time:** 5-10 minutes

---

### Option 2: Azure Portal + Local Publish

**Best for:** Quick manual deployment without complex setup

**Steps:**

1. **Build and Publish Locally**
   ```bash
   cd "/Users/cruzr/Documents/ISM Sponsor"
   dotnet publish ISMSponsor.csproj -c Release -o ./publish
   ```

2. **Create ZIP Package**
   ```bash
   cd ./publish
   zip -r ../deploy.zip .
   cd ..
   ```

3. **Deploy via Azure Portal**
   - Go to https://portal.azure.com
   - Navigate to: **App Services** → **ismsponsor**
   - In left menu: **Deployment Center**
   - Choose **Manual Deployment (Local Git/FTP/ZIP)**
   - Select **ZIP Deploy**
   - Upload `deploy.zip` file
   - Wait for deployment to complete

4. **Restart App Service**
   - In Azure Portal: App Services → ismsponsor
   - Click **Restart** at the top
   - Wait 30-60 seconds

5. **Verify Deployment**
   - Browse to https://ismsponsor.azurewebsites.net
   - Open browser DevTools (F12) → Network tab
   - Check Response Headers:
     - Should see `Content-Security-Policy` without unsafe-inline/unsafe-eval
     - Should see `Strict-Transport-Security`
     - Should NOT see `X-Powered-By` or `Server: Microsoft-IIS/10.0`

**Expected Time:** 10-15 minutes

---

### Option 3: GitHub Actions (Best for CI/CD)

**Best for:** Automated deployment on every git push

**Setup Steps:**

1. **Create GitHub Action Workflow**

Create file: `.github/workflows/azure-deploy.yml`

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: ismsponsor
  DOTNET_VERSION: '8.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Restore dependencies
        run: dotnet restore ISMSponsor.csproj
      
      - name: Build
        run: dotnet build ISMSponsor.csproj --configuration Release --no-restore
      
      - name: Publish
        run: dotnet publish ISMSponsor.csproj -c Release -o ./publish
      
      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: ${{ env.AZURE_WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

2. **Get Azure Publish Profile**
   - Go to Azure Portal
   - Navigate to App Services → ismsponsor
   - Click **Get publish profile** (top toolbar)
   - Download the `.PublishSettings` file
   - Copy entire XML content

3. **Add GitHub Secret**
   - Go to your GitHub repository
   - Settings → Secrets and variables → Actions
   - Click **New repository secret**
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the publish profile XML
   - Click **Add secret**

4. **Deploy**
   - Commit the workflow file:
     ```bash
     git add .github/workflows/azure-deploy.yml
     git commit -m "Add GitHub Actions deployment workflow"
     git push
     ```
   - GitHub Actions will automatically deploy
   - Check progress: GitHub repo → Actions tab

**Expected Time:** 15-20 minutes setup, then automatic

---

### Option 4: Azure CLI (Requires Installation)

**Install Azure CLI:**
```bash
brew install azure-cli
```

**Login to Azure:**
```bash
az login
```

**Deploy:**
```bash
cd "/Users/cruzr/Documents/ISM Sponsor"

# Build and publish
dotnet publish ISMSponsor.csproj -c Release -o ./publish

# Create ZIP
cd ./publish && zip -r ../deploy.zip . && cd ..

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group ISM-Sponsor-RG \
  --name ismsponsor \
  --src deploy.zip

# Restart app
az webapp restart \
  --resource-group ISM-Sponsor-RG \
  --name ismsponsor
```

**Expected Time:** 10 minutes after CLI installation

---

## 🔍 Post-Deployment Verification

### 1. **Check Application Loads**
```bash
# Open in browser
open https://ismsponsor.azurewebsites.net
```

Expected: Login page loads normally

### 2. **Verify Security Headers**

**Using Browser DevTools:**
1. Open https://ismsponsor.azurewebsites.net
2. Press F12 (Developer Tools)
3. Go to Network tab
4. Refresh page (Cmd+R)
5. Click on first request (document)
6. Click Response Headers

**Expected Headers:**
- ✅ `Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self'; ...` (NO unsafe-inline/unsafe-eval)
- ✅ `Strict-Transport-Security: max-age=31536000; includeSubDomains; preload`
- ✅ `X-Content-Type-Options: nosniff`
- ✅ `X-Frame-Options: DENY`
- ❌ `X-Powered-By` (should NOT be present)
- ❌ `Server` (should NOT show version, or not present)

**Using Command Line:**
```bash
curl -I https://ismsponsor.azurewebsites.net
```

### 3. **Run ZAP Test 3**

After deployment verification:

1. Open OWASP ZAP
2. Automated Scan → https://ismsponsor.azurewebsites.net
3. Wait for scan to complete
4. Generate HTML report
5. Save as: `docs/ZAP Reports/ZAP by Checkmarx Scanning Report Test3.html`

**Expected Results:**
- High: 0
- Medium: 0 (down from 4) ✨
- Low: 2-4 (only Azure infrastructure cookies)
- Info: 5

### 4. **Test Key Functionality**

Test these critical paths:
- ✅ Home page loads
- ✅ Login page accessible
- ✅ Google OAuth login works
- ✅ CSS/JS files load (no CSP violations)
- ✅ Dashboard accessible after login
- ✅ Forms submit correctly
- ✅ No console errors (F12 → Console tab)

---

## 📊 What Gets Deployed

**Files Changed in Latest Commits:**
- `Middleware/SecurityHeadersMiddleware.cs` (CSP fixes)
- `Program.cs` (HSTS, cookie security, Kestrel config)
- `web.config` (IIS header removal)
- `docs/` (documentation - not deployed)

**Expected Fixes After Deployment:**

| Issue | Status | Impact |
|-------|--------|--------|
| CSP: script-src unsafe-inline | ✅ Will be fixed | Medium → 0 |
| CSP: script-src unsafe-eval | ✅ Will be fixed | Medium → 0 |
| CSP: style-src unsafe-inline | ✅ Will be fixed | Medium → 0 |
| CSP: img-src wildcard | ✅ Will be fixed | Medium → 0 |
| X-Powered-By header | ✅ Will be fixed | Low → 0 |
| Server header | ✅ Will be fixed | Low → 0 |
| ARRAffinity cookies | ❌ Azure infra | Low (accepted) |

---

## 🛠️ Troubleshooting

### Issue: Application won't start after deployment

**Check:**
1. Azure Portal → App Service → Log stream
2. Look for startup errors
3. Check appsettings.json is correct for production

**Fix:**
```bash
az webapp log tail --name ismsponsor --resource-group ISM-Sponsor-RG
```

### Issue: CSP violations in browser console

**Symptoms:** Console shows "Refused to load..." errors

**Check:**
1. Browser DevTools → Console
2. Look for CSP violation messages
3. Check actual CSP header in Network tab

**Likely Cause:** External resource not in CSP

**Fix:** Update SecurityHeadersMiddleware.cs to allow necessary domains

### Issue: 500 Internal Server Error

**Check:**
1. Azure Portal → App Service → Diagnose and solve problems
2. Application Insights (if enabled)
3. Log stream for exceptions

**Common Causes:**
- Database connection string incorrect
- Missing environment variables
- Configuration error in appsettings

### Issue: Headers not updated (still see X-Powered-By)

**Cause:** web.config not deployed or Azure cache

**Fix:**
1. Verify web.config exists in deployed files:
   - Azure Portal → App Service → Advanced Tools (Kudu)
   - Debug console → CMD
   - Navigate to `site/wwwroot`
   - Check if web.config exists
2. If missing, redeploy
3. Restart app service
4. Clear browser cache (Cmd+Shift+R)

---

## 📝 Deployment Checklist

Before deploying:
- [ ] Code committed to git
- [ ] Build succeeds locally (`dotnet build`)
- [ ] Tests pass (`dotnet test`)
- [ ] No compilation errors/warnings

During deployment:
- [ ] Choose deployment method
- [ ] Follow steps carefully
- [ ] Wait for deployment to complete
- [ ] Don't interrupt process

After deployment:
- [ ] Site loads (https://ismsponsor.azurewebsites.net)
- [ ] Login page accessible
- [ ] Security headers verified
- [ ] No console errors
- [ ] Key functionality tested
- [ ] ZAP Test 3 run
- [ ] Results documented

---

## 🎯 Next Steps

1. **Choose a deployment method above**
   - Recommended: Option 1 (Visual Studio) or Option 2 (Azure Portal)

2. **Deploy the application**
   - Follow chosen method's steps carefully

3. **Verify deployment**
   - Check headers using browser DevTools
   - Test key functionality

4. **Run ZAP Test 3**
   - Document results
   - Compare with Test 2

5. **Create final comparison report**
   - Test 1 vs Test 2 vs Test 3
   - Show security improvement

---

## 📞 Need Help?

If you encounter issues:
1. Check Azure Portal log stream
2. Review troubleshooting section above
3. Check GitHub repo issues
4. Review Azure App Service documentation

**Estimated Total Time:** 15-30 minutes depending on method chosen

---

**Status:** Ready to deploy! All fixes are committed to GitHub. 🚀
