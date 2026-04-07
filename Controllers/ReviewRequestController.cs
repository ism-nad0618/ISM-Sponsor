using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISMSponsor.Controllers
{
    [Authorize(Roles = "admin,admissions")]
    [Route("[controller]")]
    public class ReviewRequestController : Controller
    {
        private readonly SponsorChangeRequestService _requestService;

        public ReviewRequestController(SponsorChangeRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? status, string? sponsorId, string? search, string sortBy = "SubmittedOn", string sortOrder = "desc")
        {
            ViewBag.Status = status ?? "";
            ViewBag.SponsorId = sponsorId;
            ViewBag.Search = search ?? "";
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            var requests = await _requestService.GetAllRequestsAsync(status, sponsorId);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                requests = requests.Where(r =>
                    (r.Sponsor?.SponsorName?.ToLower().Contains(searchLower) ?? false) ||
                    (r.SponsorId?.ToLower().Contains(searchLower) ?? false) ||
                    (r.RequestField?.ToLower().Contains(searchLower) ?? false)
                ).ToList();
            }

            // Apply sorting
            requests = sortBy switch
            {
                "RequestId" => sortOrder == "asc" ? requests.OrderBy(r => r.RequestId).ToList() : requests.OrderByDescending(r => r.RequestId).ToList(),
                "SponsorName" => sortOrder == "asc" ? requests.OrderBy(r => r.Sponsor?.SponsorName).ToList() : requests.OrderByDescending(r => r.Sponsor?.SponsorName).ToList(),
                "RequestField" => sortOrder == "asc" ? requests.OrderBy(r => r.RequestField).ToList() : requests.OrderByDescending(r => r.RequestField).ToList(),
                "Status" => sortOrder == "asc" ? requests.OrderBy(r => r.Status).ToList() : requests.OrderByDescending(r => r.Status).ToList(),
                "SubmittedBy" => sortOrder == "asc" ? requests.OrderBy(r => r.SubmittedByUserDisplay).ToList() : requests.OrderByDescending(r => r.SubmittedByUserDisplay).ToList(),
                "SubmittedOn" => sortOrder == "asc" ? requests.OrderBy(r => r.SubmittedOn).ToList() : requests.OrderByDescending(r => r.SubmittedOn).ToList(),
                _ => requests.OrderByDescending(r => r.SubmittedOn).ToList()
            };

            return View(requests);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpGet("GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var request = await _requestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("admin");
            return Json(new
            {
                requestId = request.RequestId,
                sponsorId = request.SponsorId,
                sponsorName = request.Sponsor?.SponsorName,
                requestField = request.RequestField,
                currentValue = request.CurrentValue ?? "(empty)",
                requestedValue = request.RequestedValue,
                requestReason = request.RequestReason,
                status = request.Status,
                submittedByUserDisplay = request.SubmittedByUserDisplay,
                submittedOn = request.SubmittedOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                reviewedByUserDisplay = request.ReviewedByUserDisplay,
                reviewedOn = request.ReviewedOn?.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                reviewNotes = request.ReviewNotes,
                appliedByUserDisplay = request.AppliedByUserDisplay,
                appliedOn = request.AppliedOn?.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                appliedValue = request.AppliedValue,
                canReview = request.Status == "Pending",
                canApply = request.Status == "Approved" && isAdmin
            });
        }

        [HttpPost("ReviewAjax")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReviewAjax([FromBody] ReviewRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request data" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userDisplay = User.Identity?.Name ?? "Unknown";

            (bool Success, string Message) result = model.Action.ToLower() switch
            {
                "approve" => await _requestService.ApproveRequestAsync(model.RequestId, model.ReviewNotes ?? string.Empty, userId, userDisplay),
                "reject" => await _requestService.RejectRequestAsync(model.RequestId, model.ReviewNotes ?? string.Empty, userId, userDisplay),
                _ => (false, "Invalid action")
            };

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost("ApplyAjax/{id}")]
        [IgnoreAntiforgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ApplyAjax(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userDisplay = User.Identity?.Name ?? "Unknown";

            var result = await _requestService.ApplyRequestAsync(id, userId, userDisplay);

            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost("Review")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ReviewRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid request data";
                return RedirectToAction("Details", new { id = model.RequestId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userDisplay = User.Identity?.Name ?? "Unknown";

            (bool Success, string Message) result = model.Action.ToLower() switch
            {
                "approve" => await _requestService.ApproveRequestAsync(model.RequestId, model.ReviewNotes ?? string.Empty, userId, userDisplay),
                "reject" => await _requestService.RejectRequestAsync(model.RequestId, model.ReviewNotes ?? string.Empty, userId, userDisplay),
                "apply" => await _requestService.ApplyRequestAsync(model.RequestId, userId, userDisplay),
                _ => (false, "Invalid action")
            };

            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Details", new { id = model.RequestId });
        }

        [HttpPost("Apply/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Apply(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var userDisplay = User.Identity?.Name ?? "Unknown";

            var result = await _requestService.ApplyRequestAsync(id, userId, userDisplay);

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
    }
}
