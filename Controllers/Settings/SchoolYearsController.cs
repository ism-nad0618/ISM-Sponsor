using ISMSponsor.Services;
using ISMSponsor.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class SchoolYearsController : Controller
    {
        private readonly SchoolYearService _service;

        public SchoolYearsController(SchoolYearService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _service.GetAllAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchoolYear schoolYear)
        {
            if (!ModelState.IsValid)
                return View(schoolYear);

            var success = await _service.CreateAsync(schoolYear);
            if (!success)
            {
                TempData["Error"] = "School year with this ID already exists.";
                return View(schoolYear);
            }

            TempData["Success"] = "School year created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Settings/SchoolYears/CreateModal")]
        public async Task<IActionResult> CreateModal(SchoolYear schoolYear)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );
                
                return Json(new { success = false, message = "Please correct the validation errors.", validationErrors = errors });
            }

            var success = await _service.CreateAsync(schoolYear);
            if (!success)
            {
                return Json(new { success = false, message = "School year with this ID already exists." });
            }

            return Json(new { success = true, message = "School year created successfully!" });
        }

        public async Task<IActionResult> Edit(string id)
        {
            var schoolYear = await _service.GetByIdAsync(id);
            if (schoolYear == null)
                return NotFound();

            return View(schoolYear);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SchoolYear schoolYear)
        {
            if (!ModelState.IsValid)
                return View(schoolYear);

            var success = await _service.UpdateAsync(schoolYear);
            if (!success)
            {
                TempData["Error"] = "Failed to update school year.";
                return View(schoolYear);
            }

            TempData["Success"] = "School year updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
            {
                TempData["Error"] = "Cannot delete school year. It may have associated students.";
            }
            else
            {
                TempData["Success"] = "School year deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("Settings/SchoolYears/EditInline")]
        public async Task<IActionResult> EditInline([FromBody] SchoolYear schoolYear)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var success = await _service.UpdateAsync(schoolYear);
            if (!success)
            {
                return Json(new { success = false, message = "Failed to update school year" });
            }

            return Json(new { success = true, message = "School year updated successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> SetActive(string id)
        {
            await _service.SetActiveAsync(id);
            HttpContext.Session.SetString("ActiveSchoolYear", id);
            return RedirectToAction("Index");
        }
    }
}