using EaziLease.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EaziLease.Web.ViewModels;
using EaziLease.Domain.Entities;
using Microsoft.AspNetCore.Http;
using EaziLease.Services;

namespace EaziLease.Web.Controllers;


[Authorize(Roles = "Admin")] //Must login as admin.
public class ElevationController : Controller
{
    public readonly UserManager<ApplicationUser> _userManager;
    public readonly AuditService _auditService;

    public ElevationController(UserManager<ApplicationUser> userManager, AuditService auditService)
    {
        _userManager = userManager;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Elevate()
    {
        return View(new ElevateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Elevate(ElevateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var isCorrect = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!isCorrect)
        {
            ModelState.AddModelError("Password", "Incorrect password.");
            return View(model);
        }

        HttpContext.Session.SetString("IsSuperAdmin", "true");
        TempData["success"] = "Elevated to Super Admin mode.";
        await _auditService.LogAsync("Elevation", user.Id, "Elevate", $"User {user.Email} has been elevated to SuperAdmin.");
        return RedirectToAction("Index", "SuperDashboard", new {Area = "SuperAdmin"});
    }

    //GET: Exit elevation
    [HttpGet]
    public IActionResult Exit()
    {
        HttpContext.Session.Remove("IsSuperAdmin");
        TempData["info"] = "Exited Super Admin mode.";
        return RedirectToAction("Index", "Dashboard");
    }
}