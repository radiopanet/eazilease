using Microsoft.AspNetCore.Mvc;

namespace EaziLease.Controllers
{
    public class HomeController: Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}