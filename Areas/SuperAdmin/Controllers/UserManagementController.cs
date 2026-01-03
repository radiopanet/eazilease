using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EaziLease.Models;
using EaziLease.Models.ViewModels;
using EaziLease.Services;


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
        if(ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed =true
            };

            var result = await  _userManager.CreateAsync(user, model.Password);
            if(result.Succeeded)
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

    
}