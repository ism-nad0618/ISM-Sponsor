using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ISMSponsor.Data;
using System.Linq;
using ISMSponsor.Integration.Orchestration.Services;
using ISMSponsor.Integration.Orchestration;
using Microsoft.EntityFrameworkCore;

namespace ISMSponsor.Controllers
{
    [Authorize]
    public class SponsorsController : Controller
    {
        private readonly SponsorService _sponsorService;
        private readonly ChangeRequestService _changeService;
        private readonly AdminUserService _adminUserService;
        private readonly LogsService _logsService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ISMSponsor.Models.ApplicationUser> _userManager;
        private readonly Microsoft.Extensions.Logging.ILogger<SponsorsController> _logger;
        private readonly AppDbContext _context;
        private readonly IPowerSchoolSponsorListService _powerSchoolListService;
        private readonly IIntegrationOrchestrator _integrationOrchestrator;

        public SponsorsController(
            SponsorService sponsorService, 
            ChangeRequestService changeService,
            AdminUserService adminUserService,
            LogsService logsService,
            Microsoft.AspNetCore.Identity.UserManager<ISMSponsor.Models.ApplicationUser> userManager, 
            Microsoft.Extensions.Logging.ILogger<SponsorsController> logger,
            AppDbContext context,
            IPowerSchoolSponsorListService powerSchoolListService,
            IIntegrationOrchestrator integrationOrchestrator)
        {
            _sponsorService = sponsorService;
            _changeService = changeService;
            _adminUserService = adminUserService;
            _logsService = logsService;
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _powerSchoolListService = powerSchoolListService;
            _integrationOrchestrator = integrationOrchestrator;
        }

        public async Task<IActionResult> Profile(string? id = null)
        {
            // administrators and cashiers may view arbitrary sponsor by id
            if (User.IsInRole("admin") || User.IsInRole("cashier"))
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

        [Authorize(Roles = "admin,cashier")]
        public async Task<IActionResult> Index()
        {
            var list = await _sponsorService.GetAllAsync();
            return View(list);
        }

        [Authorize(Roles = "admin,cashier")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateSponsorViewModel());
        }

        [Authorize(Roles = "admin,cashier")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateSponsorViewModel model)
        {
            var result = await ValidateAndCreateSponsorAsync(model);
            
            if (!result.Success)
            {
                if (result.ValidationErrors != null)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        ModelState.AddModelError(error.Key, error.Value.First());
                    }
                }
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "admin,cashier,admissions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(CreateSponsorViewModel model)
        {
            var result = await ValidateAndCreateSponsorAsync(model);
            
            if (!result.Success)
            {
                return Json(new { success = false, errors = result.ValidationErrors, message = result.Message });
            }

            return Json(new { success = true, message = result.Message });
        }

        private async Task<SponsorCreationResult> ValidateAndCreateSponsorAsync(CreateSponsorViewModel model)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return new SponsorCreationResult { Success = false, ValidationErrors = errors };
            }

            // Check if sponsor ID already exists
            if (await _sponsorService.SponsorExistsAsync(model.SponsorId))
            {
                return new SponsorCreationResult 
                { 
                    Success = false, 
                    ValidationErrors = new Dictionary<string, string[]> 
                    { 
                        { "SponsorId", new[] { "A sponsor with this ID already exists." } } 
                    } 
                };
            }

            // Check if username already exists
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                return new SponsorCreationResult 
                { 
                    Success = false, 
                    ValidationErrors = new Dictionary<string, string[]> 
                    { 
                        { "Username", new[] { "This username is already taken." } } 
                    } 
                };
            }

            // Validate file upload
            if (model.VerificationDocument == null || model.VerificationDocument.Length == 0)
            {
                return new SponsorCreationResult 
                { 
                    Success = false, 
                    ValidationErrors = new Dictionary<string, string[]> 
                    { 
                        { "VerificationDocument", new[] { "Verification document is required." } } 
                    } 
                };
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(model.VerificationDocument.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return new SponsorCreationResult 
                { 
                    Success = false, 
                    ValidationErrors = new Dictionary<string, string[]> 
                    { 
                        { "VerificationDocument", new[] { "Invalid file type. Allowed: PDF, JPG, PNG, DOC, DOCX" } } 
                    } 
                };
            }

            if (model.VerificationDocument.Length > 10 * 1024 * 1024) // 10MB limit
            {
                return new SponsorCreationResult 
                { 
                    Success = false, 
                    ValidationErrors = new Dictionary<string, string[]> 
                    { 
                        { "VerificationDocument", new[] { "File size must not exceed 10MB." } } 
                    } 
                };
            }

            try
            {
                // Create the sponsor
                var sponsor = new ISMSponsor.Models.Domain.Sponsor
                {
                    SponsorId = model.SponsorId,
                    SponsorName = model.SponsorName,
                    LegalName = model.LegalName,
                    Address = model.Address,
                    Tin = model.Tin,
                    ApprovalStatus = "PendingApproval" // New sponsors require approval
                };
                await _sponsorService.CreateAsync(sponsor);

                // Queue downstream integrations to ALL targets (non-blocking)
                // 1. PowerSchool Sponsor_OrgName list
                // 2. NetSuite Sponsors List
                // 3. OBS CompanySponsors + CompanySponsorAccount
                // 4. SCP Sponsors table
                await _integrationOrchestrator.QueueSponsorSyncAsync(
                    model.SponsorId, 
                    IntegrationEventType.SponsorCreate, 
                    model.Username);

                // Save verification document
                var fileName = await _sponsorService.SaveVerificationDocumentAsync(model.SponsorId, model.VerificationDocument);

                // Create the user account
                var user = new ISMSponsor.Models.ApplicationUser
                {
                    UserName = model.Username,
                    DisplayName = model.DisplayName,
                    SponsorId = model.SponsorId,
                    IsActive = true
                };
                var userResult = await _adminUserService.CreateUserAsync(user, model.Password, "sponsor");

                if (!userResult.Succeeded)
                {
                    var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user for sponsor {SponsorId}: {Errors}", model.SponsorId, errors);
                    return new SponsorCreationResult 
                    { 
                        Success = false, 
                        Message = $"User account creation failed: {errors}"
                    };
                }

                // Log the activity
                var currentUser = await _userManager.GetUserAsync(User);
                await _logsService.LogActivityAsync(
                    item: "New Sponsor",
                    details: $"Created sponsor '{model.SponsorName}' (ID: {model.SponsorId}) with user account '{model.Username}'. Verification document: {fileName}",
                    userDisplay: currentUser?.DisplayName ?? User.Identity?.Name ?? "System",
                    roleName: User.IsInRole("admin") ? "admin" : "cashier",
                    schoolYearId: ""
                );

                return new SponsorCreationResult 
                { 
                    Success = true, 
                    Message = $"Sponsor '{model.SponsorName}' and user account '{model.Username}' created successfully." 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sponsor {SponsorId}", model.SponsorId);
                return new SponsorCreationResult 
                { 
                    Success = false, 
                    Message = "An error occurred while creating the sponsor. Please try again."
                };
            }
        }

        [Authorize(Roles = "admin,cashier")]
        [HttpGet]
        public async Task<IActionResult> GetProfileData(string id)
        {
            var sponsor = await _sponsorService.GetByIdAsync(id);
            if (sponsor == null)
            {
                return Json(new { success = false, message = "Sponsor not found" });
            }

            return Json(new
            {
                sponsorId = sponsor.SponsorId,
                sponsorName = sponsor.SponsorName,
                legalName = sponsor.LegalName,
                address = sponsor.Address,
                tin = sponsor.Tin,
                approvalStatus = sponsor.ApprovalStatus,
                approvedOn = sponsor.ApprovedOn,
                approvedByUserId = sponsor.ApprovedByUserId,
                approvalNotes = sponsor.ApprovalNotes
            });
        }

        [Authorize(Roles = "admin,cashier")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateSponsorProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                // Get the sponsor
                var sponsor = await _sponsorService.GetByIdAsync(model.SponsorId);
                if (sponsor == null)
                {
                    return Json(new { success = false, message = "Sponsor not found" });
                }

                // Update sponsor information
                sponsor.SponsorName = model.SponsorName;
                sponsor.LegalName = model.LegalName;
                sponsor.Address = model.Address;
                sponsor.Tin = model.Tin;
                await _sponsorService.UpdateAsync(sponsor);

                // Queue downstream integrations to ALL targets (non-blocking)
                await _integrationOrchestrator.QueueSponsorSyncAsync(
                    model.SponsorId, 
                    IntegrationEventType.SponsorUpdate);

                // Update password if provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    // Validate password confirmation
                    if (model.Password != model.ConfirmPassword)
                    {
                        return Json(new { success = false, message = "Passwords do not match" });
                    }

                    // Find user by sponsor ID
                    var users = _context.Users.Where(u => u.SponsorId == model.SponsorId).ToList();
                    if (users.Any())
                    {
                        var user = users.First();
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                        
                        if (!passwordResult.Succeeded)
                        {
                            var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                            return Json(new { success = false, message = $"Sponsor updated but password change failed: {errors}" });
                        }
                    }
                }

                // Handle verification document upload
                if (model.VerificationDocument != null && model.VerificationDocument.Length > 0)
                {
                    // Validate file
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                    var fileExtension = Path.GetExtension(model.VerificationDocument.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Json(new { success = false, message = "Invalid file type. Allowed: PDF, JPG, PNG, DOC, DOCX" });
                    }

                    if (model.VerificationDocument.Length > 10 * 1024 * 1024)
                    {
                        return Json(new { success = false, message = "File size must not exceed 10MB" });
                    }

                    // Save the new document
                    await _sponsorService.SaveVerificationDocumentAsync(model.SponsorId, model.VerificationDocument);
                }

                // Log the activity
                var currentUser = await _userManager.GetUserAsync(User);
                await _logsService.LogActivityAsync(
                    item: "Sponsor Update",
                    details: $"Updated sponsor '{model.SponsorName}' (ID: {model.SponsorId})",
                    userDisplay: currentUser?.DisplayName ?? User.Identity?.Name ?? "System",
                    roleName: User.IsInRole("admin") ? "admin" : "cashier",
                    schoolYearId: ""
                );

                return Json(new { success = true, message = "Sponsor profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sponsor profile {SponsorId}", model.SponsorId);
                return Json(new { success = false, message = "An error occurred while updating the profile" });
            }
        }

        [Authorize(Roles = "admin,cashier")]
        [HttpPost]
        public async Task<IActionResult> EditProfile([FromBody] SponsorUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var sponsor = await _sponsorService.GetByIdAsync(model.SponsorId);
            if (sponsor == null)
            {
                return Json(new { success = false, message = "Sponsor not found" });
            }

            try
            {
                sponsor.SponsorName = model.SponsorName;
                sponsor.LegalName = model.LegalName;
                sponsor.Address = model.Address;
                sponsor.Tin = model.Tin;

                await _sponsorService.UpdateAsync(sponsor);

                // Queue downstream integrations to ALL targets (non-blocking)
                await _integrationOrchestrator.QueueSponsorSyncAsync(
                    model.SponsorId, 
                    IntegrationEventType.SponsorUpdate);

                // Log the activity
                var currentUser = await _userManager.GetUserAsync(User);
                await _logsService.LogActivityAsync(
                    item: "Sponsor Update",
                    details: $"Updated sponsor '{model.SponsorName}' (ID: {model.SponsorId})",
                    userDisplay: currentUser?.DisplayName ?? User.Identity?.Name ?? "System",
                    roleName: User.IsInRole("admin") ? "admin" : "cashier",
                    schoolYearId: ""
                );

                _logger.LogInformation("Sponsor {SponsorId} updated successfully by {User}", model.SponsorId, User.Identity?.Name);

                return Json(new { success = true, message = "Sponsor profile updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sponsor {SponsorId}", model.SponsorId);
                return Json(new { success = false, message = "Failed to update sponsor profile" });
            }
        }
        
        [Authorize(Roles = "admin,cashier,admissions")]
        [HttpGet]
        public async Task<IActionResult> GetContacts(string sponsorId)
        {
            try
            {
                var contacts = await _sponsorService.GetContactsAsync(sponsorId);
                return Json(contacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts for sponsor {SponsorId}", sponsorId);
                return Json(new List<object>());
            }
        }
        
        [Authorize(Roles = "admin,cashier,admissions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContact(string sponsorId, string name, string email, string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sponsorId) || string.IsNullOrWhiteSpace(name) || 
                    string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
                {
                    _logger.LogWarning("AddContact called with missing fields for sponsor {SponsorId}", sponsorId);
                    return Json(new { success = false, message = "All fields are required" });
                }
                
                var contact = new ISMSponsor.Models.Domain.SponsorContact
                {
                    SponsorId = sponsorId,
                    Name = name,
                    Email = email,
                    Phone = phone,
                    IsActive = true
                };
                
                await _sponsorService.AddContactAsync(contact);
                _logger.LogInformation("Contact {Name} added for sponsor {SponsorId} by {User}", 
                    name, sponsorId, User.Identity?.Name);
                
                return Json(new { success = true, message = "Contact added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding contact for sponsor {SponsorId}", sponsorId);
                return Json(new { success = false, message = "Failed to add contact" });
            }
        }
        
        [Authorize(Roles = "admin,cashier,admissions")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(int contactId, string sponsorId, string name, string email, string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
                {
                    _logger.LogWarning("UpdateContact called with missing fields for contact {ContactId}", contactId);
                    return Json(new { success = false, message = "All fields are required" });
                }
                
                var success = await _sponsorService.UpdateContactAsync(contactId, name, email, phone);
                if (!success)
                {
                    return Json(new { success = false, message = "Contact not found" });
                }
                
                _logger.LogInformation("Contact {Id} updated by {User}", contactId, User.Identity?.Name);
                return Json(new { success = true, message = "Contact updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {Id}", contactId);
                return Json(new { success = false, message = "Failed to update contact" });
            }
        }
        
        [Authorize(Roles = "admin,cashier,admissions")]
        [HttpPost]
        public async Task<IActionResult> DeactivateContact(int id)
        {
            try
            {
                var success = await _sponsorService.SetContactStatusAsync(id, false);
                if (!success)
                {
                    return Json(new { success = false, message = "Contact not found" });
                }
                
                _logger.LogInformation("Contact {Id} deactivated by {User}", id, User.Identity?.Name);
                return Json(new { success = true, message = "Contact deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating contact {Id}", id);
                return Json(new { success = false, message = "Failed to deactivate contact" });
            }
        }
        
        [Authorize(Roles = "admin,cashier,admissions")]
        [HttpPost]
        public async Task<IActionResult> ReactivateContact(int id)
        {
            try
            {
                var success = await _sponsorService.SetContactStatusAsync(id, true);
                if (!success)
                {
                    return Json(new { success = false, message = "Contact not found" });
                }
                
                _logger.LogInformation("Contact {Id} reactivated by {User}", id, User.Identity?.Name);
                return Json(new { success = true, message = "Contact reactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating contact {Id}", id);
                return Json(new { success = false, message = "Failed to reactivate contact" });
            }
        }

        [Authorize(Roles = "admin,cashier")]
        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] SponsorToggleStatusModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.SponsorId))
                {
                    return Json(new { success = false, message = "Invalid sponsor request" });
                }

                var sponsorName = await _context.Sponsors
                    .AsNoTracking()
                    .Where(s => s.SponsorId == model.SponsorId)
                    .Select(s => s.SponsorName)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(sponsorName))
                {
                    return Json(new { success = false, message = "Sponsor not found" });
                }

                var rowsUpdated = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE Sponsors
                    SET IsActive = {model.IsActive},
                        ModifiedOn = {DateTime.UtcNow},
                        ModifiedByUserId = {_userManager.GetUserId(User)}
                    WHERE SponsorId = {model.SponsorId}");

                if (rowsUpdated <= 0)
                {
                    return Json(new { success = false, message = "Sponsor not found" });
                }

                // Queue downstream integrations (best effort; do not fail sponsor status update)
                try
                {
                    await _integrationOrchestrator.QueueSponsorSyncAsync(
                        model.SponsorId,
                        model.IsActive ? IntegrationEventType.SponsorActivate : IntegrationEventType.SponsorDeactivate);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Sponsor status updated but failed to queue downstream sync for {SponsorId}",
                        model.SponsorId);
                }

                // Log the activity (best effort; do not fail sponsor status update)
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    await _logsService.LogActivityAsync(
                        item: "Sponsor Status Change",
                        details: $"Sponsor '{sponsorName}' (ID: {model.SponsorId}) {(model.IsActive ? "activated" : "deactivated")}",
                        userDisplay: currentUser?.DisplayName ?? User.Identity?.Name ?? "System",
                        roleName: User.IsInRole("admin") ? "admin" : "cashier",
                        schoolYearId: ""
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Sponsor status updated but failed to write activity log for {SponsorId}",
                        model.SponsorId);
                }

                _logger.LogInformation("Sponsor {SponsorId} {Action} by {User}", 
                    model.SponsorId, model.IsActive ? "activated" : "deactivated", User.Identity?.Name);

                return Json(new { success = true, message = $"Sponsor {(model.IsActive ? "activated" : "deactivated")} successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling sponsor status {SponsorId}", model.SponsorId);
                var env = HttpContext.RequestServices.GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
                if (env?.IsDevelopment() == true)
                {
                    return Json(new { success = false, message = $"Failed to update sponsor status: {ex.Message}" });
                }
                return Json(new { success = false, message = "Failed to update sponsor status" });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveSponsor(string sponsorId, string? approvalNotes = null)
        {
            try
            {
                var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
                if (sponsor == null)
                {
                    TempData["Error"] = "Sponsor not found";
                    return RedirectToAction("Index");
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? "system";
                var userDisplay = currentUser?.DisplayName ?? User.Identity?.Name ?? "System";

                sponsor.ApprovalStatus = "Approved";
                sponsor.ApprovedOn = DateTime.UtcNow;
                sponsor.ApprovedByUserId = userId;
                sponsor.ApprovalNotes = approvalNotes;

                await _sponsorService.UpdateAsync(sponsor);

                // Queue downstream integrations (newly approved sponsor now available)
                await _integrationOrchestrator.QueueSponsorSyncAsync(
                    sponsorId, 
                    IntegrationEventType.SponsorApprove);

                // Log the activity
                await _logsService.LogActivityAsync(
                    item: "Sponsor Approval",
                    details: $"Approved sponsor '{sponsor.SponsorName}' (ID: {sponsorId}). Notes: {approvalNotes ?? "None"}",
                    userDisplay: userDisplay,
                    roleName: "admin",
                    schoolYearId: ""
                );

                TempData["Success"] = $"Sponsor '{sponsor.SponsorName}' has been approved";
                return RedirectToAction("Profile", new { id = sponsorId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving sponsor {SponsorId}", sponsorId);
                TempData["Error"] = "An error occurred while approving the sponsor";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectSponsor(string sponsorId, string rejectionReason)
        {
            try
            {
                var sponsor = await _sponsorService.GetByIdAsync(sponsorId);
                if (sponsor == null)
                {
                    TempData["Error"] = "Sponsor not found";
                    return RedirectToAction("Index");
                }

                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Id ?? "system";
                var userDisplay = currentUser?.DisplayName ?? User.Identity?.Name ?? "System";

                sponsor.ApprovalStatus = "Rejected";
                sponsor.ApprovedOn = DateTime.UtcNow;
                sponsor.ApprovedByUserId = userId;
                sponsor.ApprovalNotes = rejectionReason;

                await _sponsorService.UpdateAsync(sponsor);
                // Queue PowerSchool Sponsor_OrgName list refresh (rejected sponsor should not appear)
                await _powerSchoolListService.QueueSponsorOrgListRefreshAsync("SponsorReject", sponsorId);
                // Log the activity
                await _logsService.LogActivityAsync(
                    item: "Sponsor Rejection",
                    details: $"Rejected sponsor '{sponsor.SponsorName}' (ID: {sponsorId}). Reason: {rejectionReason}",
                    userDisplay: userDisplay,
                    roleName: "admin",
                    schoolYearId: ""
                );

                TempData["Success"] = $"Sponsor '{sponsor.SponsorName}' has been rejected";
                return RedirectToAction("Profile", new { id = sponsorId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting sponsor {SponsorId}", sponsorId);
                TempData["Error"] = "An error occurred while rejecting the sponsor";
                return RedirectToAction("Index");
            }
        }
    }

    public class SponsorCreationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }

    public class SponsorToggleStatusModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SponsorUpdateModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Tin { get; set; }
    }

    public class UpdateSponsorProfileViewModel
    {
        public string SponsorId { get; set; } = string.Empty;
        public string SponsorName { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Tin { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public IFormFile? VerificationDocument { get; set; }
    }
}
