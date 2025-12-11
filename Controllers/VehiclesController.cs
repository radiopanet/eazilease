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
    }
}