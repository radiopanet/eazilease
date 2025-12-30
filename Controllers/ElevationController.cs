using EaziLease.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EaziLease.Models.ViewModels;
using EaziLease.Models;
using Microsoft.AspNetCore.Http;

namespace EaziLease.Controllers;


[Authorize(Roles = "Admin")] //Must login as admin.
public class ElevationController : Controller
{
    public readonly UserManager<ApplicationUser> _userManager;

    public ElevationController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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