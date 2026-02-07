using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EaziLease.Infrastructure.Persistence;
using EaziLease.Domain.Entitiess.ViewModels;
using EaziLease.Domain.Entitiess;
using Microsoft.EntityFrameworkCore;

namespace EaziLease.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    public class SuperDashboardController : Controller
    {
        public readonly ApplicationDbContext _context;

        public SuperDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "RequireSuperAdmin")]
        public async Task<IActionResult> Index()
        {
            ViewBag.IsSuperAdmin = HttpContext.Session.GetString("IsSuperAdmin") == "true";
            // NEW: Upcoming Maintenance

            //Fetch raw data first
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsDeleted)
                .Where(v => v.NextMaintenanceDate != null || v.NextMaintenanceMileage != null)
                .OrderBy(v => v.NextMaintenanceDate)
                .ThenBy(v => v.NextMaintenanceMileage)
                .Take(10)
                .ToListAsync();

            //Project data into the view model
            var upcoming = vehicles
                .Select(v => new UpcomingMaintenanceViewModel 
                {
                     VehicleId = v.Id,
                     RegistrationNumber = v.RegistrationNumber,
                     NextMaintenanceDate = v.NextMaintenanceDate,
                     NextMaintenanceMileage = v.NextMaintenanceMileage,
                     DaysRemaining = v.NextMaintenanceDate != null ? (v.NextMaintenanceDate.Value.Date - DateTime.Today).Days : 999,
                     KmRemaining = v.NextMaintenanceMileage != null ? v.NextMaintenanceMileage - (v.OdometerReading ?? 0) : 9999,
                     Type = v.MaintenanceHistory .Where(m => m.Status == MaintenanceStatus.Scheduled)
                      .OrderBy(m => m.ScheduledDate) 
                      .Select(m => m.Type) .FirstOrDefault() }) 
                      .OrderBy(vm => vm.DaysRemaining) 
                      .ThenBy(vm => vm.KmRemaining) 
                      .Take(10) 
                      .ToList();
                      
            ViewBag.UpcomingMaintenance = upcoming;
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
