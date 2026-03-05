using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class SponsorsController : Controller
    {
        private readonly SponsorService _sponsorService;
        private readonly ChangeRequestService _changeService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ISMSponsor.Models.ApplicationUser> _userManager;
        private readonly Microsoft.Extensions.Logging.ILogger<SponsorsController> _logger;

        public SponsorsController(SponsorService sponsorService, ChangeRequestService changeService, Microsoft.AspNetCore.Identity.UserManager<ISMSponsor.Models.ApplicationUser> userManager, Microsoft.Extensions.Logging.ILogger<SponsorsController> logger)
        {
            _sponsorService = sponsorService;
            _changeService = changeService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Profile(string? id = null)
        {
            // administrators may view arbitrary sponsor by id
            if (User.IsInRole("admin"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var adminSponsor = await _sponsorService.GetByIdAsync(id);
                    if (adminSponsor == null) return NotFound($"Sponsor with ID {id} not found.");
                    return View(adminSponsor);
                }
                return RedirectToAction("Index");
            }

            // Try claim first
            string sponsorId = User.FindFirst("SponsorId")?.Value ?? string.Empty;

            // If claim missing, attempt to resolve from the authenticated user record
            if (string.IsNullOrEmpty(sponsorId))
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId) && _userManager != null)
                {
                    var appUser = await _userManager.FindByIdAsync(userId);
                    sponsorId = appUser?.SponsorId ?? string.Empty;
                }
            }

            if (string.IsNullOrEmpty(sponsorId))
            {
                // if user not in sponsor role, deny access rather than error
                if (!User.IsInRole("sponsor"))
                {
                    _logger?.LogInformation("Non-sponsor user {User} attempted Sponsor/Profile", User.Identity?.Name);
                    return Forbid();
                }
                // Diagnostic information for development troubleshooting
                var claimsList = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                var userName = User.Identity?.Name ?? "(null)";
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "(null)";
                var appUser = (string.IsNullOrEmpty(userId) || _userManager == null) ? null : await _userManager.FindByIdAsync(userId);
                var userSponsor = appUser?.SponsorId ?? "(null)";

                var details = new
                {
                    Message = "Sponsor ID not found in user claims or user profile.",
                    UserName = userName,
                    UserId = userId,
                    UserSponsorId = userSponsor,
                    Claims = claimsList
                };
                // Log details for diagnostics
                try { _logger?.LogWarning("SponsorId missing: {Details}", System.Text.Json.JsonSerializer.Serialize(details)); } catch {}

                if (HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) is Microsoft.AspNetCore.Hosting.IWebHostEnvironment env && env.IsDevelopment())
                {
                    return BadRequest(System.Text.Json.JsonSerializer.Serialize(details));
                }

                return BadRequest("Sponsor ID not found in user claims or user profile.");
            }

            var sponsor = await _sponsorService.GetByIdAsync(sponsorId);

            if (sponsor == null)
            {
                return NotFound($"Sponsor with ID {sponsorId} not found.");
            }

            return View(sponsor);
        }

        [HttpPost]
        public async Task<IActionResult> RequestChange(ChangeRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cr = new ISMSponsor.Models.Domain.ChangeRequest
                {
                    SponsorId = model.SponsorId,
                    Field = model.Field,
                    FieldLabel = model.FieldLabel,
                    CurrentValue = model.CurrentValue,
                    NewValue = model.NewValue,
                    RequestedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "",
                    Status = "pending"
                };
                await _changeService.SubmitAsync(cr);
            }
            return RedirectToAction("Profile");
        }

        public async Task<PartialViewResult> Contacts(string sponsorId)
        {
            var contacts = await _sponsorService.GetContactsAsync(sponsorId);
            return PartialView("_ContactsModal", contacts);
        }

        [HttpPost]
        public async Task<IActionResult> Contacts(ISMSponsor.Models.Domain.SponsorContact contact)
        {
            if (ModelState.IsValid)
            {
                await _sponsorService.AddContactAsync(contact);
            }
            return RedirectToAction("Profile");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var list = await _sponsorService.GetAllAsync();
            return View(list);
        }
    }
}