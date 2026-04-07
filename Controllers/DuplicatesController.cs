using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using Microsoft.EntityFrameworkCore;
using ISMSponsor.Integration.Orchestration.Services;
using ISMSponsor.Integration.Orchestration;

namespace ISMSponsor.Controllers;

/// <summary>
/// Manages duplicate sponsor detection and merge operations.
/// Admin-only functionality for data governance.
/// </summary>
[Authorize(Roles = "admin")]
public class DuplicatesController : Controller
{
    private readonly AppDbContext _context;
    private readonly DuplicateDetectionService _duplicateService;
    private readonly SponsorMergeService _mergeService;
    private readonly ILogger<DuplicatesController> _logger;
    private readonly IIntegrationOrchestrator _integrationOrchestrator;

    public DuplicatesController(
        AppDbContext context,
        DuplicateDetectionService duplicateService,
        SponsorMergeService mergeService,
        ILogger<DuplicatesController> logger,
        IIntegrationOrchestrator integrationOrchestrator)
    {
        _context = context;
        _duplicateService = duplicateService;
        _mergeService = mergeService;
        _logger = logger;
        _integrationOrchestrator = integrationOrchestrator;
    }

    /// <summary>
    /// Display list of detected duplicate sponsors.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var duplicates = await _context.SponsorDuplicateCandidates
            .Include(d => d.PrimarySponsor)
            .Include(d => d.DuplicateSponsor)
            .Where(d => d.Status == "Pending" || d.Status == "UnderReview")
            .OrderByDescending(d => d.MatchScore)
            .ThenByDescending(d => d.DetectedOn)
            .ToListAsync();

        var lastRun = await _context.SponsorDuplicateCandidates
            .OrderByDescending(d => d.DetectedOn)
            .Select(d => d.DetectedOn)
            .FirstOrDefaultAsync();

        var viewModel = new DuplicateDetectionViewModel
        {
            TotalDuplicates = duplicates.Count,
            LastDetectionRun = lastRun,
            DuplicateGroups = duplicates.Select(d => new DuplicateSponsorGroupViewModel
            {
                DuplicateId = d.CandidateId,
                PrimarySponsorId = d.PrimarySponsorId,
                PrimarySponsorName = d.PrimarySponsor?.SponsorName ?? "Unknown",
                PrimarySponsorEmail = "", // Email not available in this view
                PrimarySponsorPhone = "", // Phone not available in this view
                SecondarySponsorId = d.DuplicateSponsorId,
                SecondarySponsorName = d.DuplicateSponsor?.SponsorName ?? "Unknown",
                SecondarySponsorEmail = "",
                SecondarySponsorPhone = "",
                MatchScore = d.MatchScore,
                MatchReason = d.MatchReasons ?? "",
                Status = d.Status,
                DetectedAt = d.DetectedOn,
                IsReviewed = d.ReviewedOn.HasValue,
                ReviewNotes = d.ReviewNotes
            }).ToList()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Run duplicate detection algorithm to find potential duplicates.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunDetection()
    {
        try
        {
            var userId = User.Identity?.Name ?? "System";
            _logger.LogInformation("Starting duplicate detection scan initiated by {User}", userId);

            var detectedCount = await _duplicateService.ScanForDuplicatesAsync(userId);

            TempData["SuccessMessage"] = $"Duplicate detection completed. Found {detectedCount} potential duplicate(s).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running duplicate detection");
            TempData["ErrorMessage"] = "Error running duplicate detection. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Display merge preview for two sponsors.
    /// </summary>
    public async Task<IActionResult> MergePreview(string primaryId, string secondaryId)
    {
        var primary = await _context.Sponsors
            .Include(s => s.Students)
            .Include(s => s.Contacts)
            .FirstOrDefaultAsync(s => s.SponsorId == primaryId);

        var secondary = await _context.Sponsors
            .Include(s => s.Students)
            .Include(s => s.Contacts)
            .FirstOrDefaultAsync(s => s.SponsorId == secondaryId);

        if (primary == null || secondary == null)
        {
            TempData["ErrorMessage"] = "One or both sponsors not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new SponsorMergeViewModel
        {
            PrimarySponsorId = primary.SponsorId,
            PrimarySponsorName = primary.SponsorName,
            PrimarySponsorEmail = "",
            PrimarySponsorPhone = "",
            PrimaryOrganization = primary.LegalName,
            SecondarySponsorId = secondary.SponsorId,
            SecondarySponsorName = secondary.SponsorName,
            SecondarySponsorEmail = "",
            SecondarySponsorPhone = "",
            SecondaryOrganization = secondary.LegalName
        };

        // Detect field conflicts
        if (primary.LegalName != secondary.LegalName && !string.IsNullOrEmpty(secondary.LegalName))
        {
            viewModel.FieldConflicts["Legal Name"] = $"Primary: {primary.LegalName}, Secondary: {secondary.LegalName}";
        }
        if (primary.Tin != secondary.Tin && !string.IsNullOrEmpty(secondary.Tin))
        {
            viewModel.FieldConflicts["TIN"] = $"Primary: {primary.Tin}, Secondary: {secondary.Tin}";
        }

        ViewBag.PrimaryStudentCount = primary.Students?.Count ?? 0;
        ViewBag.SecondaryStudentCount = secondary.Students?.Count ?? 0;
        ViewBag.PrimaryContactCount = primary.Contacts?.Count ?? 0;
        ViewBag.SecondaryContactCount = secondary.Contacts?.Count ?? 0;

        return View(viewModel);
    }

    /// <summary>
    /// Execute merge operation.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExecuteMerge(SponsorMergeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("MergePreview", model);
        }

        try
        {
            var userId = User.Identity?.Name ?? "System";
            
            var result = await _mergeService.ExecuteMergeAsync(
                model.PrimarySponsorId,
                model.SecondarySponsorId,
                userId,
                model.MergeReason,
                null // fieldSelections
            );

            if (result != null && result.Status == "Completed")
            {
                _logger.LogInformation(
                    "Successfully merged sponsor {SecondaryId} into {PrimaryId} by {User}",
                    model.SecondarySponsorId,
                    model.PrimarySponsorId,
                    userId
                );

                // Queue downstream sync to all targets (PowerSchool, NetSuite, OBS, SCP) for merged sponsor
                await _integrationOrchestrator.QueueSponsorSyncAsync(result.SurvivingSponsorId, IntegrationEventType.SponsorMerge, userId);

                TempData["SuccessMessage"] = $"Successfully merged sponsors. {result.ChildRecordsReassigned} child record(s) transferred.";
                return RedirectToAction("Profile", "Sponsors", new { id = result.SurvivingSponsorId });
            }
            else
            {
                TempData["ErrorMessage"] = $"Merge failed: {result?.ErrorMessage ?? "Unknown error"}";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing merge for sponsors {PrimaryId} and {SecondaryId}",
                model.PrimarySponsorId, model.SecondarySponsorId);
            TempData["ErrorMessage"] = "Error executing merge operation. Please try again.";
            return View("MergePreview", model);
        }
    }

    /// <summary>
    /// Mark a duplicate pair as "Not a Duplicate".
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotDuplicate(int duplicateId, string? notes)
    {
        try
        {
            var userId = User.Identity?.Name ?? "System";
            await _duplicateService.MarkAsNotDuplicateAsync(duplicateId, userId, notes ?? "");

            _logger.LogInformation(
                "Duplicate {DuplicateId} marked as 'Not a Duplicate' by {User}",
                duplicateId,
                User.Identity?.Name
            );

            TempData["SuccessMessage"] = "Marked as not a duplicate.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking duplicate as not duplicate");
            TempData["ErrorMessage"] = "Error updating duplicate status.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// View merge history.
    /// </summary>
    public async Task<IActionResult> MergeHistory()
    {
        var mergeOperations = await _context.MergeOperations
            .Include(m => m.SurvivingSponsor)
            .OrderByDescending(m => m.InitiatedOn)
            .Take(50)
            .ToListAsync();

        return View(mergeOperations);
    }
}

