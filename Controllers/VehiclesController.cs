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
        public readonly IDriverAssignmentService _driverAssignmentService;
        public readonly IMaintenanceService _maintenanceService;
        public readonly IVehicleService _vehicleService;



        public VehiclesController(ApplicationDbContext context,
             AuditService auditService, ILeaseService leaseService,
             IDriverAssignmentService driverAssignmentService,
             IMaintenanceService maintenanceService, IVehicleService vehicleService)
        {
            _context = context;
            _auditService = auditService;
            _leaseService = leaseService;
            _driverAssignmentService = driverAssignmentService;
            _maintenanceService = maintenanceService;
            _vehicleService = vehicleService;
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
            vehicle.PurchaseDate = DateTime.SpecifyKind(vehicle.PurchaseDate, DateTimeKind.Utc);
            
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

            vehicle.LastServiceDate = DateTime.SpecifyKind(vehicle.LastServiceDate ?? DateTime.MinValue, DateTimeKind.Utc);
            vehicle.PurchaseDate = DateTime.SpecifyKind(vehicle.PurchaseDate, DateTimeKind.Utc);

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

        [HttpGet]
        public async Task<JsonResult> GetClientEligibility(string clientId, string vehicleId)
        {
            var client = await _context.Clients
                .Include(c => c.Leases)
                .FirstOrDefaultAsync(c => c.Id == clientId);

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);

            if (client == null || vehicle == null) return Json(new { success = false });

            // Calculate rates (same logic from the Service - TOFIX: REPETITIVE CODE)
            decimal monthlyRate = vehicle.DailyRate * 30.4167m;
            decimal currentCommitted = client.CurrentCommittedAmount;
            decimal projectedTotal = currentCommitted + monthlyRate;
            bool hasLeasedBefore = await _context.VehicleLeases
                .AnyAsync(l => l.VehicleId == vehicleId && l.ClientId == clientId);

            return Json(new
            {
                success = true,
                limit = client.CreditLimit,
                current = currentCommitted,
                incoming = monthlyRate,
                projected = projectedTotal,
                hasLeasedBefore = hasLeasedBefore,
                isOverLimit = projectedTotal > client.CreditLimit
            });
        }
        // GET: Vehicle/Lease/1
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Lease(string id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.CurrentLease)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

            ViewBag.AvailableDrivers = await _context.Drivers
               .Where(d => d.IsActive && string.IsNullOrEmpty(d.CurrentVehicleId))
               .Select(d => new { d.Id, d.FullName })
               .ToListAsync();


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
                ModelState.AddModelError("", result.Message ?? "Failed to start lease.");
                await ReloadFormData(lease); //for dropdowns.
                return View(lease);
            }

            TempData["success"] = result.Message ?? $"Vehicle leased successfully.";
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
                ModelState.AddModelError("", result.Message ?? "Failed to end lease.");
                return View(dto);
            }



            TempData["success"] = result.Message ?? "Lease ended successfully. Vehicle is now available.";

            await _vehicleService.CreateUsageSnapshotAsync(id, "LeaseEnd", User.Identity?.Name ?? "admin");

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

            if (!result.Success)
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
                ModelState.AddModelError("", result.Message ?? "Failed to extend lease.");
                return View(model);
            }

            TempData["success"] = result.Message ?? "Lease extended successfully.";
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
            var result = await _driverAssignmentService.AssignDriverAsync(assignment, User.Identity?.Name ?? "admin");

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to assign driver.");
                // Reload dropdowns (keep your helper)
                await LoadDriverDropdowns();
                return View(assignment);
            }

            TempData["success"] = result.Message;
            return RedirectToAction("Details", new { id = assignment.VehicleId });
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
            var result = await _driverAssignmentService.ReturnDriverAsync(id, returnDate, User.Identity?.Name ?? "admin");

            if (!result.Success)
            {
                TempData["error"] = result.Message ?? "Failed to return driver.";
                return RedirectToAction("Details", new { id });
            }

            TempData["success"] = result.Message;
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddMaintenance(string vehicleId)
        {
            var vm = new VehicleMaintenance { VehicleId = vehicleId };
            ViewBag.Garages = _context.Garages
                .Where(g => !g.IsDeleted)
                .OrderBy(g => g.Name)
                .Select(g => new { g.Id, g.Name })
                .ToList();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMaintenance(VehicleMaintenance maintenance)
        {
            var result = await _maintenanceService.RecordMaintenanceAsync(maintenance, User.Identity?.Name ?? "admin");

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message ?? "Failed to add maintenance record.");
                return View(maintenance);
            }

            TempData["success"] = "Maintenance record added";
            return RedirectToAction("Details", new { id = maintenance.VehicleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteMaintenance(string maintenanceId)
        {
            var result = await _maintenanceService.CompleteMaintenanceAsync(maintenanceId, User.Identity?.Name ?? "admin");

            TempData[result.Success ? "success" : "error"] = result.Message;

            // Redirect back to vehicle details (fetch vehicleId from maintenance)
            var maintenance = await _context.VehicleMaintenance.FindAsync(maintenanceId);
            var vehicleId = maintenance?.VehicleId ?? "";

            return RedirectToAction("Details", new { id = vehicleId });
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireSuperAdmin")]
        public async Task<IActionResult> ToggleRateOverride(string id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            vehicle.OverrideHighMaintenanceRate = !vehicle.OverrideHighMaintenanceRate;

            await _context.SaveChangesAsync();

            TempData["success"] = $"Rate override {(vehicle.OverrideHighMaintenanceRate ? "activated" : "deactivated")}";
            await _auditService.LogAsync("Vehicle", id, "RateOverrideToggled",
                $"SuperAdmin toggled rate override to {vehicle.OverrideHighMaintenanceRate} for {vehicle.RegistrationNumber}");

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RequireSuperAdmin")]
        public async Task<IActionResult> UpdateRateOverrideNotes(string id, string notes)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            vehicle.OverrideRateNotes = notes;

            await _context.SaveChangesAsync();

            TempData["success"] = "Rate override notes updated.";
            await _auditService.LogAsync("Vehicle", id, "RateOverrideNotesUpdated",
                $"SuperAdmin updated rate override notes for {vehicle.RegistrationNumber}");

            return RedirectToAction("Details", new { id });
        }
    }
}