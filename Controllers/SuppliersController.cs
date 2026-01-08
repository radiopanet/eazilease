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
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public SuppliersController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index() =>
            View(await _context.Suppliers.Include(v => v.Vehicles).Where(s => !s.IsDeleted).OrderBy(s => s.Name).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.CreatedBy = User.Identity!.Name ?? "admin";
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                TempData["success"] = "Supplier added";
                await _auditService.LogAsync("Supplier", supplier.Id, "Create",
                    $"Supplier {supplier.Name} added by {supplier.CreatedBy} at {supplier.CreatedAt}.");
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null || supplier.IsDeleted) return NotFound();
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Supplier supplier)
        {
            if (id != supplier.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Suppliers.FindAsync(id);
                existing!.Name = supplier.Name;
                existing.ContactPerson = supplier.ContactPerson;
                existing.ContactEmail = supplier.ContactEmail;
                existing.ContactPhone = supplier.ContactPhone;
                existing.Address = supplier.Address;
                existing.City = supplier.City;
                existing.Country = supplier.Country;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = User.Identity!.Name ?? "admin";

                await _context.SaveChangesAsync();
                TempData["success"] = "Supplier updated";
                await _auditService.LogAsync("Supplier", supplier.Id, "Edit", 
                    $"Supplier {existing.Name} updated by {existing.UpdatedBy} at {existing.UpdatedAt}.");
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        public async Task<IActionResult> Details(string id)
        {
            if(id == null) return NotFound();
            
            var supplier = await _context.Suppliers
                    .Include(v => v.Vehicles)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if(supplier == null) return NotFound();  

            return View(supplier);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if(supplier == null || supplier.IsDeleted) return NotFound();
            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null && !supplier.IsDeleted)
            {
                supplier.IsDeleted = true;
                supplier.DeletedAt = DateTime.UtcNow;
                supplier.DeletedBy = User.Identity!.Name;
                await _context.SaveChangesAsync();
                TempData["success"] = "Supplier deleted";
                await _auditService.LogAsync("Supplier", supplier.Id, "Delete",
                    $"{supplier.Name} deleted by {supplier.DeletedBy} at {supplier.DeletedAt}");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}