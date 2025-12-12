using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using EaziLease.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EaziLease.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index() =>
            View(await _context.Suppliers.Where(s => !s.IsDeleted).OrderBy(s => s.Name).ToListAsync());

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
                return RedirectToAction(nameof(Index));
            }
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
            }
            return RedirectToAction(nameof(Index));
        }
    }
}