using ISMSponsor.Models;
using ISMSponsor.Services;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ISMSponsor.Controllers.Settings
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly AdminUserService _adminService;

        public UsersController(AdminUserService adminService)
        {
            _adminService = adminService;
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
    }
}