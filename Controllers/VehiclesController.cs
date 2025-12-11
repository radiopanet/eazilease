using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Rendering;
using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using EaziLease.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EaziLease.Controllers
{
    [Authorize]
    public class VehiclesController: Controller
    {
        public readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //GET: Vehicles
        public async Task<IActionResult> Index()
        {
            var vehicles = _context.Vehicles
                .Include(v => v.Supplier)
                .Include(v => v.Branch)
                .Include(v => v.CurrentDriver)
                .Include(v => v.CurrentLease!.Client)
                .Where(v => !v.IsDeleted)
                .OrderBy(v => v.Manufacturer);

            return View(await vehicles.ToListAsync());    
        }

        //GET: Vehicles/Details
        public async Task<IActionResult> Details(string id)
        {
            if(id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .Include(v => v.Supplier)
                .Include(v => v.Branch)
                .Include(v => v.CurrentDriver)
                .Include(v => v.CurrentLease).ThenInclude(l => l!.Client)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);


            if(vehicle == null) return NotFound();

            return View(vehicle);    
        }

        //GET: Vehicles/Create
        public IActionResult Create()
        {
            ViewBag.Supplier = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "Id", "Name");
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => !b.IsDeleted), "Id", "Name");
            return View();
        }

        //POST: Vehicle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                vehicle.CreatedBy = User.Identity!.Name ?? "admin";
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                TempData["success"] = "Vehicle added successfully";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SupplierId = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "Id", "Name", vehicle.SupplierId);
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => !b.IsDeleted), "Id", "Name", vehicle.BranchId);

            return View(vehicle);
        }

        //GET: Vehicles/Edit/1
        public async Task<IActionResult> Edit(string id)
        {
            if(id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if(vehicle == null || vehicle.IsDeleted) return NotFound();

            ViewBag.SupplierId = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "Id", "Name", vehicle.SupplierId);
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => !b.IsDeleted), "Id", "Name", vehicle.BranchId);

            return View(vehicle);
        }

        //POST: Vehicles/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Vehicle vehicle)
        {
            if(id != vehicle.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Vehicles.FindAsync(id);
                    if(existing == null || existing.IsDeleted) return NotFound();

                    //Update only allowed fields
                    existing.VIN = vehicle.VIN;
                    existing.RegistrationNumber = vehicle.RegistrationNumber;
                    existing.Manufacturer = vehicle.Manufacturer;
                    existing.Model = vehicle.Model;
                    existing.Year = vehicle.Year;
                    existing.Color = vehicle.Color;
                    existing.FuelType = vehicle.FuelType;
                    existing.Transmission = vehicle.Transmission;
                    existing.DailyRate = vehicle.DailyRate;
                    existing.PurchasePrice = vehicle.PurchasePrice;
                    existing.PurchaseDate = vehicle.PurchaseDate;
                    existing.LastServiceDate = vehicle.LastServiceDate;
                    existing.SupplierId = vehicle.SupplierId;
                    existing.BranchId = vehicle.BranchId;
                    existing.Status = vehicle.Status;

                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = User.Identity!.Name ?? "admin";

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Vehicle updated successfully";
                } 
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes.");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.SupplierId = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "Id", "Name", vehicle.SupplierId);
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => !b.IsDeleted), "Id", "Name", vehicle.BranchId);
            return View(vehicle);
        }

        //POST: Vehicles/Delete/1 (soft delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if(vehicle != null && !vehicle.IsDeleted)
            {
                vehicle.IsDeleted = true;
                vehicle.DeletedAt = DateTime.UtcNow;
                vehicle.DeletedBy = User.Identity!.Name ?? "admin";
                await _context.SaveChangesAsync();
                TempData["success"] = "Vehicle moved to trash";

            }
            return RedirectToAction(nameof(Index));
        }
    }
}