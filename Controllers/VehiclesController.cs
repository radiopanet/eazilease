using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Rendering;
using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using EaziLease.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using EaziLease.Services;

namespace EaziLease.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        public readonly ApplicationDbContext _context;
        public readonly AuditService _auditService;

        public VehiclesController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
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
                .Include(v => v.Supplier)
                .Include(v => v.Branch)
                .Include(v => v.CurrentDriver)
                .Include(v => v.MaintenanceHistory)
                .Include(v => v.CurrentLease).ThenInclude(l => l!.Client)
                .Include(v => v.LeaseHistory).ThenInclude(l => l.Client)
                .Include(v => v.AssignementHistory).ThenInclude(a => a.Driver)
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

                await _auditService.LogAsync("Vehicle", vehicle.Id, "Created", $"New Vehicle {vehicle.RegistrationNumber} added.");
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

                await _auditService.LogAsync("Vehicle", vehicle.Id, "Deleted", $"Vehicle {vehicle.RegistrationNumber} deleted.");
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

            await _auditService.LogAsync("Lease", lease.Id, "LeaseStarted",
                    $"Vehicle {lease.Vehicle?.RegistrationNumber} leased to {lease.Client?.CompanyName}");

            TempData["success"] = $"Vehicle leased to {client.CompanyName}";
            return RedirectToAction("Details", new { id = lease.VehicleId });
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

            await _auditService.LogAsync("Lease", vehicle.CurrentLease.Id, "LeaseEnded", $"Vehicle {vehicle.RegistrationNumber} lease has ended.");
            TempData["success"] = "Lease ended successfully. Vehicle is now available.";
            return RedirectToAction("Details", new { id });


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
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);

            // Get available drivers (not currently assigned)
            var availableDrivers = await _context.Drivers
                .Where(d => d.IsActive && !d.IsDeleted && string.IsNullOrEmpty(d.CurrentVehicleId))
                .OrderBy(d => d.LastName)
                .Select(d => new SelectListItem
                {
                    Value = d.Id,
                    Text = d.FullName
                })
                .ToListAsync();

            // Get busy drivers (for warning display)
            var busyDrivers = await _context.Drivers
                .Where(d => d.IsActive && !d.IsDeleted && !string.IsNullOrEmpty(d.CurrentVehicleId))
                .Include(d => d.CurrentVehicle)
                .OrderBy(d => d.LastName)
                .ToListAsync();

            ViewBag.DriverId = new SelectList(availableDrivers, "Value", "Text");
            ViewBag.BusyDrivers = busyDrivers;  // For warning display
            ViewBag.Vehicle = vehicle;

            return View(new VehicleAssignment { VehicleId = id });
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
                await LoadDriverDropdowns(); // helper method
                return View(assignment);
            }

            if (driver == null || !driver.IsActive)
            {
                ModelState.AddModelError("", "Driver not found or inactive.");
                await LoadDriverDropdowns();
                return View(assignment);
            }

            // **CRITICAL: Check if driver is already assigned elsewhere**
            if (!string.IsNullOrEmpty(driver.CurrentVehicleId))
            {
                ModelState.AddModelError("",
                    $"Driver {driver.FullName} is already assigned to vehicle {driver.CurrentVehicle?.RegistrationNumber} " +
                    $"(Reg: {driver.CurrentVehicle?.RegistrationNumber}). Please return them first.");
                await LoadDriverDropdowns();
                return View(assignment);
            }

            if (ModelState.IsValid)
            {
                // Close previous assignment for THIS vehicle
                var previous = await _context.VehicleAssignments
                    .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id && a.ReturnedDate == null);

                if (previous != null)
                {
                    previous.ReturnedDate = DateTime.SpecifyKind(
                        DateTime.UtcNow.Date, DateTimeKind.Utc);
                }

                // Create new assignment
                assignment.Id = Guid.NewGuid().ToString();
                assignment.Vehicle = vehicle;
                assignment.Driver = driver;
                assignment.AssignedDate = DateTime.SpecifyKind(
                        DateTime.UtcNow.Date, DateTimeKind.Utc);
                assignment.ReturnedDate = null;

                _context.VehicleAssignments.Add(assignment);
                await _context.SaveChangesAsync();


                vehicle.CurrentDriverId = driver.Id;
                driver.CurrentVehicleId = vehicle.Id;

                await _context.SaveChangesAsync();

                TempData["success"] = $"Driver {driver.FullName} assigned to {vehicle.RegistrationNumber}";
                return RedirectToAction("Details", new { id = assignment.VehicleId });
            }

            await LoadDriverDropdowns();
            return View(assignment);
        }

        // Helper method to reload dropdowns
        private async Task LoadDriverDropdowns()
        {
            ViewBag.DriverId = new SelectList(
                await _context.Drivers.Where(d => d.IsActive && !d.IsDeleted).OrderBy(d => d.LastName)
                    .Select(d => new SelectListItem { Value = d.Id, Text = d.FullName })
                    .ToListAsync(),
                "Value", "Text", null);
        }


        //GET: Vehicles/ReturnDriver/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReturnDriver(string id)
        {
            var vehicle = await _context.Vehicles
                    .Include(v => v.CurrentDriver)
                    .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

            if (vehicle == null) return NotFound();
            if (vehicle.CurrentDriver == null)
            {
                TempData["info"] = "No driver currently assigned to this vehicle.";
                return RedirectToAction("Details", new { id });
            }
            return View(vehicle);
        }

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

        [Authorize(Roles = "Admin")]
        public IActionResult AddMaintenance(string vehicleId)
        {
            var vm = new VehicleMaintenance { VehicleId = vehicleId };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMaintenance(VehicleMaintenance maintenance)
        {
            var vehicle = await _context.Vehicles.FindAsync(maintenance.VehicleId);
            if (vehicle == null) return NotFound();
            maintenance.ServiceDate = DateTime.SpecifyKind(
            DateTime.UtcNow.Date, DateTimeKind.Utc);

            foreach(var kvp in ModelState)
            {
                foreach(var err in kvp.Value.Errors)
                {
                    Console.WriteLine($"Property {kvp.Key} with value {kvp.Value} gave the error {err.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {
                maintenance.Id = Guid.NewGuid().ToString();
                _context.VehicleMaintenance.Add(maintenance);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Maintenance", maintenance.Id, "ServiceRecorded",
                    $"Maintenance on {vehicle.RegistrationNumber}: {maintenance.Description} @ R{maintenance.Cost}");

                TempData["success"] = "Maintenance record added";
                return RedirectToAction("Details", new { id = maintenance.VehicleId });
            }

            return View(maintenance);
        }
    }
}