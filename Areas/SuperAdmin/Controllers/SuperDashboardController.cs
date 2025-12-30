using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EaziLease.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Policy = "RequireSuperAdmin")]
    public class SuperDashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.IsSuperAdmin = HttpContext.Session.GetString("IsSuperAdmin") == "true";
            return View();
        }

        public IActionResult Exit()
        {
            HttpContext.Session.Remove("IsSuperAdmin");
            TempData["info"] = "Exited Super Admin mode.";
            return RedirectToAction("Index", "Dashboard", new { area = "" });
        }
    }
}
