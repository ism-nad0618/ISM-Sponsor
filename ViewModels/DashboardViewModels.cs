using ISMSponsor.Models.Domain;

namespace ISMSponsor.ViewModels
{
    // Base Dashboard ViewModel
    public class BaseDashboardViewModel
    {
        public SchoolYear? SelectedSchoolYear { get; set; }
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
        public List<DashboardAlert> Alerts { get; set; } = new();
        public List<QuickActionItem> QuickActions { get; set; } = new();
    }

    // Admin Dashboard ViewModel
    public class AdminDashboardViewModel : BaseDashboardViewModel
    {
        // System Overview Metrics
        public int TotalSponsors { get; set; }
        public int ActiveSponsors { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLoGs { get; set; }
        public int ActiveLoGs { get; set; }
        
        // Status Distribution
        public StatusDistribution LogStatusDistribution { get; set; } = new();
        public StatusDistribution ChangeRequestDistribution { get; set; } = new();
        
        // Pending Items
        public int PendingLoGs { get; set; }
        public int PendingChangeRequests { get; set; }
        public int PendingReviews { get; set; }
        public int PendingApprovals { get; set; }
        public int FailedIntegrationsToday { get; set; }
        
        // Recent Records
        public List<LogCoverage> RecentLoGs { get; set; } = new();
        public List<Sponsor> RecentSponsors { get; set; } = new();
        
        // System Health
        public int DuplicateSponsorsDetected { get; set; }
        public int SyncIssues { get; set; }
    }

    // Admissions Dashboard ViewModel
    public class AdmissionsDashboardViewModel : BaseDashboardViewModel
    {
        // Student & Enrollment Metrics
        public int TotalStudents { get; set; }
        public int StudentsWithActiveLoGs { get; set; }
        public int StudentsWithoutLoGs { get; set; }
        
        // LoG Review Metrics
        public int LogsAwaitingReview { get; set; }
        public int LogsApprovedToday { get; set; }
        public int LogsRejectedToday { get; set; }
        
        // Status Distribution
        public StatusDistribution LogStatusDistribution { get; set; } = new();
        
        // Work Queue
        public List<LogCoverage> LogsForReview { get; set; } = new();
        public List<Student> RecentEnrollments { get; set; } = new();
        
        // Coverage Analysis
        public decimal TotalCoverageAmount { get; set; }
        public int PartialCoverageCount { get; set; }
        public int FullCoverageCount { get; set; }
    }

    // Cashier Dashboard ViewModel
    public class CashierDashboardViewModel : BaseDashboardViewModel
    {
        // Financial Metrics
        public int ActiveLoGs { get; set; }
        public decimal TotalCoverageAmount { get; set; }
        public decimal PendingBillingAmount { get; set; }
        public int LogsWithPaymentIssues { get; set; }
        
        // Status Distribution
        public StatusDistribution LogStatusDistribution { get; set; } = new();
        
        // Work Items
        public List<LogCoverage> LogsRequiringAttention { get; set; } = new();
        public List<LogCoverage> RecentlyApprovedLoGs { get; set; } = new();
        
        // Billing Alerts
        public int ExpiringLoGsThisMonth { get; set; }
        public int MissingAttachments { get; set; }
        
        // Coverage Breakdown
        public int FullCoverageCount { get; set; }
        public int PartialCoverageCount { get; set; }
        public int NoCoverageCount { get; set; }
    }

    // Supporting Classes
    public class RecentActivityItem
    {
        public string Activity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Icon { get; set; }
        public string? ActionUrl { get; set; }
        public string ActivityType { get; set; } = string.Empty; // "log", "sponsor", "student", "request"
    }

    public class DashboardAlert
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "info"; // "info", "warning", "error", "success"
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class QuickActionItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = "badge-primary";
    }

    public class StatusDistribution
    {
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        
        public int Total => StatusCounts.Values.Sum();
        
        public double GetPercentage(string status)
        {
            if (Total == 0) return 0;
            return StatusCounts.TryGetValue(status, out var count) 
                ? (count * 100.0 / Total) 
                : 0;
        }
    }
}
