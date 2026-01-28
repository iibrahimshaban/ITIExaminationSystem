using ExaminationSystem.Abstractions.Consts;
using ExaminationSystem.Abstractions.Interfaces;
using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExaminationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUserProvisioningService _userProvisioningService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IUserProvisioningService userProvisioningService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userProvisioningService = userProvisioningService;
        }
        #region Login

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            return View(new LoginVm
            {
                RedirectUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // 🔹 User authenticated successfully
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            // 🔹 Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // 🔹 Role-based redirection
            if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Admin");

            if (roles.Contains("InstructorRole"))
                return RedirectToAction("Index", "Instructor");

            if (roles.Contains("StudentRole"))
                return RedirectToAction("Index", "Student");

            // 🔹 Fallback (ReturnUrl)
            if (!string.IsNullOrEmpty(model.RedirectUrl) &&
                Url.IsLocalUrl(model.RedirectUrl))
            {
                return LocalRedirect(model.RedirectUrl);
            }

            // 🔹 Absolute fallback
            return RedirectToAction("Index", "Account");
        }

        #endregion


        #region Logout
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("login", "Account");
        }
        #endregion

        //register will be create by admin 

        [HttpGet]
       // [Authorize(Roles = "Admin")]
        public IActionResult Register(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var model = new RegisterVm
            {
                RedirectUrl = returnUrl,
                RoleList = _userProvisioningService.GetRoles(),
                BranchList = _userProvisioningService.GetBranches(),
                TrackList = _userProvisioningService.GetTracks(),
                CourseList = _userProvisioningService.GetCourses()
            };

            return View(model);
        }

        [HttpPost]
      //  [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid)
            {
                ReloadLookups(model);
                return View(model);
            }

            var user = new ApplicationUser
            {
                Name = model.Name,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                ReloadLookups(model);
                return View(model);
            }

            var role = string.IsNullOrWhiteSpace(model.Role)
                ? DefaultRoles.StudentRole.Name
                : model.Role;

            await _userManager.AddToRoleAsync(user, role);

            await _userProvisioningService.CreateDomainProfileAsync(user, role, model);

            return RedirectToAction("Index", "Admin");
        }

        private void ReloadLookups(RegisterVm model)
        {
            model.RoleList = _userProvisioningService.GetRoles();
            model.BranchList = _userProvisioningService.GetBranches();
            model.TrackList = _userProvisioningService.GetTracks();
            model.CourseList = _userProvisioningService.GetCourses();
        }



    }
}
