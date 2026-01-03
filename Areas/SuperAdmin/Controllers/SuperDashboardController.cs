using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EaziLease.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    public class SuperDashboardController : Controller
    {

        [Authorize(Policy = "RequireSuperAdmin")]
        public IActionResult Index()
        {
            ViewBag.IsSuperAdmin = HttpContext.Session.GetString("IsSuperAdmin") == "true";
            return View();
        }


        [AllowAnonymous]
        public IActionResult Exit()
        {
            HttpContext.Session.Remove("IsSuperAdmin");
            TempData["info"] = "Exited Super Admin mode.";
            return RedirectToAction("Index", "Dashboard", new { Area = "" });
        }
    }
}
