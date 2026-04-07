using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers;

[Authorize(Roles = "admin,cashier")]
public class AdminReportsController : Controller
{
    private readonly ReportService _reportService;
    private readonly ExportService _exportService;
    private readonly SponsorService _sponsorService;

    public AdminReportsController(ReportService reportService, ExportService exportService, SponsorService sponsorService)
    {
        _reportService = reportService;
        _exportService = exportService;
        _sponsorService = sponsorService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SponsorMaster([FromQuery] ReportFilterViewModel filters)
    {
        var report = await _reportService.GenerateSponsorMasterReportAsync(filters);

        if (filters.ExportFormat == "csv")
        {
            return ExportSponsorMasterToCsv(report);
        }

        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> LogActivity([FromQuery] ReportFilterViewModel filters)
    {
        var report = await _reportService.GenerateLogActivityReportAsync(filters);

        if (filters.ExportFormat == "csv")
        {
            return ExportLogActivityToCsv(report);
        }

        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> CoverageDecisions([FromQuery] ReportFilterViewModel filters)
    {
        var report = await _reportService.GenerateCoverageDecisionReportAsync(filters);

        if (filters.ExportFormat == "csv")
        {
            return ExportCoverageDecisionsToCsv(report);
        }

        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> SyncStatus([FromQuery] ReportFilterViewModel filters)
    {
        var report = await _reportService.GenerateSyncStatusReportAsync(filters);

        if (filters.ExportFormat == "csv")
        {
            return ExportSyncStatusToCsv(report);
        }

        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> AuditActivity([FromQuery] ReportFilterViewModel filters)
    {
        var report = await _reportService.GenerateAuditActivityReportAsync(filters);

        if (filters.ExportFormat == "csv")
        {
            return ExportAuditActivityToCsv(report);
        }

        return View(report);
    }

    /// <summary>
    /// All Sponsor Contacts report - accessible by admin and cashier roles
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AllContacts()
    {
        var contacts = await _sponsorService.GetAllContactsAsync();
        
        var viewModel = new AllContactsViewModel
        {
            Contacts = contacts.Select(c => new SponsorContactRow
            {
                SponsorContactId = c.SponsorContactId,
                SponsorId = c.SponsorId,
                SponsorName = c.Sponsor?.SponsorName ?? "Unknown",
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                IsActive = c.IsActive
            }).ToList(),
            TotalContacts = contacts.Count,
            ActiveContacts = contacts.Count(c => c.IsActive),
            InactiveContacts = contacts.Count(c => !c.IsActive)
        };

        return View(viewModel);
    }

    private FileResult ExportSponsorMasterToCsv(SponsorMasterReportViewModel report)
    {
        var headers = new[] { "Sponsor ID", "Sponsor Name", "Legal Name", "Is Active", "Student Count", "Active LoG Count", "Created On", "Modified On", "Is Synced" };
        var rows = ResolveSponsorMasterRows(report);

        var csvData = _exportService.ExportToCsv(rows, headers, row => new[]
        {
            GetStringValue(row, "SponsorId"),
            GetStringValue(row, "SponsorName"),
            GetStringValue(row, "LegalName"),
            GetBooleanYesNoValue(row, "IsActive"),
            GetStringValue(row, "StudentCount"),
            GetStringValue(row, "ActiveLogCount"),
            GetDateTimeValue(row, "CreatedOn"),
            GetNullableDateTimeValue(row, "ModifiedOn"),
            GetBooleanYesNoValue(row, "IsSynced")
        });

        return File(csvData, "text/csv", $"SponsorMasterReport_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private FileResult ExportLogActivityToCsv(LogActivityReportViewModel report)
    {
        var headers = new[] { "LoG ID", "Sponsor ID", "Sponsor Name", "Status", "Rule Count", "Created On", "Activated On", "Deactivated On", "Is Active" };
        var rows = ResolveLogActivityRows(report);

        var csvData = _exportService.ExportToCsv(rows, headers, row => new[]
        {
            GetStringValue(row, "LogCoverageId"),
            GetStringValue(row, "SponsorId"),
            GetStringValue(row, "SponsorName"),
            GetStringValue(row, "Status"),
            GetStringValue(row, "RuleCount"),
            GetDateTimeValue(row, "CreatedOn"),
            GetNullableDateTimeValue(row, "ActivatedOn"),
            GetNullableDateTimeValue(row, "DeactivatedOn"),
            GetBooleanYesNoValue(row, "IsActive")
        });

        return File(csvData, "text/csv", $"LogActivityReport_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private FileResult ExportCoverageDecisionsToCsv(CoverageDecisionReportViewModel report)
    {
        var headers = new[] { "Audit ID", "Sponsor ID", "Sponsor Name", "Student ID", "Student Name", "Item Code", "Item Name", "Item Amount", "Decision", "Coverage %", "Covered Amount", "Evaluated At" };
        var rows = ResolveCoverageDecisionRows(report);

        var csvData = _exportService.ExportToCsv(rows, headers, row => new[]
        {
            GetStringValue(row, "AuditId"),
            GetStringValue(row, "SponsorId"),
            GetStringValue(row, "SponsorName"),
            GetStringValue(row, "StudentId"),
            GetStringValue(row, "StudentName"),
            GetStringValue(row, "ItemCode"),
            GetStringValue(row, "ItemName"),
            GetStringValue(row, "ItemAmount"),
            GetStringValue(row, "Decision"),
            GetStringValue(row, "CoveragePercentage"),
            GetStringValue(row, "CoveredAmount"),
            GetDateTimeValue(row, "EvaluatedAt")
        });

        return File(csvData, "text/csv", $"CoverageDecisionsReport_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private FileResult ExportSyncStatusToCsv(SyncStatusReportViewModel report)
    {
        var headers = new[] { "Sync Log ID", "Entity Type", "Entity ID", "Target System", "Event Type", "Error Message", "Attempted At", "Retry Count" };
        
        var csvData = _exportService.ExportToCsv(report.RecentFailures, headers, row => new[]
        {
            row.SyncLogId.ToString(),
            row.EntityType,
            row.EntityId,
            row.TargetSystem,
            row.EventType,
            row.ErrorMessage,
            row.AttemptedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            row.RetryCount.ToString()
        });

        return File(csvData, "text/csv", $"SyncStatusReport_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private FileResult ExportAuditActivityToCsv(AuditActivityReportViewModel report)
    {
        var headers = new[] { "Activity Log ID", "Module", "Action", "User ID", "User Display", "Entity Type", "Entity ID", "Performed At", "IP Address" };
        var rows = ResolveAuditActivityRows(report);

        var csvData = _exportService.ExportToCsv(rows, headers, row => new[]
        {
            GetStringValue(row, "ActivityLogId"),
            GetStringValue(row, "Module"),
            GetStringValue(row, "Action"),
            GetStringValue(row, "UserId"),
            GetStringValue(row, "UserDisplay"),
            GetStringValue(row, "EntityType"),
            GetStringValue(row, "EntityId"),
            GetDateTimeValue(row, "PerformedAt"),
            GetStringValue(row, "IpAddress")
        });

        return File(csvData, "text/csv", $"AuditActivityReport_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static IEnumerable<object> ResolveLogActivityRows(LogActivityReportViewModel report)
    {
        var candidates = new[] { "Logs", "LogActivities", "Items", "Rows", "Results", "Data" };

        foreach (var name in candidates)
        {
            var property = report.GetType().GetProperty(name);
            if (property?.GetValue(report) is System.Collections.IEnumerable enumerable && property.PropertyType != typeof(string))
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                return rows;
            }
        }

        foreach (var property in report.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                continue;
            }

            if (property.GetValue(report) is System.Collections.IEnumerable enumerable)
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                if (rows.Count > 0)
                {
                    return rows;
                }
            }
        }

        return Array.Empty<object>();
    }

    private static IEnumerable<object> ResolveSponsorMasterRows(SponsorMasterReportViewModel report)
    {
        var candidates = new[] { "Sponsors", "Items", "Rows", "Results", "Data" };

        foreach (var name in candidates)
        {
            var property = report.GetType().GetProperty(name);
            if (property?.GetValue(report) is System.Collections.IEnumerable enumerable && property.PropertyType != typeof(string))
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                return rows;
            }
        }

        foreach (var property in report.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                continue;
            }

            if (property.GetValue(report) is System.Collections.IEnumerable enumerable)
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                if (rows.Count > 0)
                {
                    return rows;
                }
            }
        }

        return Array.Empty<object>();
    }

    private static IEnumerable<object> ResolveCoverageDecisionRows(CoverageDecisionReportViewModel report)
    {
        var candidates = new[] { "Decisions", "CoverageDecisions", "Items", "Rows", "Results", "Data" };

        foreach (var name in candidates)
        {
            var property = report.GetType().GetProperty(name);
            if (property?.GetValue(report) is System.Collections.IEnumerable enumerable && property.PropertyType != typeof(string))
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                return rows;
            }
        }

        foreach (var property in report.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                continue;
            }

            if (property.GetValue(report) is System.Collections.IEnumerable enumerable)
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                if (rows.Count > 0)
                {
                    return rows;
                }
            }
        }

        return Array.Empty<object>();
    }

    private static IEnumerable<object> ResolveAuditActivityRows(AuditActivityReportViewModel report)
    {
        var candidates = new[] { "RecentActivity", "Activities", "AuditActivities", "Items", "Rows", "Results", "Data" };

        foreach (var name in candidates)
        {
            var property = report.GetType().GetProperty(name);
            if (property?.GetValue(report) is System.Collections.IEnumerable enumerable && property.PropertyType != typeof(string))
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                return rows;
            }
        }

        foreach (var property in report.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                continue;
            }

            if (property.GetValue(report) is System.Collections.IEnumerable enumerable)
            {
                var rows = new List<object>();
                foreach (var item in enumerable)
                {
                    if (item != null)
                    {
                        rows.Add(item);
                    }
                }

                if (rows.Count > 0)
                {
                    return rows;
                }
            }
        }

        return Array.Empty<object>();
    }

    private static object? GetPropertyValue(object source, string propertyName)
    {
        return source.GetType().GetProperty(propertyName)?.GetValue(source);
    }

    private static string GetStringValue(object source, string propertyName)
    {
        var value = GetPropertyValue(source, propertyName);
        return value?.ToString() ?? "";
    }

    private static string GetBooleanYesNoValue(object source, string propertyName)
    {
        var value = GetPropertyValue(source, propertyName);
        return value is bool b ? (b ? "Yes" : "No") : "";
    }

    private static string GetDateTimeValue(object source, string propertyName)
    {
        var value = GetPropertyValue(source, propertyName);
        return value is DateTime dt ? dt.ToString("yyyy-MM-dd HH:mm:ss") : "";
    }

    private static string GetNullableDateTimeValue(object source, string propertyName)
    {
        var value = GetPropertyValue(source, propertyName);
        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            _ => ""
        };
    }
}
