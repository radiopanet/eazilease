using Microsoft.EntityFrameworkCore;
using EaziLease.Data;
using EaziLease.Services.Interfaces;
using EaziLease.Services.ServiceModels;
using EaziLease.Models;

namespace EaziLease.Services
{
    public class DriverAssignmentService: IDriverAssignmentService
    {
        public readonly ApplicationDbContext _context;
        public readonly AuditService _auditService;

        public DriverAssignmentService(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<ServiceResult> AssignDriverAsync(VehicleAssignment assignment, string userName)
        {
            var vehicle = await _context.Vehicles.FindAsync(assignment.VehicleId);
            var driver = await _context.Drivers.FindAsync(assignment.DriverId);

            if(vehicle == null || vehicle.IsDeleted || vehicle.Status == VehicleStatus.Sold)
                return new ServiceResult { Success = false, Message = "Vehicle could not be found."};

            if(driver == null || driver.IsDeleted || !driver.IsActive)
                return new ServiceResult { Success = false, Message = "Active driver not found."};

            var existing = await _context.VehicleAssignments
                .FirstOrDefaultAsync(a => a.DriverId == assignment.DriverId && a.ReturnedDate == null);
            if(existing != null)
            {
                var currentVehicle = await _context.Vehicles.FindAsync(existing.VehicleId);

                return new ServiceResult { Success = false, Message = $"Driver {driver.FullName} is already assigned to vehicle {currentVehicle?.RegistrationNumber ?? "unknown"}. " +
                    "Please return them first."};
            } 

            //Close previous assignment for this vehicle
            var previous = await _context.VehicleAssignments
                .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id && a.ReturnedDate == null);

            if(previous != null)
            {
                previous.ReturnedDate = DateTime.UtcNow.Date;
            }

            //Create new assignment
            assignment.Id = Guid.NewGuid().ToString();
            assignment.AssignedDate = DateTime.UtcNow.Date;
            assignment.ReturnedDate = null;

            _context.VehicleAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            
            //Update vehicle and driver
            vehicle.CurrentDriverId = driver.Id;
            driver.CurrentVehicleId = vehicle.Id;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("DriverAssignment", assignment.Id, "Assigned",
                $"Driver {driver.FullName} assigned to {vehicle.RegistrationNumber} by {userName}");

            return new ServiceResult { Success = true, Message = $"Driver {driver.FullName} assigned to {vehicle.RegistrationNumber}"};
        }

        public async Task<ServiceResult> ReturnDriverAsync(string vehicleId, DateTime? returnDate, string userName)
        {
            var assignment = await _context.VehicleAssignments
                    .Include(a => a.Vehicle)
                    .Include(a => a.Driver)
                    .FirstOrDefaultAsync(a => a.VehicleId == vehicleId && a.ReturnedDate == null);

            if(assignment == null)
                return new ServiceResult { Success = false, Message = "No active driver assignment for this vehicle."};

            assignment.ReturnedDate = DateTime.UtcNow;

            var vehicle = assignment.Vehicle;
            vehicle!.CurrentDriverId = null;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("DriverAssignment", assignment.Id, "Returned",
                $"Driver {assignment.Driver?.FullName} returned from {vehicle.RegistrationNumber} by {userName}"); 


            return new ServiceResult { Success = true, Message = $"Driver {assignment.Driver?.FullName} returned successfully."};
        }

        public async Task<bool> IsDriverAvailableAsync(string driverId)
        {
            return !await _context.VehicleAssignments
                .AnyAsync(a => a.DriverId == driverId && a.ReturnedDate == null);
        }
    }
}