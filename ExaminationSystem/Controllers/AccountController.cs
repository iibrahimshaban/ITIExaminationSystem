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

        #region Register

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var model = new RegisterVm
            {
                RedirectUrl = returnUrl,
                RoleList = _userProvisioningService.GetRoles(),
                BranchList = _userProvisioningService.GetBranches(),

                // Initialize nested models to prevent null reference issues
                StudentDetails = new StudentDetailsVm
                {
                    TrackList = _userProvisioningService.GetTracks()
                },
                InstructorDetails = new InstructorDetailsVm()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            // Custom validation for role-specific fields
            ValidateRoleSpecificFields(model);

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

            try
            {
                await _userProvisioningService.CreateDomainProfileAsync(user, role, model);
            }
            catch (InvalidOperationException ex)
            {
                // Rollback user creation if domain profile fails
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, ex.Message);
                ReloadLookups(model);
                return View(model);
            }

            TempData["SuccessMessage"] = $"User {model.Name} created successfully as {role}.";
            return RedirectToAction("Index", "Admin");
        }

        private void ValidateRoleSpecificFields(RegisterVm model)
        {
            if (model.Role == DefaultRoles.StudentRole.Name)
            {
                if (model.StudentDetails == null)
                {
                    ModelState.AddModelError(string.Empty, "Student details are required.");
                    return;
                }

                if (!model.StudentDetails.BranchId.HasValue)
                    ModelState.AddModelError("StudentDetails.BranchId", "Branch is required for students.");

                if (!model.StudentDetails.TrackId.HasValue)
                    ModelState.AddModelError("StudentDetails.TrackId", "Track is required for students.");
            }
            else if (model.Role == DefaultRoles.InstructorRole.Name)
            {
                if (model.InstructorDetails == null)
                {
                    ModelState.AddModelError(string.Empty, "Instructor details are required.");
                    return;
                }

                if (!model.InstructorDetails.BranchId.HasValue)
                    ModelState.AddModelError("InstructorDetails.BranchId", "Branch is required for instructors.");

                if (model.InstructorDetails.SelectedCourseIds == null || !model.InstructorDetails.SelectedCourseIds.Any())
                    ModelState.AddModelError("InstructorDetails.SelectedCourseIds", "At least one course must be selected for instructors.");
            }
        }

        private void ReloadLookups(RegisterVm model)
        {
            model.RoleList = _userProvisioningService.GetRoles();
            model.BranchList = _userProvisioningService.GetBranches();

            // Preserve nested model references
            if (model.StudentDetails != null)
            {
                model.StudentDetails.TrackList = _userProvisioningService.GetTracks();
            }
            else
            {
                model.StudentDetails = new StudentDetailsVm
                {
                    TrackList = _userProvisioningService.GetTracks()
                };
            }

            if (model.InstructorDetails == null)
            {
                model.InstructorDetails = new InstructorDetailsVm();
            }
        }

        #endregion


    }
}
