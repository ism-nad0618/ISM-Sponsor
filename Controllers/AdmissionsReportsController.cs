using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers;

[Authorize(Roles = "admin,admissions")]
public class AdmissionsReportsController : Controller
{
    private readonly ReportService _reportService;
    private readonly ExportService _exportService;

    public AdmissionsReportsController(ReportService reportService, ExportService exportService)
    {
        _reportService = reportService;
        _exportService = exportService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> TrackingReport([FromQuery] ReportFilterViewModel filters)
    {
        var report = await _reportService.GenerateAdmissionsTrackingReportAsync(filters);

        if (filters.ExportFormat == "csv")
        {
            return ExportTrackingReportToCsv(report);
        }

        return View(report);
    }

    private FileResult ExportTrackingReportToCsv(AdmissionsTrackingReportViewModel report)
    {
        var headers = new[] { "Sponsor ID", "Sponsor Name", "Student Count", "Active LoG Count", "Students With Coverage", "Students Without Coverage", "Coverage Completeness %" };
        
        var csvData = _exportService.ExportToCsv(report.CoverageAlignment, headers, row => new[]
        {
            row.SponsorId,
            row.SponsorName,
            row.StudentCount.ToString(),
            row.ActiveLoGCount.ToString(),
            row.StudentsWithCoverage.ToString(),
            row.StudentsWithoutCoverage.ToString(),
            row.CoverageCompleteness.ToString("F1")
        });

        return File(csvData, "text/csv", $"AdmissionsTrackingReport_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }
}
