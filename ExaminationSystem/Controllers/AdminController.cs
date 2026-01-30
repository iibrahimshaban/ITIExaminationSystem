using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExaminationSystem.Entities;
using ExaminationSystem.ViewModel;

namespace ExaminationSystem.Controllers;


public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    #region Delete (GET)

    [HttpGet]
    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserVm
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList()
        };

        return View(model); // Delete confirmation view
    }

    #endregion

    #region Delete (POST)

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            // Reload confirmation view
            var roles = await _userManager.GetRolesAsync(user);
            return View("Delete", new UserVm
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList()
            });
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Index (Users List)

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();

        var list = new List<UserVm>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            list.Add(new UserVm
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = roles.ToList()
            });
        }

        return View(list);
    }

    #endregion
}
