using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers;

[Authorize(Roles = "admin,cashier")]
public class CashierReportsController : Controller
{
    private readonly ReportService _reportService;
    private readonly ExportService _exportService;

    public CashierReportsController(ReportService reportService, ExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ReconciliationReport([FromQuery] ReportFilterViewModel filters)
    {
        var report = await _reportService.GenerateCashierReconciliationReportAsync(filters);

        if (filters.ExportFormat == "csv")
        {
            return ExportReconciliationReportToCsv(report);
        }

        return View(report);
    }

    private FileResult ExportReconciliationReportToCsv(CashierReconciliationReportViewModel report)
    {
        var headers = new[] { "Student ID", "Student Name", "Sponsor ID", "Sponsor Name", "Active LoG Count", "Inactive LoG Count", "Most Recent LoG Status", "Last Evaluation Date" };
        
        var csvData = _exportService.ExportToCsv(report.StudentLogStatus, headers, row => new[]
        {
            row.StudentId,
            row.StudentName,
            row.SponsorId,
            row.SponsorName,
            row.ActiveLoGCount.ToString(),
            row.InactiveLoGCount.ToString(),
            row.MostRecentLogStatus,
            row.LastEvaluationDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""
        }!);

        return File(csvData, "text/csv", $"CashierReconciliationReport_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }
}
