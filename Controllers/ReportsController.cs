using ISMSponsor.Data;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Reports hub landing page with available reports
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Prepare data for reports hub
            await PrepareReportsHubDataAsync();
            return View();
        }

        /// <summary>
        /// Sponsor summary report with filtering
        /// </summary>
        [Authorize(Roles = "admin,admissions,cashier")]
        public async Task<IActionResult> SponsorSummary([FromQuery] ReportFilterViewModel filters)
        {
            await PrepareFilterDropdownsAsync();
            
            var query = _context.Sponsors.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                bool isActive = filters.Status == "Active";
                query = query.Where(s => s.IsActive == isActive);
            }

            if (filters.StartDate.HasValue)
            {
                query = query.Where(s => s.CreatedOn >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(s => s.CreatedOn <= filters.EndDate.Value);
            }

            var sponsors = await query
                .OrderBy(s => s.SponsorName)
                .ToListAsync();

            ViewBag.Filters = filters;
            ViewBag.SponsorCount = sponsors.Count;
            ViewBag.ActiveCount = sponsors.Count(s => s.IsActive);
            ViewBag.InactiveCount = sponsors.Count(s => !s.IsActive);

            return View(sponsors);
        }

        /// <summary>
        /// LoG activity report with filtering
        /// </summary>
        [Authorize(Roles = "admin,admissions,cashier")]
        public async Task<IActionResult> LogActivity([FromQuery] ReportFilterViewModel filters)
        {
            await PrepareFilterDropdownsAsync();
            
            var query = _context.LogCoverages
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .Include(l => l.CoverageRules)
                .AsQueryable();

            // Apply filters
            if (filters.SchoolYearId.HasValue)
            {
                var schoolYearId = filters.SchoolYearId.Value.ToString();
                query = query.Where(l => l.SchoolYearId == schoolYearId);
            }

            if (!string.IsNullOrWhiteSpace(filters.SponsorId))
            {
                query = query.Where(l => l.SponsorId == filters.SponsorId);
            }

            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                if (filters.Status == "Active")
                    query = query.Where(l => l.IsActive);
                else if (filters.Status == "Inactive")
                    query = query.Where(l => !l.IsActive);
                else
                    query = query.Where(l => l.LogStatus == filters.Status);
            }

            if (filters.StartDate.HasValue)
            {
                query = query.Where(l => l.CreatedOn >= filters.StartDate.Value);
            }

            if (filters.EndDate.HasValue)
            {
                query = query.Where(l => l.CreatedOn <= filters.EndDate.Value);
            }

            var logs = await query
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            ViewBag.Filters = filters;
            ViewBag.TotalCount = logs.Count;
            ViewBag.ActiveCount = logs.Count(l => l.IsActive);
            ViewBag.InactiveCount = logs.Count(l => !l.IsActive);

            return View(logs);
        }

        /// <summary>
        /// Student enrollment report
        /// </summary>
        [Authorize(Roles = "admin,admissions,cashier")]
        public async Task<IActionResult> StudentEnrollment([FromQuery] ReportFilterViewModel filters)
        {
            await PrepareFilterDropdownsAsync();
            
            var query = _context.Students
                .Include(s => s.Sponsor)
                .AsQueryable();

            // Apply filters
            if (filters.SchoolYearId.HasValue)
            {
                var schoolYearId = filters.SchoolYearId.Value.ToString();
                query = query.Where(s => s.SchoolYearId == schoolYearId);
            }

            if (!string.IsNullOrWhiteSpace(filters.SponsorId))
            {
                query = query.Where(s => s.SponsorId == filters.SponsorId);
            }

            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                query = query.Where(s => s.StudentStatus == filters.Status);
            }

            var students = await query
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            ViewBag.Filters = filters;
            ViewBag.TotalCount = students.Count;
            ViewBag.ActiveCount = students.Count(s => s.StudentStatus == "active");

            return View(students);
        }

        /// <summary>
        /// Prepare common filter dropdowns for reports
        /// </summary>
        private async Task PrepareFilterDropdownsAsync()
        {
            ViewBag.SchoolYears = await _context.SchoolYears
                .OrderByDescending(y => y.Name)
                .ToListAsync();

            ViewBag.Sponsors = await _context.Sponsors
                .Where(s => s.IsActive)
                .OrderBy(s => s.SponsorName)
                .ToListAsync();
        }

        /// <summary>
        /// Prepare data for reports hub landing page
        /// </summary>
        private async Task PrepareReportsHubDataAsync()
        {
            // You could add summary stats here if needed
            ViewBag.TotalSponsors = await _context.Sponsors.CountAsync();
            ViewBag.TotalLoGs = await _context.LogCoverages.CountAsync();
            ViewBag.TotalStudents = await _context.Students.CountAsync();
        }
    }
}