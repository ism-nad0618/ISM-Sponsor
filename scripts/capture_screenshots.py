#!/usr/bin/env python3
"""
Automated Screenshot Capture Script
Captures screenshots for ISM Sponsor user manuals based on SCREENSHOT_GUIDE.md
"""

import os
import sys
import time
from pathlib import Path

try:
    from playwright.sync_api import sync_playwright, TimeoutError as PlaywrightTimeout
except ImportError:
    print("Playwright not installed. Installing...")
    os.system(f"{sys.executable} -m pip install --user playwright")
    os.system(f"{sys.executable} -m playwright install chromium")
    print("\n✅ Playwright installed successfully!")
    print("⚠️  Please run this script again to capture screenshots.\n")
    sys.exit(0)

# Configuration
BASE_URL = "http://localhost:5000"
OUTPUT_DIR = Path(__file__).parent.parent / "docs" / "screenshots"
VIEWPORT_SIZE = {"width": 1920, "height": 1080}

# Screenshot specifications from SCREENSHOT_GUIDE.md
SCREENSHOTS = {
    # ADMIN SCREENSHOTS
    "admin": [
        {"file": "admin_01_login_page.png", "url": "/Account/Login", "wait": 1},
        {"file": "admin_02_dashboard_overview.png", "url": "/Dashboard/AdminDashboard", "wait": 2, "login": "admin"},
        {"file": "admin_03_dashboard_stats.png", "url": "/Dashboard/AdminDashboard", "wait": 2, "login": "admin"},
        {"file": "admin_06_sponsors_list.png", "url": "/Settings/Sponsors", "wait": 2, "login": "admin"},
        {"file": "admin_09_create_sponsor_empty.png", "url": "/Settings/Sponsors/Create", "wait": 1, "login": "admin"},
        {"file": "admin_11_sponsor_details.png", "url": "/Settings/Sponsors/Details/1", "wait": 2, "login": "admin"},
        {"file": "admin_14_portal_logs_list.png", "url": "/Portal/Index", "wait": 2, "login": "admin"},
        {"file": "admin_16_create_log_step1.png", "url": "/Portal/Create", "wait": 2, "login": "admin"},
        {"file": "admin_19_create_log_coverage_rules.png", "url": "/Portal/Create", "wait": 2, "login": "admin", "scroll": 800},
        {"file": "admin_24_requests_list.png", "url": "/ReviewRequest/Index", "wait": 2, "login": "admin"},
        {"file": "admin_30_users_list.png", "url": "/Settings/Users", "wait": 2, "login": "admin"},
        {"file": "admin_36_reports_menu.png", "url": "/AdminReports/Index", "wait": 2, "login": "admin"},
        {"file": "admin_41_operations_dashboard.png", "url": "/Dashboard/OperationsDashboard", "wait": 2, "login": "admin"},
    ],
    
    # ADMISSIONS SCREENSHOTS
    "admissions": [
        {"file": "admissions_01_login_page.png", "url": "/Account/Login", "wait": 1},
        {"file": "admissions_02_dashboard_overview.png", "url": "/Dashboard/Index", "wait": 2, "login": "admissions"},
        {"file": "admissions_04_sponsors_list.png", "url": "/Settings/Sponsors", "wait": 2, "login": "admissions"},
        {"file": "admissions_06_create_sponsor_empty.png", "url": "/Settings/Sponsors/Create", "wait": 1, "login": "admissions"},
        {"file": "admissions_08_sponsor_details.png", "url": "/Settings/Sponsors/Details/1", "wait": 2, "login": "admissions"},
        {"file": "admissions_11_portal_logs_list.png", "url": "/Portal/Index", "wait": 2, "login": "admissions"},
        {"file": "admissions_13_create_log_step1.png", "url": "/Portal/Create", "wait": 2, "login": "admissions"},
        {"file": "admissions_15_create_log_coverage_rules.png", "url": "/Portal/Create", "wait": 2, "login": "admissions", "scroll": 800},
        {"file": "admissions_19_submit_change_request.png", "url": "/Settings/Sponsors/Edit/1", "wait": 2, "login": "admissions"},
        {"file": "admissions_23_reports_menu.png", "url": "/AdmissionsReports/Index", "wait": 2, "login": "admissions"},
    ],
    
    # CASHIER SCREENSHOTS
    "cashier": [
        {"file": "cashier_01_login_page.png", "url": "/Account/Login", "wait": 1},
        {"file": "cashier_02_dashboard_overview.png", "url": "/Dashboard/Index", "wait": 2, "login": "cashier"},
        {"file": "cashier_03_sponsor_details.png", "url": "/Settings/Sponsors/Details/1", "wait": 2, "login": "cashier"},
        {"file": "cashier_04_portal_logs_list.png", "url": "/Portal/Index", "wait": 2, "login": "cashier"},
        {"file": "cashier_05_log_details_coverage.png", "url": "/Portal/Details/1", "wait": 2, "login": "cashier"},
        {"file": "cashier_11_reports_menu.png", "url": "/CashierReports/Index", "wait": 2, "login": "cashier"},
    ],
}

# User credentials (use demo accounts with strong passwords)
CREDENTIALS = {
    "admin": {"username": "admin", "password": "Admin@123"},
    "admissions": {"username": "admissions", "password": "Admissions@123"},
    "cashier": {"username": "cashier", "password": "Cashier@123"},
}


def login(page, role):
    """Login to the application with specified role"""
    if role not in CREDENTIALS:
        print(f"⚠️  Unknown role: {role}")
        return False
    
    creds = CREDENTIALS[role]
    
    # Navigate to login page
    page.goto(f"{BASE_URL}/Account/Login", wait_until="networkidle")
    
    # Fill login form
    try:
        page.fill('input[name="Username"]', creds["username"])
        page.fill('input[name="Password"]', creds["password"])
        page.click('button[type="submit"]')
        
        # Wait for redirect after login
        page.wait_for_load_state("networkidle", timeout=5000)
        time.sleep(1)
        
        # Check if login successful (URL should change)
        current_url = page.url
        if "/Account/Login" in current_url and "ReturnUrl" not in current_url:
            print(f"⚠️  Login might have failed for {role}")
            return False
        
        print(f"✅ Logged in as {role}")
        return True
    except Exception as e:
        print(f"❌ Login failed for {role}: {e}")
        return False


def capture_screenshot(page, spec, role, context):
    """Capture a single screenshot based on specification"""
    filename = spec["file"]
    url = spec["url"]
    wait_time = spec.get("wait", 1)
    needs_login = spec.get("login")
    scroll_to = spec.get("scroll", 0)
    
    # Determine output path
    if "dashboard" in filename:
        subdir = "dashboard"
    elif "sponsor" in filename:
        subdir = "sponsors"
    elif "log" in filename or "portal" in filename:
        subdir = "logs"
    elif "request" in filename:
        subdir = "requests"
    elif "user" in filename:
        subdir = "users"
    elif "report" in filename:
        subdir = "reports"
    elif "operation" in filename:
        subdir = "operations"
    else:
        subdir = ""
    
    output_path = OUTPUT_DIR / role / subdir / filename
    output_path.parent.mkdir(parents=True, exist_ok=True)
    
    try:
        # Handle login if needed
        if needs_login and needs_login != role:
            print(f"  ℹ️  Switching to {needs_login} account...")
            # Create new page with new context for different user
            new_page = context.new_page()
            new_page.set_viewport_size(VIEWPORT_SIZE)
            login(new_page, needs_login)
            page = new_page
        elif needs_login:
            # Ensure we're logged in as the current role
            pass
        
        # Navigate to URL
        full_url = f"{BASE_URL}{url}"
        print(f"  📸 Capturing: {filename}")
        print(f"     URL: {url}")
        
        page.goto(full_url, wait_until="networkidle", timeout=10000)
        time.sleep(wait_time)
        
        # Scroll if specified
        if scroll_to > 0:
            page.evaluate(f"window.scrollTo(0, {scroll_to})")
            time.sleep(0.5)
        
        # Capture screenshot
        page.screenshot(path=str(output_path), full_page=False)
        print(f"  ✅ Saved to: {output_path.relative_to(OUTPUT_DIR.parent)}")
        
        return True
        
    except PlaywrightTimeout:
        print(f"  ⚠️  Timeout loading {url}")
        return False
    except Exception as e:
        print(f"  ❌ Error capturing {filename}: {e}")
        return False


def main():
    """Main screenshot capture routine"""
    print("=" * 70)
    print("ISM Sponsor - Automated Screenshot Capture")
    print("=" * 70)
    print()
    
    # Check if application is running
    import urllib.request
    try:
        urllib.request.urlopen(BASE_URL, timeout=2)
        print(f"✅ Application detected at {BASE_URL}")
    except Exception:
        print(f"❌ Application not running at {BASE_URL}")
        print("   Please start the application first:")
        print("   dotnet run --project ISMSponsor.csproj --urls http://localhost:5000")
        return 1
    
    print(f"📁 Screenshots will be saved to: {OUTPUT_DIR}")
    print()
    
    stats = {"success": 0, "failed": 0, "skipped": 0}
    
    with sync_playwright() as p:
        # Launch browser
        print("🚀 Launching browser...")
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(viewport=VIEWPORT_SIZE)
        
        # Process each role
        for role, screenshots in SCREENSHOTS.items():
            print()
            print(f"{'=' * 70}")
            print(f"📋 Processing {role.upper()} Screenshots ({len(screenshots)} total)")
            print(f"{'=' * 70}")
            
            # Create new page for this role
            page = context.new_page()
            page.set_viewport_size(VIEWPORT_SIZE)
            
            # Login if needed
            if any(s.get("login") == role for s in screenshots):
                if not login(page, role):
                    print(f"⚠️  Skipping {role} screenshots due to login failure")
                    stats["skipped"] += len(screenshots)
                    continue
            
            # Capture screenshots
            for i, spec in enumerate(screenshots, 1):
                print(f"\n[{i}/{len(screenshots)}]")
                if capture_screenshot(page, spec, role, context):
                    stats["success"] += 1
                else:
                    stats["failed"] += 1
                
                time.sleep(0.5)  # Brief pause between captures
            
            page.close()
        
        browser.close()
    
    # Summary
    print()
    print("=" * 70)
    print("📊 CAPTURE SUMMARY")
    print("=" * 70)
    print(f"✅ Successful: {stats['success']}")
    print(f"❌ Failed:     {stats['failed']}")
    print(f"⏭️  Skipped:    {stats['skipped']}")
    print(f"📁 Output:     {OUTPUT_DIR}")
    print()
    
    if stats["success"] > 0:
        print("✅ Screenshots captured successfully!")
        print()
        print("Next steps:")
        print("1. Review screenshots in docs/screenshots/")
        print("2. Optionally annotate with arrows/highlights")
        print("3. Commit to repository: git add docs/screenshots/")
        return 0
    else:
        print("⚠️  No screenshots were captured successfully")
        return 1


if __name__ == "__main__":
    sys.exit(main())
