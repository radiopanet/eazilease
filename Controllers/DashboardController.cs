using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EaziLease.Data;
using EaziLease.Models;
using EaziLease.Models.ViewModels;
namespace EaziLease.Controllers
{
    // [Authorize(Roles ="Admin", Policy ="RequireSuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles
                .Where(v => !v.IsDeleted)
                .Include(v => v.Supplier)
                .Include(v => v.Branch)
                .Include(v => v.CurrentLease).ThenInclude(l => l!.Client)
                .ToListAsync();

            // Summary Cards
            ViewBag.TotalVehicles = vehicles.Count;
            ViewBag.Available = vehicles.Count(v => v.Status == VehicleStatus.Available);
            ViewBag.Leased = vehicles.Count(v => v.Status == VehicleStatus.Leased);
            ViewBag.InMaintenance = vehicles.Count(v => v.Status == VehicleStatus.InMaintenance);

            // Manufacturer Report – Grouped by Manufacturer
            var report = vehicles
                .GroupBy(v => v.Manufacturer)
                .Select(g => new ManufacturerReportViewModel
                {
                    Manufacturer = g.Key,
                    Total = g.Count(),
                    BySupplier = g.GroupBy(v => v.Supplier?.Name ?? "Unknown")
                                  .Select(s => new Breakdown { Name = s.Key, Count = s.Count() })
                                  .OrderByDescending(x => x.Count).ToList(),
                    ByBranch = g.GroupBy(v => v.Branch?.Name ?? "Unallocated")
                                .Select(b => new Breakdown { Name = b.Key, Count = b.Count() })
                                .OrderByDescending(x => x.Count).ToList(),
                    ByClient = g.GroupBy(v => v.CurrentLease?.Client?.CompanyName ?? "Not Leased")
                                .Select(c => new Breakdown { Name = c.Key, Count = c.Count() })
                                .OrderByDescending(x => x.Count).ToList()
                })
                .OrderByDescending(r => r.Total)
                .ToList();

            ViewBag.GrandTotal = report.Sum(r => r.Total);

            return View(report);
        }
    }
}