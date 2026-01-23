using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EaziLease.Areas.Client.Controllers
{
    [Area("ClientPortal")]
    [Authorize("ClientUser")]
    public class HomeController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}