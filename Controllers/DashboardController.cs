using ISMSponsor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly LogsService _logsService;
        private readonly ChangeRequestService _changeRequestService;

        public DashboardController(LogsService logsService, ChangeRequestService changeRequestService)
        {
            _logsService = logsService;
            _changeRequestService = changeRequestService;
        }

        public async Task<IActionResult> Index()
        {
            var user = User;
            string? sponsorId = null;
            if (user.IsInRole("sponsor"))
                sponsorId = user.FindFirstValue("SponsorId");

            var year = HttpContext.Session.GetString("ActiveSchoolYear");
            var logs = await _logsService.GetByYearAsync(year ?? "", sponsorId);
            var changes = await _changeRequestService.GetPendingAsync(sponsorId);

            ViewBag.Logs = logs;
            ViewBag.ChangeRequests = changes;
            return View();
        }
    }
}