using Microsoft.AspNetCore.Mvc;

namespace EaziLease.Web.Controllers
{
    public class HomeController: Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}