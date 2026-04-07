using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers;

[Authorize(Roles = "admin")]
public class AuditController : Controller
{
    private readonly AuditService _auditService;

    public AuditController(AuditService auditService)
    {
        _auditService = auditService;
    }

    // GET: Audit
    public async Task<IActionResult> Index(AuditFilterViewModel? filter)
    {
        var auditLogs = await _auditService.GetAuditLogsAsync(filter);
        var modules = await _auditService.GetAvailableModulesAsync();

        ViewBag.Modules = modules;
        ViewBag.Filter = filter ?? new AuditFilterViewModel();

        return View(auditLogs);
    }

    // GET: Audit/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var auditLog = await _auditService.GetAuditLogByIdAsync(id);
        if (auditLog == null)
        {
            return NotFound();
        }

        // Try to get related coverage evaluation if this is a coverage audit
        ViewBag.CoverageEvaluation = null;
        if (auditLog.Item == "CoverageEvaluation" && auditLog.Details.Contains("Evaluation #"))
        {
            // Parse evaluation ID from details
            var match = System.Text.RegularExpressions.Regex.Match(auditLog.Details, @"Evaluation #(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int evalId))
            {
                var eval = await _auditService.GetCoverageEvaluationByIdAsync(evalId);
                ViewBag.CoverageEvaluation = eval;
            }
        }

        return View(auditLog);
    }

    // GET: Audit/Monitoring
    [HttpGet]
    public async Task<IActionResult> Monitoring()
    {
        var recentFailures = await _auditService.GetRecentFailuresAsync(20);
        var recentCoverageEvaluations = await _auditService.GetRecentCoverageEvaluationsAsync(20);
        var statistics = await _auditService.GetAuditStatisticsAsync();

        ViewBag.RecentFailures = recentFailures;
        ViewBag.RecentCoverageEvaluations = recentCoverageEvaluations;
        ViewBag.Statistics = statistics;

        return View();
    }
}
