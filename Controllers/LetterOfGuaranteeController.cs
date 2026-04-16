using ISMSponsor.Models.Domain;
using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class LetterOfGuaranteeController : Controller
    {
        private readonly LetterOfGuaranteeService _logService;
        private readonly SchoolYearService _schoolYearService;
        private readonly SponsorService _sponsorService;
        private readonly LogsService _logsService;
        private readonly UserManager<ISMSponsor.Models.ApplicationUser> _userManager;
        private readonly Data.AppDbContext _context;

        public LetterOfGuaranteeController(
            LetterOfGuaranteeService logService,
            SchoolYearService schoolYearService,
            SponsorService sponsorService,
            LogsService logsService,
            UserManager<ISMSponsor.Models.ApplicationUser> userManager,
            Data.AppDbContext context)
        {
            _logService = logService;
            _schoolYearService = schoolYearService;
            _sponsorService = sponsorService;
            _logsService = logsService;
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Roles = "admin,admissions,cashier")]
        public async Task<IActionResult> Index(string? schoolYear = null, string? search = null)
        {
            var years = await _schoolYearService.GetAllAsync();
            ViewBag.SchoolYears = years;
            ViewBag.SelectedYear = schoolYear;
            ViewBag.Search = search;

            var logs = await _logService.GetAllAsync(schoolYear, null, search);
            return View(logs);
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            await PopulateRuleDropdownsAsync();
            return View(new CreateLogViewModel());
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                await PopulateRuleDropdownsAsync();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id ?? "";

            // Prepare LoG and coverage rules
            var log = new LogCoverage
            {
                SchoolYearId = model.SchoolYearId,
                StudentId = model.StudentId,
                SponsorId = model.SponsorId,
                EffectiveFrom = model.EffectiveFrom,
                EffectiveTo = model.EffectiveTo,
                Notes = model.Notes,
                LogStatus = "Draft"
            };

            var rules = model.CoverageRules.Select(r => new LoGCoverageRule
            {
                CoverageTarget = r.CoverageTarget,
                ItemId = r.ItemId,
                CategoryId = r.CategoryId,
                CoverageType = r.CoverageType,
                CoveragePercentage = r.CoveragePercentage,
                CoverageFixedAmount = r.CoverageFixedAmount,
                CapAmount = r.CapAmount,
                EffectiveFrom = r.EffectiveFrom,
                EffectiveTo = r.EffectiveTo,
                ExceptionNote = r.ExceptionNote,
                DisplayOrder = r.DisplayOrder,
                IsActive = r.IsActive
            }).ToList();

            // Create LoG with rules using service
            var (success, createdLog, errors) = await _logService.CreateLoGWithRulesAsync(log, rules, userId);

            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error);
                }
                await PopulateDropdownsAsync();
                await PopulateRuleDropdownsAsync();
                return View(model);
            }

            // Handle attachment if provided
            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                await _logService.SaveAttachmentAsync(createdLog!.LogId, model.Attachment);
            }

            // Log activity
            await _logsService.LogActivityAsync(
                item: "LoG Created",
                details: $"Created LoG #{createdLog!.LogId} for Student {model.StudentId} under Sponsor {model.SponsorId} with {model.CoverageRules.Count} coverage rules",
                userDisplay: user?.DisplayName ?? User.Identity?.Name ?? "System",
                roleName: User.IsInRole("admin") ? "admin" : "admissions",
                schoolYearId: model.SchoolYearId
            );

            TempData["Success"] = "Letter of Guarantee created successfully.";
            return RedirectToAction(nameof(Details), new { id = createdLog.LogId });
        }

        [Authorize(Roles = "admin,admissions,cashier")]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _logService.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var log = await _logService.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            var model = new EditLogViewModel
            {
                LogId = log.LogId,
                SchoolYearId = log.SchoolYearId,
                StudentId = log.StudentId,
                SponsorId = log.SponsorId,
                LogStatus = log.LogStatus,
                EffectiveFrom = log.EffectiveFrom,
                EffectiveTo = log.EffectiveTo,
                Notes = log.Notes,
                ReviewComments = log.ReviewComments,
                ExistingAttachmentFileName = log.AttachmentFileName,
                IsActive = log.IsActive,
                ActivatedOn = log.ActivatedOn,
                DeactivatedOn = log.DeactivatedOn,
                CoverageRules = log.CoverageRules?.Select(r => new CoverageRuleViewModel
                {
                    RuleId = r.RuleId,
                    CoverageTarget = r.CoverageTarget,
                    ItemId = r.ItemId,
                    CategoryId = r.CategoryId,
                    CoverageType = r.CoverageType,
                    CoveragePercentage = r.CoveragePercentage,
                    CoverageFixedAmount = r.CoverageFixedAmount,
                    CapAmount = r.CapAmount,
                    EffectiveFrom = r.EffectiveFrom,
                    EffectiveTo = r.EffectiveTo,
                    ExceptionNote = r.ExceptionNote,
                    DisplayOrder = r.DisplayOrder,
                    IsActive = r.IsActive
                }).ToList() ?? new List<CoverageRuleViewModel>()
            };

            await PopulateDropdownsAsync();
            await PopulateRuleDropdownsAsync();
            ViewBag.StatusOptions = new SelectList(new[]
            {
                "Draft",
                "Submitted",
                "UnderReview",
                "Approved",
                "Rejected"
            }, log.LogStatus);

            return View(model);
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                await PopulateRuleDropdownsAsync();
                return View(model);
            }

            var log = await _logService.GetByIdAsync(model.LogId);
            if (log == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id ?? "";

            // Validate coverage rules
            var rules = model.CoverageRules.Select(r => new LoGCoverageRule
            {
                CoverageTarget = r.CoverageTarget,
                ItemId = r.ItemId,
                CategoryId = r.CategoryId,
                CoverageType = r.CoverageType,
                CoveragePercentage = r.CoveragePercentage,
                CoverageFixedAmount = r.CoverageFixedAmount,
                CapAmount = r.CapAmount,
                EffectiveFrom = r.EffectiveFrom,
                EffectiveTo = r.EffectiveTo,
                ExceptionNote = r.ExceptionNote,
                DisplayOrder = r.DisplayOrder,
                IsActive = r.IsActive
            }).ToList();

            var validationErrors = await _logService.ValidateCoverageRules(rules);
            if (validationErrors.Any())
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError("", error);
                }
                await PopulateDropdownsAsync();
                await PopulateRuleDropdownsAsync();
                return View(model);
            }

            log.SponsorId = model.SponsorId;
            log.LogStatus = model.LogStatus;
            log.EffectiveFrom = model.EffectiveFrom;
            log.EffectiveTo = model.EffectiveTo;
            log.Notes = model.Notes;
            log.ReviewComments = model.ReviewComments;

            await _logService.UpdateAsync(log, userId);

            // Handle coverage rules - remove existing, add new
            var existingRules = await _logService.GetCoverageRulesAsync(model.LogId);
            foreach (var existingRule in existingRules)
            {
                await _logService.DeleteCoverageRuleAsync(existingRule.RuleId);
            }

            foreach (var ruleModel in model.CoverageRules)
            {
                var rule = new LoGCoverageRule
                {
                    LogId = log.LogId,
                    CoverageTarget = ruleModel.CoverageTarget,
                    ItemId = ruleModel.ItemId,
                    CategoryId = ruleModel.CategoryId,
                    CoverageType = ruleModel.CoverageType,
                    CoveragePercentage = ruleModel.CoveragePercentage,
                    CoverageFixedAmount = ruleModel.CoverageFixedAmount,
                    CapAmount = ruleModel.CapAmount,
                    EffectiveFrom = ruleModel.EffectiveFrom,
                    EffectiveTo = ruleModel.EffectiveTo,
                    ExceptionNote = ruleModel.ExceptionNote,
                    DisplayOrder = ruleModel.DisplayOrder,
                    IsActive = ruleModel.IsActive
                };

                await _logService.AddCoverageRuleAsync(rule, userId);
            }

            // Handle new attachment if provided
            if (model.NewAttachment != null && model.NewAttachment.Length > 0)
            {
                await _logService.SaveAttachmentAsync(log.LogId, model.NewAttachment);
            }

            // Log activity
            await _logsService.LogActivityAsync(
                item: "LoG Updated",
                details: $"Updated LoG #{log.LogId} - Status: {model.LogStatus}, Rules: {model.CoverageRules.Count}",
                userDisplay: user?.DisplayName ?? User.Identity?.Name ?? "System",
                roleName: User.IsInRole("admin") ? "admin" : "admissions",
                schoolYearId: log.SchoolYearId
            );

            TempData["Success"] = "Letter of Guarantee updated successfully.";
            return RedirectToAction(nameof(Details), new { id = log.LogId });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id ?? "";
            var userDisplay = user?.DisplayName ?? User.Identity?.Name ?? "System";

            try
            {
                var success = await _logService.ActivateAsync(id, userId, userDisplay, "admin");
                if (success)
                {
                    TempData["Success"] = "Letter of Guarantee activated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to activate LoG. It may already be active or not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Deactivation reason is required.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id ?? "";
            var userDisplay = user?.DisplayName ?? User.Identity?.Name ?? "System";

            var success = await _logService.DeactivateAsync(id, reason, userId, userDisplay, "admin");
            if (success)
            {
                TempData["Success"] = "Letter of Guarantee deactivated successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to deactivate LoG. It may not be active or not found.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task PopulateDropdownsAsync()
        {
            var schoolYears = await _schoolYearService.GetAllAsync();
            var sponsors = await _sponsorService.GetAllAsync();
            var students = await _context.Students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync();

            ViewBag.SchoolYears = new SelectList(schoolYears, "SchoolYearId", "SchoolYearId");
            ViewBag.Sponsors = new SelectList(sponsors, "SponsorId", "SponsorName");
            ViewBag.Students = new SelectList(students, "StudentId", "StudentId");
        }

        private async Task PopulateRuleDropdownsAsync()
        {
            var items = await _context.Items.Where(i => i.IsActive).OrderBy(i => i.ItemName).ToListAsync();
            var categories = await _context.ItemCategories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();

            ViewBag.Items = new SelectList(items, "ItemId", "ItemName");
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            ViewBag.CoverageTargetOptions = new SelectList(new[] { "Item", "Category" });
            ViewBag.CoverageTypeOptions = new SelectList(new[] { "Full", "Percentage", "FixedAmount", "UpToCap" });
        }

        [Authorize(Roles = "admin,admissions,cashier")]
        public async Task<IActionResult> Coverage(int id)
        {
            var log = await _logService.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpGet]
        public async Task<IActionResult> GetFormData()
        {
            var schoolYears = await _schoolYearService.GetAllAsync();
            var sponsors = await _sponsorService.GetAllAsync();
            var students = await _context.Students
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            // Get items and categories for rules
            var items = await _context.Items
                .Where(i => i.IsActive)
                .Select(i => new
                {
                    value = i.ItemId,
                    text = i.ItemName,
                    categoryId = i.CategoryId,
                    gradeLevel = i.GradeLevel
                })
                .Cast<object>()
                .ToListAsync();

            var categories = await _context.ItemCategories
                .Where(c => c.IsActive)
                .Select(c => new { value = c.CategoryId, text = c.CategoryName })
                .ToListAsync();

            return Json(new
            {
                schoolYears = schoolYears.Select(y => new { 
                    value = y.SchoolYearId, 
                    text = y.SchoolYearId,
                    validFrom = y.ValidFrom.ToString("yyyy-MM-dd"),
                    validTo = y.ValidTo.ToString("yyyy-MM-dd")
                }),
                sponsors = sponsors.Select(s => new { value = s.SponsorId, text = s.SponsorName }),
                students = students.Select(s => new {
                    value = s.StudentId,
                    text = $"{s.StudentId} - {s.FirstName} {s.LastName}",
                    gradeLevel = s.GradeLevel
                }),
                items,
                categories
            });
        }

        // Check if a LoG already exists for student/school year
        [Authorize(Roles = "admin,admissions")]
        [HttpGet]
        public async Task<IActionResult> CheckExisting(string schoolYearId, string studentId)
        {
            var existingLog = await _context.LogCoverages
                .FirstOrDefaultAsync(l => l.SchoolYearId == schoolYearId && l.StudentId == studentId);

            if (existingLog != null)
            {
                return Json(new { exists = true, logId = existingLog.LogId });
            }

            return Json(new { exists = false });
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpPost]
        public async Task<IActionResult> CreateModal([FromBody] CreateLogModalViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid value"
                );
                return Json(new { success = false, errors, message = "Validation failed" });
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id ?? "";

                var log = new LogCoverage
                {
                    SchoolYearId = model.SchoolYearId,
                    StudentId = model.StudentId,
                    SponsorId = model.SponsorId,
                    EffectiveFrom = model.EffectiveFrom,
                    EffectiveTo = model.EffectiveTo,
                    Notes = model.Notes,
                    LogStatus = "Draft",
                    IsActive = false
                };

                var (success, createdLog, validationErrors) = await _logService.CreateLoGWithRulesAsync(log, null, userId);

                if (!success)
                {
                    return Json(new { success = false, message = string.Join(", ", validationErrors) });
                }

                // Log the activity
                await _logsService.LogActivityAsync(
                    item: "New LoG",
                    details: $"Created LoG #{createdLog!.LogId} for student {model.StudentId}",
                    userDisplay: user?.DisplayName ?? User.Identity?.Name ?? "System",
                    roleName: User.IsInRole("admin") ? "admin" : "admissions",
                    schoolYearId: model.SchoolYearId
                );

                return Json(new { success = true, message = "Letter of Guarantee created successfully", logId = createdLog.LogId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error creating LoG: {ex.Message}" });
            }
        }

        // Create LoG with coverage rules and attachment (full modal)
        [Authorize(Roles = "admin,admissions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModalFull([FromForm] CreateLogModalFullViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id ?? "";

                // Check if LoG already exists
                if (await _logService.LogExistsForStudentAsync(model.SchoolYearId, model.StudentId))
                {
                    return Json(new { success = false, message = "A Letter of Guarantee already exists for this student in the selected school year." });
                }

                var log = new LogCoverage
                {
                    SchoolYearId = model.SchoolYearId,
                    StudentId = model.StudentId,
                    SponsorId = model.SponsorId,
                    LogStatus = model.LogStatus ?? "Draft",
                    EffectiveFrom = model.EffectiveFrom,
                    EffectiveTo = model.EffectiveTo,
                    Notes = model.Notes,
                    ReviewComments = model.ReviewComments,
                    IsActive = false,
                    CreatedOn = DateTime.Now,
                    CreatedByUserId = userId
                };

                // Handle file upload
                if (model.Attachment != null && model.Attachment.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "log_attachments");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"new_{DateTime.Now:yyyyMMddHHmmss}_{model.Attachment.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }

                    log.AttachmentFileName = fileName;
                    log.AttachmentUploadedOn = DateTime.Now;
                }

                // Create the LoG
                _context.LogCoverages.Add(log);
                await _context.SaveChangesAsync();

                // Add coverage rules
                if (!string.IsNullOrEmpty(model.CoverageRulesJson))
                {
                    var rulesData = System.Text.Json.JsonSerializer.Deserialize<List<CoverageRuleEditModel>>(model.CoverageRulesJson);

                    if (rulesData != null && rulesData.Any())
                    {
                        foreach (var ruleData in rulesData)
                        {
                            var rule = new LoGCoverageRule
                            {
                                LogId = log.LogId,
                                CoverageTarget = ruleData.CoverageTarget,
                                ItemId = ruleData.ItemId,
                                CategoryId = ruleData.CategoryId,
                                CoverageType = ruleData.CoverageType,
                                CoveragePercentage = ruleData.CoveragePercentage,
                                CoverageFixedAmount = ruleData.CoverageFixedAmount,
                                CapAmount = ruleData.CapAmount,
                                EffectiveFrom = ruleData.EffectiveFrom,
                                EffectiveTo = ruleData.EffectiveTo,
                                ExceptionNote = ruleData.ExceptionNote,
                                DisplayOrder = ruleData.DisplayOrder,
                                IsActive = true,
                                CreatedOn = DateTime.Now,
                                CreatedByUserId = userId
                            };

                            _context.LoGCoverageRules.Add(rule);
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                // Log the activity
                await _logsService.LogActivityAsync(
                    item: "New LoG",
                    details: $"Created LoG #{log.LogId} for student {model.StudentId} with {log.CoverageRules?.Count ?? 0} coverage rules",
                    userDisplay: user?.DisplayName ?? User.Identity?.Name ?? "System",
                    roleName: User.IsInRole("admin") ? "admin" : "admissions",
                    schoolYearId: model.SchoolYearId
                );

                return Json(new { success = true, message = "Letter of Guarantee created successfully", logId = log.LogId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error creating LoG: {ex.Message}" });
            }
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpPost]
        public async Task<IActionResult> ValidateImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please select a CSV file to upload" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Only CSV files are supported" });
            }

            var validationResults = new List<object>();
            int willAdd = 0, hasErrors = 0;

            try
            {
                using var reader = new System.IO.StreamReader(file.OpenReadStream());
                var header = await reader.ReadLineAsync();
                
                if (header == null)
                {
                    return Json(new { success = false, message = "CSV file is empty" });
                }

                var lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    var itemErrors = new List<string>();
                    string action = "Skip";
                    
                    if (values.Length < 6)
                    {
                        itemErrors.Add("Invalid format - expected 6 columns");
                    }
                    else
                    {
                        var schoolYearId = values[0].Trim();
                        var studentId = values[1].Trim();
                        var sponsorId = values[2].Trim();
                        var effectiveFromStr = values[3].Trim();
                        var effectiveToStr = values[4].Trim();
                        var notes = values[5].Trim();

                        // Validate required fields
                        if (string.IsNullOrEmpty(schoolYearId))
                            itemErrors.Add("Missing SchoolYearId");
                        if (string.IsNullOrEmpty(studentId))
                            itemErrors.Add("Missing StudentId");
                        if (string.IsNullOrEmpty(sponsorId))
                            itemErrors.Add("Missing SponsorId");

                        // Check if LoG already exists
                        if (itemErrors.Count == 0 && await _logService.LogExistsForStudentAsync(schoolYearId, studentId))
                        {
                            itemErrors.Add($"LoG already exists for student {studentId} in {schoolYearId}");
                        }

                        // Validate date formats
                        if (!string.IsNullOrEmpty(effectiveFromStr) && !DateTime.TryParse(effectiveFromStr, out _))
                        {
                            itemErrors.Add("Invalid EffectiveFrom date format");
                        }
                        if (!string.IsNullOrEmpty(effectiveToStr) && !DateTime.TryParse(effectiveToStr, out _))
                        {
                            itemErrors.Add("Invalid EffectiveTo date format");
                        }

                        if (itemErrors.Count == 0)
                        {
                            action = "Add";
                            willAdd++;
                        }
                        else
                        {
                            hasErrors++;
                        }

                        validationResults.Add(new
                        {
                            lineNumber,
                            schoolYearId,
                            studentId,
                            sponsorId,
                            effectiveFrom = effectiveFromStr,
                            effectiveTo = effectiveToStr,
                            notes = notes?.Length > 50 ? notes.Substring(0, 50) + "..." : notes,
                            action,
                            errors = itemErrors,
                            hasError = itemErrors.Count > 0
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    items = validationResults,
                    summary = new
                    {
                        total = validationResults.Count,
                        willAdd,
                        hasErrors
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Validation failed: {ex.Message}" });
            }
        }

        [Authorize(Roles = "admin,admissions")]
        [HttpPost]
        public async Task<IActionResult> BulkImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please select a CSV file to upload" });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Only CSV files are supported" });
            }

            var successCount = 0;
            var failedCount = 0;
            var errors = new List<string>();

            try
            {
                using var reader = new System.IO.StreamReader(file.OpenReadStream());
                var header = await reader.ReadLineAsync();
                
                if (header == null)
                {
                    return Json(new { success = false, message = "CSV file is empty" });
                }

                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id ?? "";
                var lineNumber = 1;

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    if (values.Length < 6)
                    {
                        errors.Add($"Line {lineNumber}: Invalid format - expected 6 columns");
                        failedCount++;
                        continue;
                    }

                    try
                    {
                        var schoolYearId = values[0].Trim();
                        var studentId = values[1].Trim();
                        var sponsorId = values[2].Trim();
                        var effectiveFrom = DateTime.TryParse(values[3].Trim(), out var from) ? from : (DateTime?)null;
                        var effectiveTo = DateTime.TryParse(values[4].Trim(), out var to) ? to : (DateTime?)null;
                        var notes = values[5].Trim();

                        // Validate required fields
                        if (string.IsNullOrEmpty(schoolYearId) || string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(sponsorId))
                        {
                            errors.Add($"Line {lineNumber}: Missing required fields (SchoolYearId, StudentId, or SponsorId)");
                            failedCount++;
                            continue;
                        }

                        // Check if LoG already exists
                        if (await _logService.LogExistsForStudentAsync(schoolYearId, studentId))
                        {
                            errors.Add($"Line {lineNumber}: LoG already exists for student {studentId} in {schoolYearId}");
                            failedCount++;
                            continue;
                        }

                        // Create the LoG
                        var log = new LogCoverage
                        {
                            SchoolYearId = schoolYearId,
                            StudentId = studentId,
                            SponsorId = sponsorId,
                            EffectiveFrom = effectiveFrom,
                            EffectiveTo = effectiveTo,
                            Notes = notes,
                            LogStatus = "Draft",
                            IsActive = false
                        };

                        await _logService.CreateAsync(log, userId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Line {lineNumber}: {ex.Message}");
                        failedCount++;
                    }
                }

                // Log the bulk import activity
                await _logsService.LogActivityAsync(
                    item: "LoG Bulk Import",
                    details: $"Imported {successCount} LoGs. Failed: {failedCount}",
                    userDisplay: user?.DisplayName ?? User.Identity?.Name ?? "System",
                    roleName: User.IsInRole("admin") ? "admin" : "admissions",
                    schoolYearId: ""
                );

                return Json(new
                {
                    success = true,
                    message = $"Import completed: {successCount} successful, {failedCount} failed",
                    details = new { successCount, failedCount, errors }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error processing file: {ex.Message}" });
            }
        }

        // Get LoG data for editing in modal
        [Authorize(Roles = "admin,admissions")]
        [HttpGet]
        public async Task<IActionResult> GetEditData(int id)
        {
            try
            {
                var log = await _context.LogCoverages
                    .Include(l => l.Student)
                    .Include(l => l.Sponsor)
                    .FirstOrDefaultAsync(l => l.LogId == id);

                if (log == null)
                {
                    return Json(new { success = false, message = "LoG not found" });
                }

                // Get sponsors
                var sponsors = (await _sponsorService.GetAllAsync())
                    .Select(s => new { value = s.SponsorId, text = s.SponsorName })
                    .ToList();

                // Get items and categories for rules
                var items = await _context.Items
                    .Where(i => i.IsActive)
                    .Select(i => new
                    {
                        value = i.ItemId.ToString(),
                        text = i.ItemName,
                        categoryId = i.CategoryId,
                        gradeLevel = i.GradeLevel
                    })
                    .Cast<object>()
                    .ToListAsync();

                var categories = await _context.ItemCategories
                    .Where(c => c.IsActive)
                    .Select(c => new { value = c.CategoryId, text = c.CategoryName })
                    .ToListAsync();

                // Get coverage rules
                var coverageRules = await _context.LoGCoverageRules
                    .Where(r => r.LogId == id && r.IsActive)
                    .OrderBy(r => r.DisplayOrder)
                    .Select(r => new
                    {
                        ruleId = r.RuleId,
                        coverageTarget = r.CoverageTarget,
                        itemId = r.ItemId,
                        categoryId = r.CategoryId,
                        coverageType = r.CoverageType,
                        coveragePercentage = r.CoveragePercentage,
                        coverageFixedAmount = r.CoverageFixedAmount,
                        capAmount = r.CapAmount,
                        effectiveFrom = r.EffectiveFrom.HasValue ? r.EffectiveFrom.Value.ToString("yyyy-MM-dd") : null,
                        effectiveTo = r.EffectiveTo.HasValue ? r.EffectiveTo.Value.ToString("yyyy-MM-dd") : null,
                        displayOrder = r.DisplayOrder,
                        exceptionNote = r.ExceptionNote,
                        isActive = r.IsActive
                    })
                    .Cast<object>()
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    logId = log.LogId,
                    schoolYearId = log.SchoolYearId,
                    studentId = log.StudentId,
                    studentName = log.Student != null ? $"{log.Student.FirstName} {log.Student.LastName}" : "N/A",
                    studentGradeLevel = log.Student?.GradeLevel ?? "",
                    sponsorId = log.SponsorId,
                    sponsors,
                    logStatus = log.LogStatus,
                    effectiveFrom = log.EffectiveFrom?.ToString("yyyy-MM-dd"),
                    effectiveTo = log.EffectiveTo?.ToString("yyyy-MM-dd"),
                    notes = log.Notes,
                    reviewComments = log.ReviewComments,
                    isActive = log.IsActive,
                    activatedOn = log.ActivatedOn?.ToString("yyyy-MM-dd"),
                    deactivatedOn = log.DeactivatedOn?.ToString("yyyy-MM-dd"),
                    existingAttachmentFileName = log.AttachmentFileName,
                    items,
                    categories,
                    coverageRules
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading LoG data: {ex.Message}" });
            }
        }

        // Save edited LoG from modal
        [Authorize(Roles = "admin,admissions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal([FromForm] EditLogModalViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id ?? "";

                var log = await _context.LogCoverages
                    .Include(l => l.CoverageRules)
                    .ThenInclude(r => r.Item)
                    .Include(l => l.CoverageRules)
                    .ThenInclude(r => r.Category)
                    .FirstOrDefaultAsync(l => l.LogId == model.LogId);

                if (log == null)
                {
                    return Json(new { success = false, message = "LoG not found" });
                }

                // Update basic fields
                log.SponsorId = model.SponsorId;
                log.LogStatus = model.LogStatus;
                log.EffectiveFrom = model.EffectiveFrom;
                log.EffectiveTo = model.EffectiveTo;
                log.Notes = model.Notes;
                log.ReviewComments = model.ReviewComments;
                log.ModifiedOn = DateTime.Now;

                // Handle file upload
                if (model.NewAttachment != null && model.NewAttachment.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "log_attachments");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{log.LogId}_{DateTime.Now:yyyyMMddHHmmss}_{model.NewAttachment.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.NewAttachment.CopyToAsync(stream);
                    }

                    log.AttachmentFileName = fileName;
                }

                // Update coverage rules
                if (!string.IsNullOrEmpty(model.CoverageRulesJson))
                {
                    var rulesData = System.Text.Json.JsonSerializer.Deserialize<List<CoverageRuleEditModel>>(model.CoverageRulesJson);

                    if (rulesData != null)
                    {
                        // Remove old rules
                        var existingRules = log.CoverageRules?.ToList() ?? new List<LoGCoverageRule>();
                        foreach (var existingRule in existingRules)
                        {
                            _context.LoGCoverageRules.Remove(existingRule);
                        }

                        // Add new rules
                        foreach (var ruleData in rulesData)
                        {
                            var rule = new LoGCoverageRule
                            {
                                LogId = log.LogId,
                                CoverageTarget = ruleData.CoverageTarget,
                                ItemId = ruleData.ItemId,
                                CategoryId = ruleData.CategoryId,
                                CoverageType = ruleData.CoverageType,
                                CoveragePercentage = ruleData.CoveragePercentage,
                                CoverageFixedAmount = ruleData.CoverageFixedAmount,
                                CapAmount = ruleData.CapAmount,
                                EffectiveFrom = ruleData.EffectiveFrom,
                                EffectiveTo = ruleData.EffectiveTo,
                                ExceptionNote = ruleData.ExceptionNote,
                                DisplayOrder = ruleData.DisplayOrder,
                                IsActive = true,
                                CreatedOn = DateTime.Now
                            };

                            _context.LoGCoverageRules.Add(rule);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Log the activity
                await _logsService.LogActivityAsync(
                    item: $"LoG #{log.LogId}",
                    details: $"Updated LoG for student {log.StudentId}",
                    userDisplay: user?.DisplayName ?? User.Identity?.Name ?? "System",
                    roleName: User.IsInRole("admin") ? "admin" : "admissions",
                    schoolYearId: log.SchoolYearId
                );

                return Json(new { success = true, message = "LoG updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating LoG: {ex.Message}" });
            }
        }
    }

    public class CreateLogModalViewModel
    {
        public string SchoolYearId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? Notes { get; set; }
    }

    public class EditLogModalViewModel
    {
        public int LogId { get; set; }
        public string SponsorId { get; set; } = string.Empty;
        public string LogStatus { get; set; } = string.Empty;
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? Notes { get; set; }
        public string? ReviewComments { get; set; }
        public IFormFile? NewAttachment { get; set; }
        public string? CoverageRulesJson { get; set; }
    }

    public class CoverageRuleEditModel
    {
        public int RuleId { get; set; }
        public string CoverageTarget { get; set; } = string.Empty;
        public string? ItemId { get; set; }
        public string? CategoryId { get; set; }
        public string CoverageType { get; set; } = string.Empty;
        public decimal? CoveragePercentage { get; set; }
        public decimal? CoverageFixedAmount { get; set; }
        public decimal? CapAmount { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int DisplayOrder { get; set; }
        public string? ExceptionNote { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateLogModalFullViewModel
    {
        public string SchoolYearId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string SponsorId { get; set; } = string.Empty;
        public string? LogStatus { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? Notes { get; set; }
        public string? ReviewComments { get; set; }
        public IFormFile? Attachment { get; set; }
        public string? CoverageRulesJson { get; set; }
    }
}
