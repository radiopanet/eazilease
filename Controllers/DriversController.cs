using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using EaziLease.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using EaziLease.Services;

namespace EaziLease.Controllers
{
    [Authorize]
    public class DriversController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public DriversController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index() =>
            View(await _context.Drivers.Where(d => !d.IsDeleted).OrderBy(d => d.FirstName).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Driver driver)
        {
            driver.LicenseExpiry = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            if (ModelState.IsValid)
            {
                driver.CreatedBy = User.Identity!.Name ?? "admin";
                _context.Add(driver);
                await _context.SaveChangesAsync();
                TempData["success"] = "Driver added";
                await _auditService.LogAsync("Driver", driver.Id, "Create", $"Driver {driver.FullName} added by {driver.CreatedBy} at {driver.CreatedAt}.");
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null || driver.IsDeleted) return NotFound();
            return View(driver);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Driver driver)
        {
            driver.LicenseExpiry = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            if (id != driver.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Drivers.FindAsync(id);
                existing!.FirstName = driver.FirstName;
                existing!.LastName = driver.LastName;
                existing.Phone = driver.Phone;
                existing.Email = driver.Email;
                existing.IsActive = driver.IsActive;
                existing.AccidentCount = driver.AccidentCount;
                existing.LicenseExpiry = driver.LicenseExpiry;
                existing.LicenseNumber = driver.LicenseNumber;
                existing.CurrentVehicle = driver.CurrentVehicle;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = User.Identity!.Name ?? "admin";

                await _context.SaveChangesAsync();
                TempData["success"] = "Driver updated";
                await _auditService.LogAsync("Driver", existing.Id, "Edit", $"Driver {existing.FullName} updated by {existing.UpdatedBy} at {existing.UpdatedAt}");
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if(driver == null || driver.IsDeleted) return NotFound();
            return View(driver);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null && !driver.IsDeleted)
            {
                driver.IsDeleted = true;
                driver.DeletedAt = DateTime.UtcNow;
                driver.DeletedBy = User.Identity!.Name;
                await _context.SaveChangesAsync();
                TempData["success"] = "Driver deleted";
                await _auditService.LogAsync("Driver", driver.Id, "Delete", $"{driver.FullName} deleted by {driver.DeletedBy} at {driver.DeletedAt}");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}