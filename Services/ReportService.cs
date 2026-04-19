using ISMSponsor.Data;
using ISMSponsor.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Services;

public class ReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SponsorMasterReportViewModel> GenerateSponsorMasterReportAsync(ReportFilterViewModel filters)
    {
        var query = _context.Sponsors.AsQueryable();

        // Apply filters
        if (filters.StartDate.HasValue)
            query = query.Where(s => s.CreatedOn >= filters.StartDate.Value);
        if (filters.EndDate.HasValue)
            query = query.Where(s => s.CreatedOn <= filters.EndDate.Value);
        if (!string.IsNullOrEmpty(filters.Status))
        {
            if (filters.Status == "Active")
                query = query.Where(s => s.IsActive);
            else if (filters.Status == "Inactive")
                query = query.Where(s => !s.IsActive);
        }

        var sponsors = await query
            .Include(s => s.Students)
            .Include(s => s.LettersOfGuarantee)
            .ToListAsync();

        var report = new SponsorMasterReportViewModel
        {
            Filters = filters,
            TotalSponsors = sponsors.Count,
            ActiveSponsors = sponsors.Count(s => s.IsActive),
            InactiveSponsors = sponsors.Count(s => !s.IsActive)
        };

        if (filters.StartDate.HasValue && filters.EndDate.HasValue)
        {
            report.SponsorsCreatedInPeriod = sponsors.Count(s =>
                s.CreatedOn >= filters.StartDate.Value && s.CreatedOn <= filters.EndDate.Value);
            report.SponsorsUpdatedInPeriod = sponsors.Count(s =>
                s.ModifiedOn.HasValue && s.ModifiedOn.Value >= filters.StartDate.Value && s.ModifiedOn.Value <= filters.EndDate.Value);
        }

        // Count merged sponsors
        if (filters.StartDate.HasValue && filters.EndDate.HasValue)
        {
            report.MergedSponsors = await _context.Sponsors
                .Where(s => s.IsMerged && s.MergedOn >= filters.StartDate.Value && s.MergedOn <= filters.EndDate.Value)
                .CountAsync();
        }

        report.SponsorDetails = sponsors.Select(s => new SponsorSummaryRow
        {
            SponsorId = s.SponsorId,
            SponsorName = s.SponsorName,
            LegalName = s.LegalName,
            IsActive = s.IsActive,
            StudentCount = s.Students?.Count ?? 0,
            ActiveLogCount = s.LettersOfGuarantee?.Count(l => l.IsActive) ?? 0,
            CreatedOn = s.CreatedOn,
            ModifiedOn = s.ModifiedOn,
            IsSynced = !string.IsNullOrEmpty(s.PowerSchoolId) || !string.IsNullOrEmpty(s.NetSuiteId)
        }).ToList();

        return report;
    }

    public async Task<LogActivityReportViewModel> GenerateLogActivityReportAsync(ReportFilterViewModel filters)
    {
        var query = _context.LogCoverages.AsQueryable();

        // Apply filters
        if (filters.SchoolYearId.HasValue)
            query = query.Where(l => l.SchoolYearId == filters.SchoolYearId.Value.ToString());
        if (!string.IsNullOrEmpty(filters.SponsorId))
            query = query.Where(l => l.SponsorId == filters.SponsorId);
        if (!string.IsNullOrEmpty(filters.Status))
            query = query.Where(l => l.LogStatus == filters.Status);
        if (filters.StartDate.HasValue)
            query = query.Where(l => l.CreatedOn >= filters.StartDate.Value);
        if (filters.EndDate.HasValue)
            query = query.Where(l => l.CreatedOn <= filters.EndDate.Value);

        var logs = await query
            .Include(l => l.Sponsor)
            .ToListAsync();

        var report = new LogActivityReportViewModel
        {
            Filters = filters,
            TotalLoGs = logs.Count,
            ActiveLoGs = logs.Count(l => l.IsActive),
            InactiveLoGs = logs.Count(l => !l.IsActive),
            PendingApprovalLoGs = logs.Count(l => l.LogStatus == "UnderReview")
        };

        if (filters.StartDate.HasValue && filters.EndDate.HasValue)
        {
            report.LoGsCreatedInPeriod = logs.Count(l =>
                l.CreatedOn >= filters.StartDate.Value && l.CreatedOn <= filters.EndDate.Value);
            report.LoGsModifiedInPeriod = logs.Count(l =>
                l.ModifiedOn.HasValue && l.ModifiedOn.Value >= filters.StartDate.Value && l.ModifiedOn.Value <= filters.EndDate.Value);
        }

        report.LogsByStatus = logs.GroupBy(l => l.LogStatus)
            .ToDictionary(g => g.Key, g => g.Count());

        report.LogDetails = logs.Select(l => new LogActivityRow
        {
            LogCoverageId = l.LogId,
            SponsorId = l.SponsorId,
            SponsorName = l.Sponsor?.SponsorName ?? "Unknown",
            Status = l.LogStatus,
            RuleCount = l.CoverageRules?.Count ?? 0,
            CreatedOn = l.CreatedOn,
            ActivatedOn = l.ActivatedOn,
            DeactivatedOn = l.DeactivatedOn,
            IsActive = l.IsActive
        }).ToList();

        return report;
    }

    public async Task<CoverageDecisionReportViewModel> GenerateCoverageDecisionReportAsync(ReportFilterViewModel filters)
    {
        // Get coverage evaluation audits
        var query = _context.Set<Models.Domain.CoverageEvaluationAudit>().AsQueryable();

        if (filters.StartDate.HasValue)
            query = query.Where(a => a.EvaluatedOn >= filters.StartDate.Value);
        if (filters.EndDate.HasValue)
            query = query.Where(a => a.EvaluatedOn <= filters.EndDate.Value);
        if (!string.IsNullOrEmpty(filters.SponsorId))
            query = query.Where(a => a.SponsorId == filters.SponsorId);
        if (!string.IsNullOrEmpty(filters.StudentId))
            query = query.Where(a => a.StudentId == filters.StudentId);

        var evaluations = await query.ToListAsync();

        var report = new CoverageDecisionReportViewModel
        {
            Filters = filters,
            TotalEvaluations = evaluations.Count
        };

        report.DecisionDetails = evaluations.Select(e => new CoverageDecisionRow
        {
            AuditId = e.AuditId,
            SponsorId = e.SponsorId ?? "",
            SponsorName = e.SponsorId ?? "",
            StudentId = e.StudentId,
            ItemCode = e.ItemId ?? "",
            ItemName = e.ItemId ?? "",
            ItemAmount = e.RequestedAmount,
            Decision = e.Decision,
            CoveragePercentage = e.RequestedAmount > 0 ? (e.SponsorAmount / e.RequestedAmount * 100) : 0,
            CoveredAmount = e.SponsorAmount,
            EvaluatedAt = e.EvaluatedOn
        }).ToList();

        report.CoveredCount = report.DecisionDetails.Count(d => d.Decision == "Covered");
        report.SplitCount = report.DecisionDetails.Count(d => d.Decision == "Split");
        report.NotCoveredCount = report.DecisionDetails.Count(d => d.Decision == "NotCovered");
        report.ErrorCount = evaluations.Count(e => !e.Success);

        return report;
    }

    public async Task<SyncStatusReportViewModel> GenerateSyncStatusReportAsync(ReportFilterViewModel filters)
    {
        var query = _context.Set<Models.Domain.SyncLog>().AsQueryable();

        if (filters.StartDate.HasValue)
            query = query.Where(s => s.AttemptedAt >= filters.StartDate.Value);
        if (filters.EndDate.HasValue)
            query = query.Where(s => s.AttemptedAt <= filters.EndDate.Value);

        var syncLogs = await query.ToListAsync();

        var report = new SyncStatusReportViewModel
        {
            Filters = filters,
            TotalSyncAttempts = syncLogs.Count,
            SuccessfulSyncs = syncLogs.Count(s => s.Status == "Success"),
            FailedSyncs = syncLogs.Count(s => s.Status == "Failed")
        };

        if (report.TotalSyncAttempts > 0)
            report.SuccessRate = (decimal)Math.Round((double)report.SuccessfulSyncs / report.TotalSyncAttempts * 100, 2);

        // Group by target system
        report.SyncBySystem = syncLogs.GroupBy(s => s.TargetSystem)
            .ToDictionary(g => g.Key, g => new SyncSystemStats
            {
                SystemName = g.Key,
                TotalAttempts = g.Count(),
                SuccessfulSyncs = g.Count(s => s.Status == "Success"),
                FailedSyncs = g.Count(s => s.Status == "Failed"),
                SuccessRate = g.Count() > 0 ? (decimal)Math.Round((double)g.Count(s => s.Status == "Success") / g.Count() * 100, 2) : 0,
                LastSuccessfulSyncAt = g.Where(s => s.Status == "Success").Max(s => (DateTime?)s.AttemptedAt),
                LastFailedSyncAt = g.Where(s => s.Status == "Failed").Max(s => (DateTime?)s.AttemptedAt)
            });

        // Recent failures
        report.RecentFailures = syncLogs
            .Where(s => s.Status == "Failed")
            .OrderByDescending(s => s.AttemptedAt)
            .Take(50)
            .Select(s => new SyncFailureRow
            {
                SyncLogId = s.SyncLogId,
                EntityType = s.EntityType,
                EntityId = s.EntityId,
                TargetSystem = s.TargetSystem,
                EventType = s.EventType,
                ErrorMessage = s.ResponsePayload ?? "Unknown error",
                AttemptedAt = s.AttemptedAt,
                RetryCount = s.RetryCount
            }).ToList();

        return report;
    }

    public async Task<AuditActivityReportViewModel> GenerateAuditActivityReportAsync(ReportFilterViewModel filters)
    {
        var query = _context.ActivityLogs.AsQueryable();

        if (filters.StartDate.HasValue)
            query = query.Where(a => a.Date >= filters.StartDate.Value);
        if (filters.EndDate.HasValue)
            query = query.Where(a => a.Date <= filters.EndDate.Value);

        var auditEntries = await query.ToListAsync();

        var report = new AuditActivityReportViewModel
        {
            Filters = filters,
            TotalAuditEntries = auditEntries.Count,
            UserActions = auditEntries.Count(a => !string.IsNullOrEmpty(a.UserDisplay)),
            SystemActions = 0,
            SecurityEvents = 0
        };

        report.AuditByModule = auditEntries.GroupBy(a => a.Item)
            .ToDictionary(g => g.Key, g => g.Count());

        report.AuditByAction = new Dictionary<string, int> { { "Activity", auditEntries.Count } };

        report.RecentEvents = auditEntries
            .OrderByDescending(a => a.Date)
            .Take(100)
            .Select(a => new AuditActivityRow
            {
                ActivityLogId = a.ActivityLogId,
                Module = a.Item,
                Action = "Activity",
                UserId = a.UserDisplay,
                UserDisplay = a.UserDisplay,
                EntityType = a.Item,
                EntityId = "",
                PerformedAt = a.Date,
                IpAddress = ""
            }).ToList();

        return report;
    }

    public async Task<AdmissionsTrackingReportViewModel> GenerateAdmissionsTrackingReportAsync(ReportFilterViewModel filters)
    {
        var report = new AdmissionsTrackingReportViewModel
        {
            Filters = filters
        };

        // Pending work counts
        report.PendingSponsorRequests = await _context.ChangeRequests
            .CountAsync(cr => cr.Status == "pending");

        report.PendingLoGReviews = await _context.LogCoverages
            .CountAsync(l => l.LogStatus == "UnderReview");

        // Recent sponsor requests
        var requests = await _context.ChangeRequests
            .Where(cr => filters.StartDate == null || cr.RequestedOn >= filters.StartDate.Value)
            .Where(cr => filters.EndDate == null || cr.RequestedOn <= filters.EndDate.Value)
            .OrderByDescending(cr => cr.RequestedOn)
            .Take(50)
            .ToListAsync();

        report.RecentSponsorRequests = requests.Select(cr => new SponsorRequestRow
        {
            RequestId = cr.ChangeRequestId,
            SponsorId = cr.SponsorId,
            SponsorName = cr.SponsorId,
            RequestType = cr.Field,
            Status = cr.Status,
            RequestedOn = cr.RequestedOn,
            RequestedBy = cr.RequestedByUserId
        }).ToList();

        // Recent sponsor additions
        var recentSponsors = await _context.Sponsors
            .Where(s => filters.StartDate == null || s.CreatedOn >= filters.StartDate.Value)
            .Where(s => filters.EndDate == null || s.CreatedOn <= filters.EndDate.Value)
            .Include(s => s.Students)
            .OrderByDescending(s => s.CreatedOn)
            .Take(50)
            .ToListAsync();

        report.RecentlyAddedSponsors = recentSponsors.Select(s => new RecentSponsorRow
        {
            SponsorId = s.SponsorId,
            SponsorName = s.SponsorName,
            CreatedOn = s.CreatedOn,
            CreatedBy = s.CreatedByUserId ?? "System",
            StudentCount = s.Students?.Count ?? 0,
            IsSynced = !string.IsNullOrEmpty(s.PowerSchoolId) || !string.IsNullOrEmpty(s.NetSuiteId)
        }).ToList();

        // Recent LoG changes
        var recentLogs = await _context.LogCoverages
            .Where(l => filters.StartDate == null || l.ModifiedOn >= filters.StartDate.Value)
            .Where(l => filters.EndDate == null || l.ModifiedOn <= filters.EndDate.Value)
            .Where(l => l.ModifiedOn.HasValue)
            .Include(l => l.Sponsor)
            .OrderByDescending(l => l.ModifiedOn)
            .Take(50)
            .ToListAsync();

        report.RecentlyChangedLoGs = recentLogs.Select(l => new RecentLogRow
        {
            LogCoverageId = l.LogId,
            SponsorId = l.SponsorId,
            SponsorName = l.Sponsor?.SponsorName ?? "Unknown",
            Status = l.LogStatus,
            ModifiedOn = l.ModifiedOn ?? DateTime.UtcNow,
            RuleCount = l.CoverageRules?.Count ?? 0
        }).ToList();

        // Coverage alignment
        var sponsors = await _context.Sponsors
            .Include(s => s.Students)
            .Include(s => s.LettersOfGuarantee)
            .Where(s => s.IsActive)
            .ToListAsync();

        report.CoverageAlignment = sponsors.Select(s => new CoverageAlignmentRow
        {
            SponsorId = s.SponsorId,
            SponsorName = s.SponsorName,
            StudentCount = s.Students?.Count ?? 0,
            ActiveLoGCount = s.LettersOfGuarantee?.Count(l => l.IsActive) ?? 0,
            StudentsWithCoverage = s.Students?.Count ?? 0,
            StudentsWithoutCoverage = 0,
            CoverageCompleteness = 100.0m
        }).ToList();

        return report;
    }

    public async Task<CashierReconciliationReportViewModel> GenerateCashierReconciliationReportAsync(ReportFilterViewModel filters)
    {
        var report = new CashierReconciliationReportViewModel
        {
            Filters = filters
        };

        // Recent coverage decisions from evaluation audit
        var decisions = await _context.Set<Models.Domain.CoverageEvaluationAudit>()
            .Where(a => filters.StartDate == null || a.EvaluatedOn >= filters.StartDate.Value)
            .Where(a => filters.EndDate == null || a.EvaluatedOn <= filters.EndDate.Value)
            .OrderByDescending(a => a.EvaluatedOn)
            .Take(100)
            .ToListAsync();

        report.RecentDecisions = decisions.Select(e => new RecentCoverageDecisionRow
        {
            AuditId = e.AuditId,
            StudentId = e.StudentId,
            SponsorId = e.SponsorId ?? "",
            SponsorName = e.SponsorId ?? "",
            ItemCode = e.ItemId ?? "",
            ItemAmount = e.RequestedAmount,
            Decision = e.Decision,
            CoveredAmount = e.SponsorAmount,
            EvaluatedAt = e.EvaluatedOn
        }).ToList();

        // Student LoG status
        var students = await _context.Students
            .Include(s => s.Sponsor)
            .ThenInclude(sp => sp!.LettersOfGuarantee)
            .Where(s => filters.StudentId == null || s.StudentId == filters.StudentId)
            .Where(s => filters.SponsorId == null || s.SponsorId == filters.SponsorId)
            .ToListAsync();

        report.StudentLogStatus = students.Select(st => new StudentLogStatusRow
        {
            StudentId = st.StudentId,
            StudentName = $"{st.FirstName} {st.LastName}",
            SponsorId = st.SponsorId ?? "",
            SponsorName = st.Sponsor?.SponsorName ?? "",
            ActiveLoGCount = st.Sponsor?.LettersOfGuarantee?.Count(l => l.IsActive) ?? 0,
            InactiveLoGCount = st.Sponsor?.LettersOfGuarantee?.Count(l => !l.IsActive) ?? 0,
            MostRecentLogStatus = st.Sponsor?.LettersOfGuarantee?.OrderByDescending(l => l.CreatedOn).FirstOrDefault()?.LogStatus ?? "None",
            LastEvaluationDate = null
        }).ToList();

        // Coverage exceptions (simplified - would need more context)
        report.CoverageExceptions = new List<CoverageExceptionRow>();

        return report;
    }

    public async Task<CoverageRulesReportViewModel> GenerateCoverageRulesReportAsync(ReportFilterViewModel filters)
    {
        var query = _context.LoGCoverageRules
            .Include(r => r.LetterOfGuarantee)
                .ThenInclude(l => l.Sponsor)
            .Include(r => r.Item)
            .Include(r => r.Category)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filters.SponsorId))
            query = query.Where(r => r.LetterOfGuarantee != null && r.LetterOfGuarantee.SponsorId == filters.SponsorId);
        if (filters.StartDate.HasValue)
            query = query.Where(r => r.CreatedOn >= filters.StartDate.Value);
        if (filters.EndDate.HasValue)
            query = query.Where(r => r.CreatedOn <= filters.EndDate.Value);
        if (!string.IsNullOrEmpty(filters.Status))
        {
            if (filters.Status == "Active")
                query = query.Where(r => r.IsActive);
            else if (filters.Status == "Inactive")
                query = query.Where(r => !r.IsActive);
        }

        var rules = await query.OrderByDescending(r => r.CreatedOn).ToListAsync();

        var report = new CoverageRulesReportViewModel
        {
            Filters = filters,
            TotalRules = rules.Count,
            ActiveRules = rules.Count(r => r.IsActive),
            InactiveRules = rules.Count(r => !r.IsActive),
            ItemRules = rules.Count(r => r.CoverageTarget == "Item"),
            CategoryRules = rules.Count(r => r.CoverageTarget == "Category")
        };

        report.RuleDetails = rules.Select(r => new CoverageRuleRow
        {
            RuleId = r.RuleId,
            LogId = r.LogId,
            SponsorId = r.LetterOfGuarantee?.SponsorId ?? "",
            SponsorName = r.LetterOfGuarantee?.Sponsor?.SponsorName ?? "",
            CoverageTarget = r.CoverageTarget,
            ItemId = r.ItemId,
            ItemName = r.Item?.ItemName,
            CategoryId = r.CategoryId,
            CategoryName = r.Category?.CategoryName,
            CoverageType = r.CoverageType,
            CoveragePercentage = r.CoveragePercentage,
            CoverageFixedAmount = r.CoverageFixedAmount,
            CapAmount = r.CapAmount,
            EffectiveFrom = r.EffectiveFrom,
            EffectiveTo = r.EffectiveTo,
            IsActive = r.IsActive,
            CreatedOn = r.CreatedOn
        }).ToList();

        return report;
    }
}
