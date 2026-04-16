using ISMSponsor.Models;
using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly AdminUserService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(AdminUserService adminService, UserManager<ApplicationUser> userManager)
        {
            _adminService = adminService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _adminService.GetAllAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                DisplayName = model.DisplayName,
                SponsorId = model.SponsorId,
                IsActive = true
            };
            var res = await _adminService.CreateUserAsync(user, model.Password, model.Role);
            if (res.Succeeded)
                return RedirectToAction("Index");
            foreach (var err in res.Errors) ModelState.AddModelError("", err.Description);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid value"
                );
                return Json(new { success = false, errors, message = "Validation failed" });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                DisplayName = model.DisplayName,
                SponsorId = model.SponsorId,
                IsActive = true
            };

            var result = await _adminService.CreateUserAsync(user, model.Password, model.Role);
            
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User created successfully" });
            }

            var errorMessages = result.Errors.Select(e => e.Description).ToList();
            return Json(new { success = false, message = string.Join(", ", errorMessages) });
        }

        [HttpPost]
        public async Task<IActionResult> EditInline([FromBody] UserUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.DisplayName = model.DisplayName;
            user.SponsorId = model.SponsorId;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User updated successfully" });
            }

            return Json(new { success = false, message = "Failed to update user" });
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            return Json(new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    username = user.UserName,
                    displayName = user.DisplayName,
                    sponsorId = user.SponsorId,
                    role = role,
                    isActive = user.IsActive
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditAjax([FromBody] EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid value"
                );
                return Json(new { success = false, errors, message = "Validation failed" });
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Update user properties
            user.DisplayName = model.DisplayName;
            user.Email = model.Email;
            user.SponsorId = model.SponsorId;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, message = string.Join(", ", errorMessages) });
            }

            // Update role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!passwordResult.Succeeded)
                {
                    var errorMessages = passwordResult.Errors.Select(e => e.Description).ToList();
                    return Json(new { success = false, message = "User updated but password change failed: " + string.Join(", ", errorMessages) });
                }
            }

            return Json(new { success = true, message = "User updated successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] ToggleStatusModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.IsActive = model.IsActive;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Json(new { success = true, message = $"User {(model.IsActive ? "activated" : "deactivated")} successfully" });
            }

            return Json(new { success = false, message = "Failed to update user status" });
        }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SponsorId { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Password { get; set; }
    }

    public class ToggleStatusModel
    {
        public string Id { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UserUpdateModel
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? SponsorId { get; set; }
        public bool IsActive { get; set; }
    }
}