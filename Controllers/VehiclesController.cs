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
    public class VehiclesController : Controller
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
                .AsNoTracking()
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
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .Include(v => v.Supplier)
                .Include(v => v.Branch)
                .Include(v => v.CurrentDriver)
                .Include(v => v.CurrentLease).ThenInclude(l => l!.Client)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);


            if (vehicle == null) return NotFound();

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
            if (id == null) return NotFound();

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null || vehicle.IsDeleted) return NotFound();

            ViewBag.SupplierId = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "Id", "Name", vehicle.SupplierId);
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => !b.IsDeleted), "Id", "Name", vehicle.BranchId);

            return View(vehicle);
        }

        //POST: Vehicles/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Vehicle vehicle)
        {
            if (id != vehicle.Id) return NotFound();
           
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Vehicles.FindAsync(id);
                    if (existing == null || existing.IsDeleted) return NotFound();

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
            if (vehicle != null && !vehicle.IsDeleted)
            {
                vehicle.IsDeleted = true;
                vehicle.DeletedAt = DateTime.UtcNow;
                vehicle.DeletedBy = User.Identity!.Name ?? "admin";
                await _context.SaveChangesAsync();
                TempData["success"] = "Vehicle moved to trash";

            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Vehicle/Lease/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Lease(string id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.CurrentLease)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

            if (vehicle == null) return NotFound();

            if (vehicle.Status == VehicleStatus.Leased)
                TempData["error"] = "Vehicle already leased!";

            ViewBag.ClientId = new SelectList(
                _context.Clients.Where(c => !c.IsDeleted),
                "Id", "CompanyName"
            );

            return View(new VehicleLease
            {
                VehicleId = id,
                Vehicle = vehicle,
                LeaseStartDate = DateTime.UtcNow.Date,
                LeaseEndDate = DateTime.UtcNow.Date.AddMonths(1)
            });
        }

        // POST: Vehicle/Lease/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Lease(VehicleLease lease)
        {
            lease.LeaseStartDate = DateTime.SpecifyKind(
            lease.LeaseStartDate.Date, DateTimeKind.Utc);

            lease.LeaseEndDate = DateTime.SpecifyKind(
                lease.LeaseEndDate.Date, DateTimeKind.Utc);
            // Validate FK references
            var vehicle = await _context.Vehicles.FindAsync(lease.VehicleId);
            var client = await _context.Clients.FindAsync(lease.ClientId);

            if (vehicle == null)
            {
                ModelState.AddModelError("", "Vehicle not found.");
                await ReloadFormData(lease);
                return View(lease);
            }

            if (client == null)
            {
                ModelState.AddModelError("", "Client not found.");
                await ReloadFormData(lease);
                return View(lease);
            }

            if (vehicle.Status == VehicleStatus.Leased)
            {
                ModelState.AddModelError("", "This vehicle is already leased.");
                await ReloadFormData(lease);
                return View(lease);
            }

            if (lease.LeaseEndDate <= lease.LeaseStartDate)
            {
                ModelState.AddModelError(nameof(lease.LeaseEndDate),
                    "End date must be after start date.");
            }

            if (!ModelState.IsValid)
            {
                await ReloadFormData(lease);
                return View(lease);
            }

            // Save Lease
            lease.Id = Guid.NewGuid().ToString();
            lease.Vehicle = vehicle;
            lease.Client = client;

            _context.VehicleLeases.Add(lease);
            await _context.SaveChangesAsync();

            // Update Vehicle
            vehicle.CurrentLeaseId = lease.Id;
            vehicle.Status = VehicleStatus.Leased;

            await _context.SaveChangesAsync();

            TempData["success"] = $"Vehicle leased to {client.CompanyName}";
            return RedirectToAction("Details", new { id = lease.VehicleId });
        }

        // Helper function
        private async Task ReloadFormData(VehicleLease lease)
        {
            lease.Vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == lease.VehicleId);

            ViewBag.ClientId = new SelectList(
                _context.Clients.Where(c => !c.IsDeleted),
                "Id", "CompanyName",
                lease.ClientId
            );
        }


        // GET: Vehicles/AssignDriver/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignDriver(string id)
        {
            var vehicle = await _context.Vehicles.Include(v => v.CurrentDriver).FirstOrDefaultAsync(v => v.Id == id);
            ViewBag.DriverId = new SelectList(_context.Drivers.Where(d => d.IsActive && !d.IsDeleted), "Id", "FullName");
            return View(new VehicleAssignment
            {
                VehicleId = id,
                Vehicle = vehicle
            });
        }

        // POST: Vehicles/AssignDriver/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignDriver(VehicleAssignment assignment)
        {
            var vehicle = await _context.Vehicles.FindAsync(assignment.VehicleId);
            var driver = await _context.Drivers.FindAsync(assignment.DriverId);

            if (vehicle == null)
            {
                ModelState.AddModelError("", "Vehicle not found.");
                ViewBag.DriverId = new SelectList(_context.Drivers.Where(d => d.IsActive), "Id", "FullName", assignment.DriverId);
                return View(assignment);
            }

            if (driver == null || !driver.IsActive)
            {
                ModelState.AddModelError("", "Driver not found or inactive.");
                ViewBag.DriverId = new SelectList(_context.Drivers.Where(d => d.IsActive), "Id", "FullName", assignment.DriverId);
                return View(assignment);
            }

            if (ModelState.IsValid)
            {

                var previous = await _context.VehicleAssignments
                    .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id && a.ReturnedDate == null);

                if (previous != null)
                {
                    previous.ReturnedDate = DateTime.UtcNow.Date;
                }

                // CREATE NEW ASSIGNMENT - MUST BE NULL
                assignment.Id = Guid.NewGuid().ToString();
                assignment.Vehicle = vehicle;
                assignment.Driver = driver;
                assignment.AssignedDate = DateTime.UtcNow.Date;
                assignment.ReturnedDate = null;

                _context.VehicleAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                vehicle.CurrentDriverId = driver.Id;
                vehicle.CurrentDriver = driver;

                await _context.SaveChangesAsync();

                TempData["success"] = $"Driver {driver.FullName} assigned successfully";
                return RedirectToAction("Details", new { id = assignment.VehicleId });
            }

            ViewBag.DriverId = new SelectList(_context.Drivers.Where(d => d.IsActive), "Id", "FullName", assignment.DriverId);
            return View(assignment);
        }

        //GET: Vehicles/EndLease/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EndLease(string id)
        {
            var vehicle = await _context.Vehicles
                    .Include(v => v.CurrentLease)
                    .ThenInclude(l => l!.Client)
                    .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

            if (vehicle == null) return NotFound();

            if (vehicle.CurrentLease == null)
            {
                TempData["error"] = "This vehicle is not currently leased.";
                return RedirectToAction("Details", new { id });
            }

            return View(vehicle);
        }

        //POST: Vehicles/EndLease/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EndLease(string id, DateTime returnDate, int? finalOdometerReading,
                         string? returnNotes, decimal? penaltyFee)
        {

            returnDate = DateTime.SpecifyKind(
            DateTime.UtcNow.Date, DateTimeKind.Utc);

            var vehicle = await _context.Vehicles
                .Include(v => v.CurrentLease)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null || vehicle.CurrentLease == null)
                return NotFound();

            //Update the lease record   
            vehicle.CurrentLease.ReturnDate = returnDate;
            vehicle.CurrentLease.ReturnOdometer = finalOdometerReading;
            vehicle.CurrentLease.ReturnConditionNotes = returnNotes;
            vehicle.CurrentLease.PenaltyFee = penaltyFee;

            //clear current lease reference
            vehicle.CurrentLeaseId = null;
            vehicle.Status = VehicleStatus.Available;  // or ask for new status

            await _context.SaveChangesAsync();
            TempData["success"] = "Lease ended successfully. Vehicle is now available.";
            return RedirectToAction("Details", new { id });


        }

        //GET: Vehicles/ReturnDriver/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReturnDriver(string id)
        {
            var vehicle = await _context.Vehicles
                    .Include(v => v.CurrentDriver)
                    .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);


            // var allAssignments = await _context.VehicleAssignments
            //     .Where(a => a.VehicleId == id)
            //     .ToListAsync();

            // var debugInfo = allAssignments.Any()
            //     ? $"Found {allAssignments.Count} assignments. Current ones: {allAssignments.Count(a => a.ReturnedDate == null)}"
            //     : "No assignments exist for this vehicle at all";

            // ViewBag.DebugAssignments = debugInfo;  // Show in view        

            if (vehicle == null) return NotFound();
            if (vehicle.CurrentDriver == null)
            {
                TempData["info"] = "No driver currently assigned to this vehicle.";
                return RedirectToAction("Details", new { id });
            }
            return View(vehicle);
        }

        //POST: Vehicles/ReturnDriver/1
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> ReturnDriver(string id, DateTime? returnDate = null)
        // {
        //     var vehicle = await _context.Vehicles
        //         .Include(v => v.CurrentDriver) 
        //         .FirstOrDefaultAsync(v => v.Id == id);

        //     if (vehicle == null)
        //         return NotFound();

        //     // Find the CURRENT assignment (ReturnedDate == null)
        //     var currentAssignment = await _context.VehicleAssignments
        //         .Include(a => a.Driver)
        //         .FirstOrDefaultAsync(a => a.VehicleId == id && a.ReturnedDate == null);

        //     if (currentAssignment == null)
        //     {
        //         TempData["info"] = "No active driver assignment found to return.";
        //         return RedirectToAction("Details", new { id });
        //     }

        //     var driverName = currentAssignment.Driver?.FullName ?? "unknown";

        //     // Update the assignment
        //     currentAssignment.ReturnedDate = returnDate ?? DateTime.UtcNow.Date;

        //     // IMPORTANT: Clear the current driver reference on the vehicle
        //     vehicle.CurrentDriverId = null;
        //     vehicle.CurrentDriver = null; 

        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //         TempData["success"] = $"Driver {currentAssignment.Driver?.FullName ?? "unknown"} has been successfully returned.";
        //     }
        //     catch (Exception ex)
        //     {
        //         TempData["error"] = $"Failed to return driver: {ex.Message}";
        //         // Log ex if you have logging
        //     }

        //     return RedirectToAction("Details", new { id });
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReturnDriver(string id, DateTime? returnDate = null)
        {
            // Find the CURRENT assignment first
            var currentAssignment = await _context.VehicleAssignments
                .Include(a => a.Driver)
                .FirstOrDefaultAsync(a => a.VehicleId == id && a.ReturnedDate == null);

            if (currentAssignment == null)
            {
                TempData["info"] = "No active driver assignment found to return.";
                return RedirectToAction("Details", new { id });
            }

            var driverName = currentAssignment.Driver?.FullName ?? "N/A";

            returnDate = DateTime.SpecifyKind(
                        DateTime.UtcNow.Date, DateTimeKind.Utc);

            // Update the assignment
            currentAssignment.ReturnedDate = returnDate ?? DateTime.UtcNow.Date;

            // Now get the vehicle separately
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
                return NotFound();

           
            // Clear the current driver reference
            vehicle.CurrentDriverId = null;
            vehicle.CurrentDriver = null;

            try
            {
                await _context.SaveChangesAsync();
                TempData["success"] = $"Driver {driverName} has been successfully returned.";
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Failed to return driver: {ex.Message}";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}