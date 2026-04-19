using ISMSponsor.Models.Domain;
using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISMSponsor.Controllers
{
    [Authorize(Roles = "sponsor")]
    public class SponsorRequestController : Controller
    {
        private readonly SponsorChangeRequestService _requestService;
        private readonly SponsorService _sponsorService;

        public SponsorRequestController(
            SponsorChangeRequestService requestService,
            SponsorService sponsorService)
        {
            _requestService = requestService;
            _sponsorService = sponsorService;
        }

        public async Task<IActionResult> Index()
        {
            var sponsorId = User.FindFirstValue("SponsorId");
            if (string.IsNullOrEmpty(sponsorId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var requests = await _requestService.GetRequestsBySponsorAsync(sponsorId);
            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);

            ViewBag.SponsorName = sponsor?.SponsorName ?? "Unknown";
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sponsorId = User.FindFirstValue("SponsorId");
            if (string.IsNullOrEmpty(sponsorId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            if (sponsor == null)
            {
                TempData["Error"] = "Sponsor not found";
                return RedirectToAction("Index");
            }

            var model = new SponsorChangeRequestViewModel
            {
                SponsorId = sponsorId,
                SponsorName = sponsor.SponsorName ?? string.Empty
            };

            return View(model);
        }

        [HttpGet]
        public async Task<PartialViewResult> CreateModal()
        {
            var sponsorId = User.FindFirstValue("SponsorId");
            var sponsor = await _sponsorService.GetByIdAsync(sponsorId ?? string.Empty);

            var model = new SponsorChangeRequestViewModel
            {
                SponsorId = sponsorId ?? string.Empty,
                SponsorName = sponsor?.SponsorName ?? string.Empty
            };

            return PartialView("_CreateRequestModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(SponsorChangeRequestViewModel model)
        {
            var sponsorId = User.FindFirstValue("SponsorId");
            if (string.IsNullOrEmpty(sponsorId) || model.SponsorId != sponsorId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Get current value from sponsor master data
            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            if (sponsor == null)
            {
                TempData["Error"] = "Sponsor not found";
                return RedirectToAction("Profile", "Sponsors");
            }

            model.CurrentValue = GetCurrentFieldValue(sponsor, model.RequestField);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields";
                return RedirectToAction("Profile", "Sponsors");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userDisplay = User.Identity?.Name ?? "Unknown";

            var result = await _requestService.SubmitRequestAsync(model, userId, userDisplay);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Profile", "Sponsors");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SponsorChangeRequestViewModel model)
        {
            var sponsorId = User.FindFirstValue("SponsorId");
            if (string.IsNullOrEmpty(sponsorId) || model.SponsorId != sponsorId)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Get current value from sponsor master data
            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            if (sponsor == null)
            {
                TempData["Error"] = "Sponsor not found";
                return RedirectToAction("Index");
            }

            model.CurrentValue = GetCurrentFieldValue(sponsor, model.RequestField);

            if (!ModelState.IsValid)
            {
                model.SponsorName = sponsor.SponsorName ?? string.Empty;
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userDisplay = User.Identity?.Name ?? "Unknown";

            var result = await _requestService.SubmitRequestAsync(model, userId, userDisplay);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("Details", new { id = result.RequestId });
            }
            else
            {
                TempData["Error"] = result.Message;
                model.SponsorName = sponsor.SponsorName ?? string.Empty;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var sponsorId = User.FindFirstValue("SponsorId");
            if (string.IsNullOrEmpty(sponsorId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null || request.SponsorId != sponsorId)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var sponsorId = User.FindFirstValue("SponsorId");
            if (string.IsNullOrEmpty(sponsorId))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null || request.SponsorId != sponsorId)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userDisplay = User.Identity?.Name ?? "Unknown";

            var result = await _requestService.CancelRequestAsync(id, userId, userDisplay);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentValue(string sponsorId, string field)
        {
            var userSponsorId = User.FindFirstValue("SponsorId");
            if (string.IsNullOrEmpty(userSponsorId) || userSponsorId != sponsorId)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
            if (sponsor == null)
            {
                return Json(new { success = false, message = "Sponsor not found" });
            }

            if (Enum.TryParse<SponsorRequestField>(field, out var fieldEnum))
            {
                var currentValue = GetCurrentFieldValue(sponsor, fieldEnum);
                return Json(new { success = true, currentValue });
            }

            return Json(new { success = false, message = $"Invalid field: {field}" });
        }

        private string GetCurrentFieldValue(Sponsor sponsor, SponsorRequestField field)
        {
            return field switch
            {
                SponsorRequestField.SponsorName => sponsor.SponsorName ?? string.Empty,
                SponsorRequestField.LegalName => sponsor.LegalName ?? string.Empty,
                SponsorRequestField.Address => sponsor.Address ?? string.Empty,
                SponsorRequestField.Tin => sponsor.Tin ?? string.Empty,
                SponsorRequestField.ContactName => "N/A - Please use Contacts page",
                SponsorRequestField.ContactEmail => "N/A - Please use Contacts page",
                SponsorRequestField.ContactPhone => "N/A - Please use Contacts page",
                _ => string.Empty
            };
        }
    }
}
