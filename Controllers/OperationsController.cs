using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISMSponsor.Data;
using Microsoft.EntityFrameworkCore;
using ISMSponsor.Integration.Adapters;

namespace ISMSponsor.Controllers;

[Authorize(Roles = "admin")]
public class OperationsController : Controller
{
    private readonly MonitoringService _monitoringService;
    private readonly SmokeTestService _smokeTestService;
    private readonly AppDbContext _context;
    private readonly IIntegrationSyncService _integrationSyncService;

    public OperationsController(
        MonitoringService monitoringService, 
        SmokeTestService smokeTestService,
        AppDbContext context,
        IIntegrationSyncService integrationSyncService)
    {
        _monitoringService = monitoringService;
        _smokeTestService = smokeTestService;
        _context = context;
        _integrationSyncService = integrationSyncService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var model = await _monitoringService.GetOperationalStatusAsync();
        return View(model);
    }

    [HttpGet]
    public IActionResult SmokeTest()
    {
        var model = _smokeTestService.GetSmokeTestChecklist();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSmokeTestResult([FromForm] int testId, [FromForm] string status, [FromForm] string? notes, [FromForm] string? evidence)
    {
        var userId = User.Identity?.Name ?? "Unknown";
        await _smokeTestService.UpdateSmokeTestResultAsync(testId, status, notes, evidence, userId);
        return RedirectToAction(nameof(SmokeTest));
    }

    [HttpGet]
    public IActionResult Runbooks()
    {
        var model = GetRunbooksContent();
        return View(model);
    }

    [HttpGet]
    public IActionResult ReleaseChecklist()
    {
        var model = GetReleaseChecklistContent();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SyncRetry(string? status = null, string? targetSystem = null, string? entityType = null)
    {
        var query = _context.SyncLogs.AsQueryable();

        // Filter by status
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status == status);
        }
        else
        {
            // Default: show only failed syncs
            query = query.Where(s => s.Status == "Failed");
        }

        // Filter by target system
        if (!string.IsNullOrEmpty(targetSystem))
        {
            query = query.Where(s => s.TargetSystem == targetSystem);
        }

        // Filter by entity type
        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(s => s.EntityType == entityType);
        }

        var syncLogs = await query
            .OrderByDescending(s => s.AttemptedAt)
            .Take(100)
            .ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.TargetSystemFilter = targetSystem;
        ViewBag.EntityTypeFilter = entityType;

        // Get distinct values for filters
        ViewBag.AvailableStatuses = await _context.SyncLogs
            .Select(s => s.Status)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        ViewBag.AvailableTargetSystems = await _context.SyncLogs
            .Select(s => s.TargetSystem)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        ViewBag.AvailableEntityTypes = await _context.SyncLogs
            .Select(s => s.EntityType)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        return View(syncLogs);
    }

    [HttpPost]
    public async Task<IActionResult> RetrySync(int syncLogId)
    {
        try
        {
            var result = await _integrationSyncService.RetrySyncAsync(syncLogId);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Retry succeeded: {result.Message}";
            }
            else
            {
                TempData["ErrorMessage"] = $"Retry failed: {result.ErrorMessage ?? result.Message}";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error during retry: {ex.Message}";
        }

        return RedirectToAction(nameof(SyncRetry));
    }

    private RunbookViewModel GetRunbooksContent()
    {
        var model = new RunbookViewModel
        {
            Runbooks = new List<RunbookItem>
            {
                new RunbookItem
                {
                    Title = "Sync Failure Response",
                    Category = "Sync",
                    Description = "What to do when integration sync fails",
                    WhatToCheck = new List<string>
                    {
                        "Check Sync Status dashboard for recent failures",
                        "Review error messages in sync log",
                        "Verify target system availability",
                        "Check entity data for validation issues"
                    },
                    WhereToLook = new List<string>
                    {
                        "Operations → Dashboard → Recent Sync Failures",
                        "Sync Status page",
                        "Activity Logs filtered by Module: Sync"
                    },
                    CommonCauses = new List<string>
                    {
                        "Target system temporarily unavailable",
                        "Authentication token expired",
                        "Data validation failure in target system",
                        "Network connectivity issues",
                        "Missing or invalid required fields"
                    },
                    ActionSteps = new List<string>
                    {
                        "1. Check sync log for specific error message",
                        "2. Verify target system is accessible",
                        "3. If authentication issue, refresh token/credentials",
                        "4. If data validation issue, correct source data and retry",
                        "5. Manual retry available for failed syncs (if retry count < 3)",
                        "6. If persistent, check target system documentation"
                    },
                    WhenToEscalate = "If sync failures persist after 3 retries or if target system is confirmed down"
                },
                new RunbookItem
                {
                    Title = "Data Mismatch Investigation",
                    Category = "DataMismatch",
                    Description = "How to investigate data consistency issues",
                    WhatToCheck = new List<string>
                    {
                        "Check Data Consistency Warnings on Operations Dashboard",
                        "Review recent merge operations",
                        "Check for duplicate records",
                        "Verify LoG activation status"
                    },
                    WhereToLook = new List<string>
                    {
                        "Operations → Dashboard → Data Consistency Warnings",
                        "Duplicates → Merge History",
                        "Admin Reports → Sponsor Master Report",
                        "Activity Logs filtered by Module: Sponsors or LogCoverage"
                    },
                    CommonCauses = new List<string>
                    {
                        "Sponsor has students but no active LoG",
                        "Duplicate sponsors not yet merged",
                        "Student tagged to inactive sponsor",
                        "LoG created but not activated",
                        "Stale data from failed sync"
                    },
                    ActionSteps = new List<string>
                    {
                        "1. Identify the warning type and affected entities",
                        "2. Navigate to the entity detail page",
                        "3. Review audit history for recent changes",
                        "4. If duplicate: use Duplicates → Merge Preview",
                        "5. If missing LoG: create and activate LoG",
                        "6. If sync issue: check Sync Status and retry",
                        "7. Document resolution in Activity Log"
                    },
                    WhenToEscalate = "If data cannot be reconciled or if issue affects multiple entities"
                },
                new RunbookItem
                {
                    Title = "Access Issue Resolution",
                    Category = "Access",
                    Description = "How to resolve user access and authorization issues",
                    WhatToCheck = new List<string>
                    {
                        "Check Authorization Failures on Operations Dashboard",
                        "Verify user's assigned roles",
                        "Check if user account is active",
                        "Review recent role changes"
                    },
                    WhereToLook = new List<string>
                    {
                        "Operations → Dashboard → Authorization Failures",
                        "Settings → Users → User Detail",
                        "Settings → Roles",
                        "Activity Logs filtered by Action: AccessDenied"
                    },
                    CommonCauses = new List<string>
                    {
                        "User not assigned required role",
                        "User account inactive or locked",
                        "Role permissions changed recently",
                        "User attempting to access restricted resource",
                        "Session expired or authentication token invalid"
                    },
                    ActionSteps = new List<string>
                    {
                        "1. Identify the user and resource from authorization failure log",
                        "2. Navigate to Settings → Users → find user",
                        "3. Verify user has appropriate role (Admin/Admissions/Cashier/Sponsor)",
                        "4. If role missing: assign correct role and save",
                        "5. If account inactive: activate account",
                        "6. Ask user to log out and log back in",
                        "7. Verify access is restored"
                    },
                    WhenToEscalate = "If user should not have access or if security concern identified"
                },
                new RunbookItem
                {
                    Title = "Coverage Evaluation Issues",
                    Category = "Coverage",
                    Description = "Troubleshooting coverage API evaluation problems",
                    WhatToCheck = new List<string>
                    {
                        "Check if sponsor has active LoG",
                        "Verify LoG contains rules for the requested item",
                        "Check LoG status (must be Active and Activated)",
                        "Verify student is tagged to sponsor",
                        "Check school year alignment"
                    },
                    WhereToLook = new List<string>
                    {
                        "Sponsors → Profile → LoG tab",
                        "Admin Reports → LoG Activity Report",
                        "Admin Reports → Coverage Decisions Report",
                        "Activity Logs filtered by Module: Coverage"
                    },
                    CommonCauses = new List<string>
                    {
                        "Student not tagged to any sponsor",
                        "Sponsor has no active LoG",
                        "LoG status is Draft or Inactive",
                        "Item not included in LoG rules",
                        "School year mismatch",
                        "LoG percentage is 0%"
                    },
                    ActionSteps = new List<string>
                    {
                        "1. Look up student and verify sponsor assignment",
                        "2. Navigate to sponsor profile and check LoG tab",
                        "3. If no LoG: create LoG with appropriate rules",
                        "4. If LoG inactive: activate LoG",
                        "5. If item not covered: add rule to LoG (if appropriate)",
                        "6. Test coverage evaluation again",
                        "7. Check Coverage Decisions Report to verify"
                    },
                    WhenToEscalate = "If coverage evaluation consistently returns errors or unexpected results"
                },
                new RunbookItem
                {
                    Title = "Deployment Health Check",
                    Category = "Deployment",
                    Description = "Post-deployment verification steps",
                    WhatToCheck = new List<string>
                    {
                        "Application health status (Operations Dashboard)",
                        "Database connectivity",
                        "Recent sync attempts successful",
                        "No critical authorization failures",
                        "Smoke test results"
                    },
                    WhereToLook = new List<string>
                    {
                        "Operations → Dashboard",
                        "Operations → Smoke Test",
                        "Admin Reports → Sync Status",
                        "Activity Logs"
                    },
                    CommonCauses = new List<string>
                    {
                        "Configuration not updated for new environment",
                        "Database migration not applied",
                        "External service credentials not configured",
                        "Network/firewall blocking connections",
                        "Application pool not started"
                    },
                    ActionSteps = new List<string>
                    {
                        "1. Navigate to Operations → Dashboard",
                        "2. Verify Application Health = Healthy",
                        "3. Check Recent Sync Attempts for any errors",
                        "4. Review Data Consistency Warnings",
                        "5. Navigate to Operations → Smoke Test",
                        "6. Execute all smoke test items",
                        "7. Document any failures",
                        "8. Verify critical workflows (create sponsor, create LoG, evaluate coverage)"
                    },
                    WhenToEscalate = "If health status is Unhealthy or multiple smoke tests fail"
                }
            }
        };

        return model;
    }

    private ReleaseChecklist GetReleaseChecklistContent()
    {
        var checklist = new ReleaseChecklist
        {
            ReleaseName = "ISM Sponsor Management System - Pilot",
            Environment = "Pilot",
            PlannedDate = DateTime.UtcNow,
            PreDeploymentChecks = new List<ChecklistItem>
            {
                new ChecklistItem { Item = "Database backup completed" },
                new ChecklistItem { Item = "Release notes prepared" },
                new ChecklistItem { Item = "Rollback plan documented" },
                new ChecklistItem { Item = "Target environment verified (e.g., Pilot)" },
                new ChecklistItem { Item = "Configuration files reviewed" },
                new ChecklistItem { Item = "External service credentials verified" },
                new ChecklistItem { Item = "Team notified of deployment window" },
                new ChecklistItem { Item = "Change Request/Approval obtained (if required)" }
            },
            DeploymentSteps = new List<ChecklistItem>
            {
                new ChecklistItem { Item = "Stop application pool/service" },
                new ChecklistItem { Item = "Deploy application files" },
                new ChecklistItem { Item = "Update configuration/appsettings" },
                new ChecklistItem { Item = "Run database migrations" },
                new ChecklistItem { Item = "Verify file permissions" },
                new ChecklistItem { Item = "Start application pool/service" },
                new ChecklistItem { Item = "Verify application starts without errors" },
                new ChecklistItem { Item = "Check application logs for startup issues" }
            },
            PostDeploymentVerification = new List<ChecklistItem>
            {
                new ChecklistItem { Item = "Navigate to Operations → Dashboard and verify Health = Healthy" },
                new ChecklistItem { Item = "Execute smoke tests (Operations → Smoke Test)" },
                new ChecklistItem { Item = "Verify login functionality" },
                new ChecklistItem { Item = "Test sponsor creation" },
                new ChecklistItem { Item = "Test LoG creation and activation" },
                new ChecklistItem { Item = "Test coverage evaluation API" },
                new ChecklistItem { Item = "Verify sync to external systems working" },
                new ChecklistItem { Item = "Check audit logging is functioning" },
                new ChecklistItem { Item = "Verify reports are accessible" },
                new ChecklistItem { Item = "Notify team of successful deployment" }
            },
            RollbackProcedure = new List<ChecklistItem>
            {
                new ChecklistItem { Item = "Stop application pool/service", IsRequired = true },
                new ChecklistItem { Item = "Restore previous application files", IsRequired = true },
                new ChecklistItem { Item = "Restore database backup (if migrations applied)", IsRequired = true },
                new ChecklistItem { Item = "Restore previous configuration", IsRequired = true },
                new ChecklistItem { Item = "Start application pool/service", IsRequired = true },
                new ChecklistItem { Item = "Verify application starts", IsRequired = true },
                new ChecklistItem { Item = "Execute critical smoke tests", IsRequired = true },
                new ChecklistItem { Item = "Document reason for rollback", IsRequired = true },
                new ChecklistItem { Item = "Notify team of rollback", IsRequired = true }
            }
        };

        return checklist;
    }
}
