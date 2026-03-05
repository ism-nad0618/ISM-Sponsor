using ISMSponsor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class LogsController : Controller
    {
        private readonly LogsService _logsService;

        public LogsController(LogsService logsService)
        {
            _logsService = logsService;
        }

        public async Task<IActionResult> Index()
        {
            string? sponsorId = null;
            if (User.IsInRole("sponsor"))
            {
                sponsorId = User.FindFirst("SponsorId")?.Value;
            }

            var year = HttpContext.Session.GetString("ActiveSchoolYear") ?? string.Empty;
            var logs = await _logsService.GetByYearAsync(year, sponsorId);
            ViewBag.ActivityLogs = await _logsService.GetRecentActivityAsync(year);
            return View(logs);
        }

        [HttpGet]
        public async Task<PartialViewResult> Upload(string schoolYearId, string studentId)
        {
            var logs = await _logsService.GetByYearAsync(schoolYearId, null);
            var item = logs.FirstOrDefault(l => l.StudentId == studentId);
            return PartialView("_CoverageModal", item);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string schoolYearId, string studentId, IFormFile file)
        {
            if (file == null)
            {
                TempData["Error"] = "Please select a file before uploading.";
                return RedirectToAction("Index");
            }

            try
            {
                var role = User.IsInRole("admin") ? "Admin" :
                           User.IsInRole("admissions") ? "Admissions" :
                           User.IsInRole("cashier") ? "Cashier" :
                           "Sponsor";
                var display = User.Identity?.Name ?? "Unknown User";
                await _logsService.UploadCoverageAsync(schoolYearId, studentId, file, display, role);
                TempData["Success"] = "LoGs file uploaded successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin,admissions,cashier")]
        [HttpPost]
        public async Task<IActionResult> ReviewStatus(string schoolYearId, string studentId, string status, string? comment)
        {
            try
            {
                var role = User.IsInRole("admin") ? "Admin" :
                           User.IsInRole("admissions") ? "Admissions" :
                           "Cashier";
                var display = User.Identity?.Name ?? "Unknown User";

                await _logsService.UpdateStatusAsync(schoolYearId, studentId, status, comment, display, role);
                TempData["Success"] = "LoGs status updated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
