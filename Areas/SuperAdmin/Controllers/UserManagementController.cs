using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EaziLease.Models;
using EaziLease.Models.ViewModels;
using EaziLease.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


[Area("SuperAdmin")]
[Authorize(Policy = "RequireSuperAdmin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AuditService _auditService;

    public UserManagementController(UserManager<ApplicationUser> userManager,
     RoleManager<IdentityRole> roleManager, AuditService auditService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _auditService = auditService;
    }

    //GET list of users
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var userList = new List<UserVeiwModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userList.Add(new UserVeiwModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles,
                CanElevate = roles.Contains("SuperAdminCandidate")
            });
        }

        return View(userList);
    }

    //GET create user
    public IActionResult Create()
    {
        return View();
    }


    //POST create user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");

                TempData["success"] = $"User {model.Email} created successfully.";

                await _auditService.LogAsync("ApplicationUser", user.Id, "Create",
                $"User {model.Email} created successfully.");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
        return View(model);
    }

    // POST: Toggle CanElevate (SuperAdminCandidate role)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleElevate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var hasElevate = await _userManager.IsInRoleAsync(user, "SuperAdminCandidate");

        if (hasElevate)
        {
            await _userManager.RemoveFromRoleAsync(user, "SuperAdminCandidate");
            await _auditService.LogAsync("ApplicationUser", user.Id, "ToggleElevate", $"User {user.FullName} was removed from super admin role");
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "SuperAdminCandidate");
            await _auditService.LogAsync("ApplicationUser", user.Id, "ToggleElevate", $"User {user.FullName} was added to super admin role");
        }
        TempData["success"] = $"Elevation permission {(hasElevate ? "revoked" : "granted")} for {user.Email}.";
        return RedirectToAction(nameof(Index));
    }


    // POST: Delete user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["success"] = $"User {user.Email} deleted.";
            await _auditService.LogAsync("ApplicationUser", user.Id, "Delete", $"User {user.FullName} was successfully deleted");
        }
        else
        {
            TempData["error"] = "Failed to delete user.";
            await _auditService.LogAsync("ApplicationUser", user.Id, "Delete", $"Unable to delete User {user.FullName}");
        }

        return RedirectToAction("Index", "SuperDashboard", new { Area = "SuperAdmin" });
    }
}