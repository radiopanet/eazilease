using EaziLease.Data;
using EaziLease.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EaziLease.Services;

namespace EaziLease.Controllers
{
    [Authorize]
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public BranchesController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index() =>
            View(await _context.Branches.Where(b => !b.IsDeleted).OrderBy(b => b.Name).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch branch)
        {
            if (ModelState.IsValid)
            {
                branch.CreatedBy = User.Identity!.Name ?? "admin";
                _context.Add(branch);
                await _context.SaveChangesAsync();
                TempData["success"] = "Branch added";
                await _auditService.LogAsync("Branches", branch.Id, "Create", $"Branch {branch.Name} added successfully");
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null || branch.IsDeleted) return NotFound();
            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Branch branch)
        {
            if (id != branch.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existing = await _context.Branches.FindAsync(id);
                existing!.Name = branch.Name;
                existing.Code = branch.Code;
                existing.Address = branch.Address;
                existing.City = branch.City;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = User.Identity!.Name ?? "admin";

                await _context.SaveChangesAsync();
                TempData["success"] = "Branch updated";
                await _auditService.LogAsync("Branches", existing.Id, "Edit",
                     $"Branch {existing.Name} updated by {existing.UpdatedBy} at {existing.UpdatedAt}.");
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null && !branch.IsDeleted)
            {
                branch.IsDeleted = true;
                branch.DeletedAt = DateTime.UtcNow;
                branch.DeletedBy = User.Identity!.Name;
                await _context.SaveChangesAsync();
                TempData["success"] = "Branch deleted";
                await _auditService.LogAsync("Braches", branch.Id, "Delete",
                     $"Branch {branch.Name} deleted by {branch.DeletedBy} at {branch.DeletedAt}");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}