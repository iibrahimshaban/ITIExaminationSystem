using ExaminationSystem.Abstractions.Consts;
using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;
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
        public AccountController( UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        #region Login 
        public IActionResult Login(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            LoginVm loginVm = new()
            {
                RedirectUrl = returnUrl,
            };
            return View(loginVm);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Home");  //dashboard
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(model.RedirectUrl))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return LocalRedirect(model.RedirectUrl);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Login.");
                }

            }
            return View(model);
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

                // Optional: show roles ONLY if admin is creating users
                RoleList = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid)
            {
                ReloadRoles(model);
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

                ReloadRoles(model);
                return View(model);
            }

           
            var roleToAssign = string.IsNullOrEmpty(model.Role)
                ? DefaultRoles.StudentRole.Name   // default role
                : model.Role;

            await _userManager.AddToRoleAsync(user, roleToAssign);

            await _signInManager.SignInAsync(user, isPersistent: false);

            return string.IsNullOrEmpty(model.RedirectUrl)
                ? RedirectToAction("Index", "Home")
                : LocalRedirect(model.RedirectUrl);
        }

        private void ReloadRoles(RegisterVm model)
        {
            model.RoleList = _roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            });
        }

        #endregion

    }
}
