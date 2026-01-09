using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Rendering;
using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using EaziLease.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using EaziLease.Services;
using EaziLease.Models.ViewModels;
using EaziLease.Services.Interfaces;

namespace EaziLease.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        public readonly ApplicationDbContext _context;
        public readonly AuditService _auditService;
        public readonly ILeaseService _leaseService;



        public VehiclesController(ApplicationDbContext context,
             AuditService auditService, ILeaseService leaseService)
        {
            _context = context;
            _auditService = auditService;
            _leaseService = leaseService;
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
                    existing.OdometerReading = vehicle.OdometerReading;
                    existing.Status = vehicle.Status;
                    existing.DailyRate = vehicle.DailyRate;
                    existing.LastServiceDate = vehicle.LastServiceDate;
                    existing.PurchasePrice = vehicle.PurchasePrice;
                    existing.PurchaseDate = vehicle.PurchaseDate;
                    existing.SupplierId = vehicle.SupplierId;
                    existing.BranchId = vehicle.BranchId;

                    // if(vehicle.Status == )

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
                LeaseEndDate = DateTime.UtcNow.Date.AddMonths(1),
                MonthlyRate = vehicle.DailyRate * 30.4167m
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

            var result = await _leaseService.StartLeaseAsync(lease, User.Identity?.Name ?? "admin");

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Messsage ?? "Failed to start lease.");
                await ReloadFormData(lease); //for dropdowns.
                return View(lease);
            }

            TempData["success"] = result.Messsage ?? $"Vehicle leased successfully.";
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
        public async Task<IActionResult> EndLease(string id, EndLeaseDto dto)
        {

            var result = await _leaseService.EndLeaseAsync(id, dto, User.Identity?.Name ?? "admin");

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Messsage ?? "Failed to end lease.");
                return View(dto);
            }

            TempData["success"] = result.Messsage ?? "Lease ended successfully. Vehicle is now available.";


            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> ExtendLease(string leaseId)
        {
            var lease = await _context.VehicleLeases
                .Include(l => l.Vehicle)
                .Include(l => l.Client)
                .FirstOrDefaultAsync(l => l.Id == leaseId && l.ReturnDate == null);

            if (lease == null)
                return NotFound();

            var model = new ExtendLeaseViewModel
            {
                LeaseId = lease.Id,
                VehicleId = lease.VehicleId,
                VehicleRegistration = lease.Vehicle?.RegistrationNumber ?? "",
                ClientName = lease.Client?.CompanyName ?? "",
                CurrentEndDate = lease.LeaseEndDate,
                DailyRate = lease.Vehicle?.DailyRate ?? 0
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtendLease(string leaseId, DateTime newEndDate)
        {
            var result = await _leaseService.ExtendLeaseAsync(leaseId, newEndDate, User.Identity?.Name ?? "admin");

            if(!result.Success)
            {
                // Reload form for error display
                var lease = await _context.VehicleLeases
                    .Include(l => l.Vehicle)
                    .Include(l => l.Client)
                    .FirstOrDefaultAsync(l => l.Id == leaseId);

                var model = new ExtendLeaseViewModel
                {
                    LeaseId = leaseId,
                    VehicleId = lease?.VehicleId ?? "",
                    VehicleRegistration = lease?.Vehicle?.RegistrationNumber ?? "",
                    ClientName = lease?.Client?.CompanyName ?? "",
                    CurrentEndDate = lease.LeaseEndDate.Date,
                    DailyRate = lease?.Vehicle?.DailyRate ?? 0
                };
                ModelState.AddModelError("", result.Messsage ?? "Failed to extend lease.");
                return View(model);    
            }

            TempData["success"] = result.Messsage ?? "Lease extended successfully.";
            var vehicleId = (await _context.VehicleLeases.FindAsync(leaseId))?.VehicleId;
            return RedirectToAction("Details", new { id = vehicleId });
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


                // vehicle.CurrentDriverId = driver.Id;
                vehicle.CurrentDriver = driver;
                // driver.CurrentVehicleId = vehicle.Id;
                driver.CurrentVehicle = vehicle;

                await _context.SaveChangesAsync();
                await _auditService.LogAsync("Driver", driver.Id, "AssignDriver",
                        $"Driver {driver.FullName} has been assigned to ${vehicle.RegistrationNumber}" +
                        $"by {assignment.CreatedBy}");

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
                await _auditService.LogAsync("Driver", id,
                    "ReturnDriver", $"Driver {vehicle?.CurrentDriver?.FullName} has been returned successfully.");
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RequestRateOverride(string vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);

            if (vehicle == null) return NotFound();

            var model = new RateOverrideRequestViewModel
            {
                VehicleId = vehicle.Id,
                RequestedDailyRate = vehicle.DailyRate,
                CurrentDailyRate = vehicle.DailyRate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RequestRateOverride(RateOverrideRequestViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var vehicle = await _context.Vehicles.FindAsync(model.VehicleId);
            if (vehicle == null) return NotFound();

            model.EffectiveFrom = DateTime.SpecifyKind(model.EffectiveFrom.Value.Date,
                    DateTimeKind.Utc);
            model.EffectiveTo = DateTime.SpecifyKind(model.EffectiveTo.Value.Date, DateTimeKind.Utc);

            var request = new RateOverrideRequest
            {
                VehicleId = model.VehicleId,
                RequestedDailyRate = model.RequestedDailyRate,
                OriginalDailyRate = vehicle.DailyRate,
                IsPermanent = model.IsPermanent,
                EffectiveFrom = model.EffectiveFrom,
                EffectiveTo = model.EffectiveTo,
                RequestedBy = User.Identity?.Name ?? "unknown",
                Reason = model.Reason
            };

            _context.RateOverrideRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["success"] = "Rate override request submitted. Awaiting SuperAdmin approval.";
            return RedirectToAction("Details", new { id = model.VehicleId });
        }

    }
}