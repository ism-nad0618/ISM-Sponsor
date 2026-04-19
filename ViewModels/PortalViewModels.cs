using ISMSponsor.Models.Domain;

namespace ISMSponsor.ViewModels
{
    /// <summary>
    /// ViewModel for Sponsor Portal Home page
    /// </summary>
    public class PortalHomeViewModel
    {
        public Sponsor Sponsor { get; set; } = new();
        public int StudentCount { get; set; }
        public int ActiveLoGCount { get; set; }
        public int PendingRequestCount { get; set; }
        public List<LogCoverage> RecentLoGs { get; set; } = new();
        public List<SponsorChangeRequest> RecentRequests { get; set; } = new();
        public string SelectedSchoolYearId { get; set; } = string.Empty;
        public string SelectedSchoolYearName { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for Portal LoGs listing
    /// </summary>
    public class PortalLoGsViewModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string SelectedSchoolYearId { get; set; } = string.Empty;
        public string SelectedSchoolYearName { get; set; } = string.Empty;
        public List<LogCoverage> LettersOfGuarantee { get; set; } = new();
        public List<SchoolYear> AvailableSchoolYears { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
    }

    /// <summary>
    /// ViewModel for Portal Students listing
    /// </summary>
    public class PortalStudentsViewModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string SelectedSchoolYearId { get; set; } = string.Empty;
        public string SelectedSchoolYearName { get; set; } = string.Empty;
        public List<StudentWithLoGViewModel> Students { get; set; } = new();
        public List<SchoolYear> AvailableSchoolYears { get; set; } = new();
        public string? SearchQuery { get; set; }
    }

    /// <summary>
    /// Student with associated LoG information
    /// </summary>
    public class StudentWithLoGViewModel
    {
        public Student Student { get; set; } = new();
        public LogCoverage? LetterOfGuarantee { get; set; }
        public bool HasActiveLoG { get; set; }
    }

    /// <summary>
    /// ViewModel for Portal Change Requests listing
    /// </summary>
    public class PortalRequestsViewModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public List<SponsorChangeRequest> ChangeRequests { get; set; } = new();
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public string? StatusFilter { get; set; }
    }

    /// <summary>
    /// ViewModel for Portal Contacts management
    /// </summary>
    public class PortalContactsViewModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public List<SponsorContact> Contacts { get; set; } = new();
        public int ActiveContactsCount { get; set; }
    }

    /// <summary>
    /// ViewModel for LoG details in portal
    /// </summary>
    public class PortalLoGDetailViewModel
    {
        public LogCoverage LetterOfGuarantee { get; set; } = new();
        public Student? Student { get; set; }
        public List<LoGCoverageRule> CoverageRules { get; set; } = new();
        public bool CanDownloadAttachment { get; set; }
    }
}
