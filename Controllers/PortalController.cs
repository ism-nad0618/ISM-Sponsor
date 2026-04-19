using ISMSponsor.Data;
using ISMSponsor.Models.Domain;
using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ISMSponsor.Controllers
{
    /// <summary>
    /// Self-service portal for sponsor users to manage their information,
    /// view students, LoGs, and submit change requests
    /// </summary>
    [Authorize(Roles = "sponsor")]
    public class PortalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SponsorService _sponsorService;
        private readonly SchoolYearContextService _schoolYearContext;
        private readonly SponsorChangeRequestService _changeRequestService;
        private readonly ILogger<PortalController> _logger;

        public PortalController(
            AppDbContext context,
            SponsorService sponsorService,
            SchoolYearContextService schoolYearContext,
            SponsorChangeRequestService changeRequestService,
            ILogger<PortalController> logger)
        {
            _context = context;
            _sponsorService = sponsorService;
            _schoolYearContext = schoolYearContext;
            _changeRequestService = changeRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Portal home page with overview and quick stats
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized("Sponsor ID not found. Please contact support.");
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            if (sponsor == null)
            {
                return NotFound("Sponsor profile not found.");
            }

            var schoolYear = await _schoolYearContext.GetSelectedSchoolYearAsync();
            var schoolYearId = schoolYear?.SchoolYearId ?? string.Empty;

            var viewModel = new PortalHomeViewModel
            {
                Sponsor = sponsor,
                SelectedSchoolYearId = schoolYearId,
                SelectedSchoolYearName = schoolYear?.Name ?? "Not Selected",
                StudentCount = await _context.Students
                    .Where(s => s.SponsorId == sponsorId && s.SchoolYearId == schoolYearId)
                    .CountAsync(),
                ActiveLoGCount = await _context.LogCoverages
                    .Where(l => l.SponsorId == sponsorId && l.SchoolYearId == schoolYearId && l.IsActive)
                    .CountAsync(),
                PendingRequestCount = await _context.SponsorChangeRequests
                    .Where(r => r.SponsorId == sponsorId && r.Status == "Pending")
                    .CountAsync(),
                RecentLoGs = await _context.LogCoverages
                    .Include(l => l.Student)
                    .Where(l => l.SponsorId == sponsorId && l.SchoolYearId == schoolYearId)
                    .OrderByDescending(l => l.CreatedOn)
                    .Take(5)
                    .ToListAsync(),
                RecentRequests = await _context.SponsorChangeRequests
                    .Where(r => r.SponsorId == sponsorId)
                    .OrderByDescending(r => r.SubmittedOn)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// View all Letters of Guarantee for this sponsor
        /// </summary>
        public async Task<IActionResult> LettersOfGuarantee(string? schoolYear = null, string? search = null, string? status = null)
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized("Sponsor ID not found.");
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            var currentSchoolYear = await _schoolYearContext.GetSelectedSchoolYearAsync();
            var selectedYearId = schoolYear ?? currentSchoolYear?.SchoolYearId ?? string.Empty;

            var schoolYears = await _context.SchoolYears.OrderByDescending(y => y.Name).ToListAsync();
            var selectedYear = schoolYears.FirstOrDefault(y => y.SchoolYearId == selectedYearId);

            var query = _context.LogCoverages
                .Include(l => l.Student)
                .Include(l => l.CoverageRules)
                .Where(l => l.SponsorId == sponsorId && l.SchoolYearId == selectedYearId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.StudentId.Contains(search) || 
                                        (l.Student != null && (l.Student.FirstName.Contains(search) || l.Student.LastName.Contains(search))));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "active")
                    query = query.Where(l => l.IsActive);
                else if (status == "inactive")
                    query = query.Where(l => !l.IsActive);
                else
                    query = query.Where(l => l.LogStatus == status);
            }

            var logs = await query.OrderByDescending(l => l.CreatedOn).ToListAsync();

            var viewModel = new PortalLoGsViewModel
            {
                SponsorId = sponsorId,
                SponsorName = sponsor?.SponsorName ?? "Unknown",
                SelectedSchoolYearId = selectedYearId,
                SelectedSchoolYearName = selectedYear?.Name ?? "Not Selected",
                LettersOfGuarantee = logs,
                AvailableSchoolYears = schoolYears,
                SearchQuery = search,
                StatusFilter = status
            };

            return View(viewModel);
        }

        /// <summary>
        /// View LoG details with coverage rules
        /// </summary>
        public async Task<IActionResult> LoGDetail(int id)
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized();
            }

            var log = await _context.LogCoverages
                .Include(l => l.Student)
                .Include(l => l.Sponsor)
                .Include(l => l.CoverageRules)
                .ThenInclude(r => r.Item)
                .Include(l => l.CoverageRules)
                .ThenInclude(r => r.Category)
                .FirstOrDefaultAsync(l => l.LogId == id && l.SponsorId == sponsorId);

            if (log == null)
            {
                return NotFound("Letter of Guarantee not found or you don't have access to it.");
            }

            var viewModel = new PortalLoGDetailViewModel
            {
                LetterOfGuarantee = log,
                Student = log.Student,
                CoverageRules = log.CoverageRules?.OrderBy(r => r.DisplayOrder).ToList() ?? new(),
                CanDownloadAttachment = !string.IsNullOrEmpty(log.AttachmentFileName)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Download LoG attachment
        /// </summary>
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized();
            }

            var log = await _context.LogCoverages
                .FirstOrDefaultAsync(l => l.LogId == id && l.SponsorId == sponsorId);

            if (log == null || string.IsNullOrEmpty(log.AttachmentFileName))
            {
                return NotFound("Attachment not found.");
            }

            var filePath = Path.Combine("wwwroot", "uploads", "logs", log.SchoolYearId, log.StudentId, log.AttachmentFileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = "application/octet-stream";
            
            return File(fileBytes, contentType, log.AttachmentFileName);
        }

        /// <summary>
        /// View students under this sponsor
        /// </summary>
        public async Task<IActionResult> Students(string? schoolYear = null, string? search = null)
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized();
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            var currentSchoolYear = await _schoolYearContext.GetSelectedSchoolYearAsync();
            var selectedYearId = schoolYear ?? currentSchoolYear?.SchoolYearId ?? string.Empty;

            var schoolYears = await _context.SchoolYears.OrderByDescending(y => y.Name).ToListAsync();
            var selectedYear = schoolYears.FirstOrDefault(y => y.SchoolYearId == selectedYearId);

            var studentsQuery = _context.Students
                .Where(s => s.SponsorId == sponsorId && s.SchoolYearId == selectedYearId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                studentsQuery = studentsQuery.Where(s => 
                    s.StudentId.Contains(search) || 
                    s.FirstName.Contains(search) || 
                    s.LastName.Contains(search));
            }

            var students = await studentsQuery.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync();

            // Get LoGs for these students
            var studentIds = students.Select(s => s.StudentId).ToList();
            var logs = await _context.LogCoverages
                .Where(l => l.SchoolYearId == selectedYearId && studentIds.Contains(l.StudentId))
                .ToDictionaryAsync(l => l.StudentId);

            var studentVMs = students.Select(s => new StudentWithLoGViewModel
            {
                Student = s,
                LetterOfGuarantee = logs.ContainsKey(s.StudentId) ? logs[s.StudentId] : null,
                HasActiveLoG = logs.ContainsKey(s.StudentId) && logs[s.StudentId].IsActive
            }).ToList();

            var viewModel = new PortalStudentsViewModel
            {
                SponsorId = sponsorId,
                SponsorName = sponsor?.SponsorName ?? "Unknown",
                SelectedSchoolYearId = selectedYearId,
                SelectedSchoolYearName = selectedYear?.Name ?? "Not Selected",
                Students = studentVMs,
                AvailableSchoolYears = schoolYears,
                SearchQuery = search
            };

            return View(viewModel);
        }

        /// <summary>
        /// View and manage change requests
        /// </summary>
        public async Task<IActionResult> Requests(string? status = null)
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized();
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);

            var requests = await _changeRequestService.GetRequestsBySponsorAsync(sponsorId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                requests = requests.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var viewModel = new PortalRequestsViewModel
            {
                SponsorId = sponsorId,
                SponsorName = sponsor?.SponsorName ?? "Unknown",
                ChangeRequests = requests,
                PendingCount = requests.Count(r => r.Status.Equals("pending", StringComparison.OrdinalIgnoreCase)),
                ApprovedCount = requests.Count(r => r.Status.Equals("approved", StringComparison.OrdinalIgnoreCase)),
                RejectedCount = requests.Count(r => r.Status.Equals("rejected", StringComparison.OrdinalIgnoreCase)),
                StatusFilter = status
            };

            return View(viewModel);
        }

        /// <summary>
        /// Submit a new change request
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRequest(string field, string fieldLabel, string currentValue, string newValue, string? reason)
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var userDisplay = User.Identity?.Name ?? "Unknown";

            var viewModel = new SponsorChangeRequestViewModel
            {
                SponsorId = sponsorId,
                RequestField = Enum.TryParse<SponsorRequestField>(field, out var parsedField) ? parsedField : SponsorRequestField.SponsorName,
                CurrentValue = currentValue,
                RequestedValue = newValue,
                RequestReason = reason
            };

            var result = await _changeRequestService.SubmitRequestAsync(viewModel, userId, userDisplay);

            if (result.Success)
            {
                TempData["Success"] = "Change request submitted successfully. An administrator will review it.";
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Requests));
        }

        /// <summary>
        /// Manage sponsor contacts
        /// </summary>
        public async Task<IActionResult> Contacts()
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized();
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            var contacts = await _sponsorService.GetContactsAsync(sponsorId);

            var viewModel = new PortalContactsViewModel
            {
                SponsorId = sponsorId,
                SponsorName = sponsor?.SponsorName ?? "Unknown",
                Contacts = contacts,
                ActiveContactsCount = contacts.Count(c => c.IsActive)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Add a new contact
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContact(string name, string email, string phone)
        {
            var sponsorId = GetSponsorId();
            if (string.IsNullOrEmpty(sponsorId))
            {
                return Unauthorized();
            }

            var contact = new SponsorContact
            {
                SponsorId = sponsorId,
                Name = name,
                Email = email,
                Phone = phone,
                IsActive = true
            };

            await _sponsorService.AddContactAsync(contact);

            TempData["Success"] = "Contact added successfully.";
            return RedirectToAction(nameof(Contacts));
        }

        /// <summary>
        /// Profile view (redirect to enhanced Sponsors/Profile)
        /// </summary>
        public IActionResult Profile()
        {
            return RedirectToAction("Profile", "Sponsors");
        }

        /// <summary>
        /// Helper to get sponsor ID from claims
        /// </summary>
        private string GetSponsorId()
        {
            return User.FindFirst("SponsorId")?.Value ?? string.Empty;
        }
    }
}
