using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EaziLease.Models;
using EaziLease.Models.ViewModels;


[Area("SuperAdmin")]
[Authorize(Policy = "RequireSuperAdmin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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
    
}