using ISMSponsor.Data;
using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly LogsService _logsService;
        private readonly SchoolYearContextService _schoolYearContext;

        public DashboardController(
            AppDbContext context,
            LogsService logsService, 
            SchoolYearContextService schoolYearContext)
        {
            _context = context;
            _logsService = logsService;
            _schoolYearContext = schoolYearContext;
        }

        public async Task<IActionResult> Index()
        {
            // Redirect sponsor users to their dedicated portal
            if (User.IsInRole("sponsor"))
            {
                return RedirectToAction("Index", "Portal");
            }

            var schoolYear = await _schoolYearContext.GetSelectedSchoolYearAsync();
            var schoolYearId = schoolYear?.SchoolYearId ?? string.Empty;
            
            // Route to role-specific dashboard
            if (User.IsInRole("admin"))
            {
                var viewModel = await BuildAdminDashboardAsync(schoolYearId, schoolYear);
                return View("AdminDashboard", viewModel);
            }
            else if (User.IsInRole("admissions"))
            {
                var viewModel = await BuildAdmissionsDashboardAsync(schoolYearId, schoolYear);
                return View("AdmissionsDashboard", viewModel);
            }
            else if (User.IsInRole("cashier"))
            {
                var viewModel = await BuildCashierDashboardAsync(schoolYearId, schoolYear);
                return View("CashierDashboard", viewModel);
            }
            
            // Fallback to basic view if no specific role
            return View();
        }

        private async Task<AdminDashboardViewModel> BuildAdminDashboardAsync(string schoolYearId, Models.Domain.SchoolYear? schoolYear)
        {
            var viewModel = new AdminDashboardViewModel
            {
                SelectedSchoolYear = schoolYear
            };

            // System Overview Metrics
            viewModel.TotalSponsors = await _context.Sponsors.CountAsync();
            viewModel.ActiveSponsors = await _context.Sponsors.Where(s => s.IsActive).CountAsync();
            viewModel.TotalStudents = await _context.Students.CountAsync();
            viewModel.TotalLoGs = await _context.LogCoverages.Where(l => l.SchoolYearId == schoolYearId).CountAsync();
            viewModel.ActiveLoGs = await _context.LogCoverages.Where(l => l.SchoolYearId == schoolYearId && l.IsActive).CountAsync();

            // Status Distribution
            var logStatuses = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId)
                .GroupBy(l => l.LogStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            
            viewModel.LogStatusDistribution.StatusCounts = logStatuses.ToDictionary(x => x.Status, x => x.Count);

            var changeRequestStatuses = await _context.ChangeRequests
                .GroupBy(cr => cr.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            
            viewModel.ChangeRequestDistribution.StatusCounts = changeRequestStatuses.ToDictionary(x => x.Status, x => x.Count);

            // Pending Items
            viewModel.PendingLoGs = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.LogStatus == "Pending")
                .CountAsync();
            
            viewModel.PendingChangeRequests = await _context.ChangeRequests
                .Where(cr => cr.Status == "Pending")
                .CountAsync();

            // Recent Records
            viewModel.RecentLoGs = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId)
                .OrderByDescending(l => l.CreatedOn)
                .Take(5)
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .ToListAsync();

            viewModel.RecentSponsors = await _context.Sponsors
                .OrderByDescending(s => s.CreatedOn)
                .Take(5)
                .ToListAsync();

            // System Health
            viewModel.DuplicateSponsorsDetected = await _context.SponsorDuplicateCandidates
                .Where(d => d.Status == "Unresolved")
                .CountAsync();

            // Phase 3: Pending Approvals (Sponsors awaiting approval)
            viewModel.PendingApprovals = await _context.Sponsors
                .Where(s => s.ApprovalStatus == "PendingApproval")
                .CountAsync();

            // Phase 3: Failed Integrations - Disabled (Operations page removed)
            viewModel.FailedIntegrationsToday = 0;

            // Build Recent Activities
            viewModel.RecentActivities = await BuildRecentActivitiesAsync(schoolYearId, "admin");

            // Build Alerts
            viewModel.Alerts = BuildAdminAlerts(viewModel);

            // Build Quick Actions
            viewModel.QuickActions = BuildAdminQuickActions(viewModel);

            return viewModel;
        }

        private async Task<AdmissionsDashboardViewModel> BuildAdmissionsDashboardAsync(string schoolYearId, Models.Domain.SchoolYear? schoolYear)
        {
            var viewModel = new AdmissionsDashboardViewModel
            {
                SelectedSchoolYear = schoolYear
            };

            // Student & Enrollment Metrics
            viewModel.TotalStudents = await _context.Students.CountAsync();
            viewModel.StudentsWithActiveLoGs = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.IsActive)
                .Select(l => l.StudentId)
                .Distinct()
                .CountAsync();
            viewModel.StudentsWithoutLoGs = viewModel.TotalStudents - viewModel.StudentsWithActiveLoGs;

            // LoG Review Metrics
            viewModel.LogsAwaitingReview = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.LogStatus == "Pending")
                .CountAsync();

            var today = DateTime.UtcNow.Date;
            viewModel.LogsApprovedToday = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && 
                           l.LogStatus == "Approved" && 
                           l.ModifiedOn != null &&
                           l.ModifiedOn.Value.Date == today)
                .CountAsync();

            viewModel.LogsRejectedToday = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && 
                           l.LogStatus == "Rejected" && 
                           l.ModifiedOn != null &&
                           l.ModifiedOn.Value.Date == today)
                .CountAsync();

            // Status Distribution
            var logStatuses = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId)
                .GroupBy(l => l.LogStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            
            viewModel.LogStatusDistribution.StatusCounts = logStatuses.ToDictionary(x => x.Status, x => x.Count);

            // Work Queue
            viewModel.LogsForReview = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.LogStatus == "Pending")
                .OrderBy(l => l.CreatedOn)
                .Take(10)
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .ToListAsync();

            // Get recent students (sorted by StudentId as proxy for enrollment)
            viewModel.RecentEnrollments = await _context.Students
                .Where(s => s.SchoolYearId == schoolYearId)
                .OrderByDescending(s => s.StudentId)
                .Take(5)
                .ToListAsync();

            // Coverage Analysis - Calculate from rules
            var logsWithRules = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.IsActive)
                .Include(l => l.CoverageRules)
                .ToListAsync();

            var coverageData = logsWithRules.Select(l => new {
                AvgPercentage = l.CoverageRules?.Any() == true 
                    ? l.CoverageRules.Where(r => r.CoveragePercentage.HasValue).Average(r => r.CoveragePercentage) ?? 0
                    : 0
            }).ToList();

            viewModel.TotalCoverageAmount = coverageData.Sum(c => c.AvgPercentage);
            viewModel.FullCoverageCount = coverageData.Count(c => c.AvgPercentage >= 100);
            viewModel.PartialCoverageCount = coverageData.Count(c => c.AvgPercentage > 0 && c.AvgPercentage < 100);

            // Build Recent Activities
            viewModel.RecentActivities = await BuildRecentActivitiesAsync(schoolYearId, "admissions");

            // Build Alerts
            viewModel.Alerts = BuildAdmissionsAlerts(viewModel);

            // Build Quick Actions
            viewModel.QuickActions = BuildAdmissionsQuickActions(viewModel);

            return viewModel;
        }

        private async Task<CashierDashboardViewModel> BuildCashierDashboardAsync(string schoolYearId, Models.Domain.SchoolYear? schoolYear)
        {
            var viewModel = new CashierDashboardViewModel
            {
                SelectedSchoolYear = schoolYear
            };

            // Financial Metrics
            viewModel.ActiveLoGs = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.IsActive)
                .CountAsync();

            // Get coverage data with rules
            var logsData = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.IsActive)
                .Include(l => l.CoverageRules)
                .ToListAsync();

            var coverageData = logsData.Select(l => new { 
                AvgPercentage = l.CoverageRules?.Any() == true 
                    ? l.CoverageRules.Where(r => r.CoveragePercentage.HasValue).Average(r => r.CoveragePercentage) ?? 0
                    : 0,
                HasAttachment = !string.IsNullOrEmpty(l.AttachmentFileName)
            }).ToList();

            viewModel.TotalCoverageAmount = coverageData.Sum(c => c.AvgPercentage);
            viewModel.LogsWithPaymentIssues = coverageData.Count(c => !c.HasAttachment);

            // Status Distribution
            var logStatuses = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId)
                .GroupBy(l => l.LogStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            
            viewModel.LogStatusDistribution.StatusCounts = logStatuses.ToDictionary(x => x.Status, x => x.Count);

            // Work Items
            viewModel.LogsRequiringAttention = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && 
                           (string.IsNullOrEmpty(l.AttachmentFileName) || l.LogStatus == "Pending"))
                .OrderBy(l => l.CreatedOn)
                .Take(10)
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .ToListAsync();

            viewModel.RecentlyApprovedLoGs = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && l.LogStatus == "Approved")
                .OrderByDescending(l => l.ModifiedOn)
                .Take(5)
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .ToListAsync();

            // Billing Alerts
            var oneMonthFromNow = DateTime.UtcNow.AddMonths(1);
            viewModel.ExpiringLoGsThisMonth = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && 
                           l.IsActive && 
                           l.EffectiveTo.HasValue &&
                           l.EffectiveTo.Value <= oneMonthFromNow)
                .CountAsync();

            viewModel.MissingAttachments = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId && 
                           l.IsActive && 
                           string.IsNullOrEmpty(l.AttachmentFileName))
                .CountAsync();

            // Coverage Breakdown
            viewModel.FullCoverageCount = coverageData.Count(c => c.AvgPercentage >= 100);
            viewModel.PartialCoverageCount = coverageData.Count(c => c.AvgPercentage > 0 && c.AvgPercentage < 100);
            viewModel.NoCoverageCount = coverageData.Count(c => c.AvgPercentage == 0);

            // Build Recent Activities
            viewModel.RecentActivities = await BuildRecentActivitiesAsync(schoolYearId, "cashier");

            // Build Alerts
            viewModel.Alerts = BuildCashierAlerts(viewModel);

            // Build Quick Actions
            viewModel.QuickActions = BuildCashierQuickActions(viewModel);

            return viewModel;
        }

        private async Task<List<RecentActivityItem>> BuildRecentActivitiesAsync(string schoolYearId, string role)
        {
            var activities = new List<RecentActivityItem>();

            // Recent LoGs
            var recentLogs = await _context.LogCoverages
                .Where(l => l.SchoolYearId == schoolYearId)
                .OrderByDescending(l => l.CreatedOn)
                .Take(5)
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .ToListAsync();

            foreach (var log in recentLogs)
            {
                activities.Add(new RecentActivityItem
                {
                    Activity = $"LoG {log.LogStatus ?? "Created"}",
                    Description = $"{log.Student?.FirstName} {log.Student?.LastName} - {log.Sponsor?.SponsorName}",
                    Timestamp = log.CreatedOn,
                    Icon = "📄",
                    ActionUrl = $"/LetterOfGuarantee/Details/{log.LogId}",
                    ActivityType = "log"
                });
            }

            // Recent Change Requests (for admin)
            if (role == "admin")
            {
                var recentRequests = await _context.ChangeRequests
                    .OrderByDescending(cr => cr.RequestedOn)
                    .Take(3)
                    .Include(cr => cr.Sponsor)
                    .ToListAsync();

                foreach (var request in recentRequests)
                {
                    activities.Add(new RecentActivityItem
                    {
                        Activity = $"Change Request {request.Status}",
                        Description = $"{request.Sponsor?.SponsorName} - {request.Field}",
                        Timestamp = request.RequestedOn,
                        Icon = "✏️",
                        ActionUrl = $"/Sponsors/ReviewChangeRequest/{request.ChangeRequestId}",
                        ActivityType = "request"
                    });
                }
            }

            return activities.OrderByDescending(a => a.Timestamp).Take(8).ToList();
        }

        private List<DashboardAlert> BuildAdminAlerts(AdminDashboardViewModel model)
        {
            var alerts = new List<DashboardAlert>();

            if (model.PendingLoGs > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Pending LoGs",
                    Message = $"{model.PendingLoGs} Letter(s) of Guarantee awaiting approval",
                    Severity = "warning",
                    ActionUrl = "/LetterOfGuarantee",
                    ActionText = "Review LoGs"
                });
            }

            if (model.PendingChangeRequests > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Pending Change Requests",
                    Message = $"{model.PendingChangeRequests} sponsor change request(s) need review",
                    Severity = "info",
                    ActionUrl = "/ReviewRequest",
                    ActionText = "Review Requests"
                });
            }

            if (model.DuplicateSponsorsDetected > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Duplicate Sponsors Detected",
                    Message = $"{model.DuplicateSponsorsDetected} potential duplicate sponsor(s) found",
                    Severity = "warning",
                    ActionUrl = "/Duplicates",
                    ActionText = "Review Duplicates"
                });
            }

            // Phase 3: Pending Approvals Alert
            if (model.PendingApprovals > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Sponsors Awaiting Approval",
                    Message = $"{model.PendingApprovals} new sponsor(s) pending approval",
                    Severity = "warning",
                    ActionUrl = "/Sponsors",
                    ActionText = "Review Sponsors"
                });
            }

            // Integration Failures alert removed - Operations page no longer exists

            return alerts;
        }

        private List<DashboardAlert> BuildAdmissionsAlerts(AdmissionsDashboardViewModel model)
        {
            var alerts = new List<DashboardAlert>();

            if (model.LogsAwaitingReview > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "LoGs Awaiting Review",
                    Message = $"{model.LogsAwaitingReview} Letter(s) of Guarantee need review",
                    Severity = "warning",
                    ActionUrl = "/LetterOfGuarantee",
                    ActionText = "Review Queue"
                });
            }

            if (model.StudentsWithoutLoGs > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Students Without LoGs",
                    Message = $"{model.StudentsWithoutLoGs} student(s) have no active LoG",
                    Severity = "info",
                    ActionUrl = "/Reports/StudentEnrollment",
                    ActionText = "View Report"
                });
            }

            return alerts;
        }

        private List<DashboardAlert> BuildCashierAlerts(CashierDashboardViewModel model)
        {
            var alerts = new List<DashboardAlert>();

            if (model.MissingAttachments > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Missing Attachments",
                    Message = $"{model.MissingAttachments} active LoG(s) missing attachments",
                    Severity = "warning",
                    ActionUrl = "/LetterOfGuarantee",
                    ActionText = "View LoGs"
                });
            }

            if (model.ExpiringLoGsThisMonth > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Expiring LoGs",
                    Message = $"{model.ExpiringLoGsThisMonth} LoG(s) expiring this month",
                    Severity = "info",
                    ActionUrl = "/LetterOfGuarantee",
                    ActionText = "Review LoGs"
                });
            }

            if (model.LogsWithPaymentIssues > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Payment Issues",
                    Message = $"{model.LogsWithPaymentIssues} LoG(s) with potential payment issues",
                    Severity = "warning",
                    ActionUrl = "/CashierReports",
                    ActionText = "View Report"
                });
            }

            return alerts;
        }

        private List<QuickActionItem> BuildAdminQuickActions(AdminDashboardViewModel model)
        {
            return new List<QuickActionItem>
            {
                new QuickActionItem
                {
                    Title = "Create Sponsor",
                    Description = "Add a new sponsor to the system",
                    ActionUrl = "/Sponsors/Create",
                    Icon = "➕",
                    BadgeText = ""
                },
                new QuickActionItem
                {
                    Title = "Review LoGs",
                    Description = "Process pending Letters of Guarantee",
                    ActionUrl = "/LetterOfGuarantee",
                    Icon = "📋",
                    BadgeText = model.PendingLoGs.ToString(),
                    BadgeClass = "badge-warning"
                },
                new QuickActionItem
                {
                    Title = "System Reports",
                    Description = "View comprehensive system reports",
                    ActionUrl = "/Reports",
                    Icon = "📊",
                    BadgeText = ""
                },
                new QuickActionItem
                {
                    Title = "Manage Duplicates",
                    Description = "Review and merge duplicate sponsors",
                    ActionUrl = "/Duplicates",
                    Icon = "🔍",
                    BadgeText = model.DuplicateSponsorsDetected > 0 ? model.DuplicateSponsorsDetected.ToString() : "",
                    BadgeClass = "badge-warning"
                }
            };
        }

        private List<QuickActionItem> BuildAdmissionsQuickActions(AdmissionsDashboardViewModel model)
        {
            return new List<QuickActionItem>
            {
                new QuickActionItem
                {
                    Title = "Review LoGs",
                    Description = "Process Letters of Guarantee queue",
                    ActionUrl = "/LetterOfGuarantee",
                    Icon = "📋",
                    BadgeText = model.LogsAwaitingReview.ToString(),
                    BadgeClass = "badge-warning"
                },
                new QuickActionItem
                {
                    Title = "Student Reports",
                    Description = "View student enrollment reports",
                    ActionUrl = "/Reports/StudentEnrollment",
                    Icon = "🎓",
                    BadgeText = ""
                },
                new QuickActionItem
                {
                    Title = "LoG Activity",
                    Description = "View LoG activity report",
                    ActionUrl = "/Reports/LogActivity",
                    Icon = "📊",
                    BadgeText = ""
                },
                new QuickActionItem
                {
                    Title = "Coverage Analysis",
                    Description = "Analyze student coverage data",
                    ActionUrl = "/AdmissionsReports",
                    Icon = "💰",
                    BadgeText = ""
                }
            };
        }

        private List<QuickActionItem> BuildCashierQuickActions(CashierDashboardViewModel model)
        {
            return new List<QuickActionItem>
            {
                new QuickActionItem
                {
                    Title = "Review LoGs",
                    Description = "Process active Letters of Guarantee",
                    ActionUrl = "/LetterOfGuarantee",
                    Icon = "📋",
                    BadgeText = model.LogsRequiringAttention.Count.ToString(),
                    BadgeClass = "badge-info"
                },
                new QuickActionItem
                {
                    Title = "Financial Reports",
                    Description = "View cashier and financial reports",
                    ActionUrl = "/CashierReports",
                    Icon = "💰",
                    BadgeText = ""
                },
                new QuickActionItem
                {
                    Title = "Missing Attachments",
                    Description = "LoGs requiring documentation",
                    ActionUrl = "/LetterOfGuarantee",
                    Icon = "📎",
                    BadgeText = model.MissingAttachments.ToString(),
                    BadgeClass = "badge-warning"
                },
                new QuickActionItem
                {
                    Title = "Coverage Report",
                    Description = "View sponsor coverage summary",
                    ActionUrl = "/Reports/SponsorSummary",
                    Icon = "📊",
                    BadgeText = ""
                }
            };
        }
    }
}