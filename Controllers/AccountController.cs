using ISMSponsor.Models;
using ISMSponsor.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ISMSponsor.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "/Dashboard/index");
            }
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Initiates Google OAuth login flow
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return Challenge(properties, "Google");
        }

        /// <summary>
        /// Handles Google OAuth callback and auto-provisions users
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
        {
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogWarning("Google login failed: External login info not found");
                    TempData["Error"] = "Unable to load external login information.";
                    return RedirectToAction(nameof(Login));
                }

                // Attempt to sign in with external login
                var result = await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: false,
                    bypassTwoFactor: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in with {Provider} provider.", info.LoginProvider);
                    return Redirect(returnUrl ?? "/Dashboard/Index");
                }

                // User doesn't exist - auto-provision for ISM Google accounts
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Google login failed: Email claim not found");
                    TempData["Error"] = "Unable to retrieve email from Google account.";
                    return RedirectToAction(nameof(Login));
                }

                // Only allow ISM email addresses (@ismanila.org)
                if (!email.EndsWith("@ismanila.org", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Google login rejected: Non-ISM email {Email}", email);
                    TempData["Error"] = "Only ISM Google accounts (@ismanila.org) are allowed.";
                    return RedirectToAction(nameof(Login));
                }

                // Auto-provision new user
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = name ?? email.Split('@')[0],
                    EmailConfirmed = true, // Google email is pre-verified
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Failed to create user {Email}: {Errors}",
                        email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    TempData["Error"] = "Failed to create user account.";
                    return RedirectToAction(nameof(Login));
                }

                // Assign default role (admin for ISM staff - can be customized)
                await _userManager.AddToRoleAsync(user, "admin");

                // Link Google login to user
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError("Failed to link Google login for {Email}", email);
                    TempData["Error"] = "Failed to link Google account.";
                    return RedirectToAction(nameof(Login));
                }

                // Sign in the newly created user
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User {Email} created and signed in with Google.", email);

                return Redirect(returnUrl ?? "/Dashboard/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login callback");
                TempData["Error"] = "An error occurred during Google login. Please try again.";
                return RedirectToAction(nameof(Login));
            }
        }
    }
}
