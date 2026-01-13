using EaziLease.Data;
using EaziLease.Models;
using EaziLease.Services.Interfaces;
using EaziLease.Services.ServiceModels;
using Microsoft.EntityFrameworkCore;

namespace EaziLease.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public MaintenanceService(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }
        public async Task<ServiceResult> RecordMaintenanceAsync(VehicleMaintenance maintenance, string userName)
        {
            var vehicle = await _context.Vehicles.FindAsync(maintenance.VehicleId);

            maintenance.ServiceDate = DateTime.SpecifyKind(maintenance.ServiceDate, DateTimeKind.Utc);
            maintenance.ScheduledDate = DateTime.SpecifyKind(maintenance.ScheduledDate ?? DateTime.MinValue, DateTimeKind.Utc);

            if (vehicle == null)
                return new ServiceResult { Success = false, Message = "Vehicle not found." };


            //Prevent future maintenance for immediate service.
            if (!maintenance.IsFutureScheduled && maintenance.ServiceDate > DateTime.Today)
                return new ServiceResult { Success = false, Message = "Cannot record future maintenance date for immediate service." };

            //For immediate maintenance
            if(!maintenance.IsFutureScheduled && !maintenance.MileageAtService.HasValue)
                return new ServiceResult { Success = false, Message="Mileage at service is required."};

            if(maintenance.MileageAtService.HasValue)
                vehicle.OdometerReading = maintenance.MileageAtService.Value;

            //NEW: If checkbox checked -> Set InMaintenance and Auto-Return driver
            if (maintenance.AffectsAvailability)
            {
                vehicle.Status = VehicleStatus.InMaintenance;

                //Auto-return driver if assigned
                var assignment = await _context.VehicleAssignments
                    .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id && a.ReturnedDate == null);

                if (assignment != null)
                {
                    assignment.ReturnedDate = DateTime.UtcNow;
                    vehicle.CurrentDriverId = null;
                    vehicle.CurrentDriver = null;
                }

                await _auditService.LogAsync("Vehicle", vehicle.Id, "StatusChange",
                    $"Vehicle status changed to InMaintenance due to maintenance record {maintenance.Id} by {userName}");
            }

            // if (maintenance.Cost <= 0)
            //     return new ServiceResult { Success = false, Message = "Maintenance cost must be greater than zero." };

            //Update vehicle last service date with the record service date.
            // vehicle.LastServiceDate = maintenance.ServiceDate;
            maintenance.Id = Guid.NewGuid().ToString();

            //Handle future scheduling
            if (maintenance.IsFutureScheduled)
            {
                //Required fields validation
                if (!maintenance.ScheduledDate.HasValue)
                    return new ServiceResult { Success = false, Message = "Scheduled date is required for future maintenance." };

                if (!maintenance.ScheduledMileage.HasValue)
                    return new ServiceResult { Success = false, Message = "Schedule mileage is required for future maintenance." };

                maintenance.Status = MaintenanceStatus.Scheduled;
                maintenance.ServiceDate = DateTime.MinValue;

                //Calculate next due (allow manual override.)
                var nextDate = maintenance.ScheduledDate.Value.AddMonths(vehicle.MaintenanceIntervalMonths);
                var nextMileage = maintenance.ScheduledMileage.Value + vehicle.MaintenanceIntervalKm;

                //update vehicle next due
                vehicle.NextMaintenanceDate = nextDate;
                vehicle.NextMaintenanceMileage = nextMileage;
                
            }
            else
            {
                //Immediate maintenance
                maintenance.Status = MaintenanceStatus.InProgress;
                maintenance.ServiceDate = DateTime.UtcNow;
            }



            _context.VehicleMaintenance.Add(maintenance);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Maintenance", maintenance.Id, "Recorded",
                    $"Maintenance {(maintenance.IsFutureScheduled ? "scheduled" : "recorded")} on {vehicle.RegistrationNumber}: " +
                    $"{maintenance.Description} @ R{maintenance.Cost} by {userName}");
                    
            return new ServiceResult { Success = true, Message = "Maintenance record added successfully." };
        }

        public async Task<ServiceResult> CompleteMaintenanceAsync(string maintenanceId, string completedBy)
        {
            var maintenance = await _context.VehicleMaintenance
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == maintenanceId);

            if(maintenance == null)
                return new ServiceResult { Success = false, Message = "Maintenance record not found."};

            if(maintenance.Status != MaintenanceStatus.InProgress)
                return new ServiceResult { Success = false, Message = "Only InProgress records can be completed." };

            maintenance.Status = MaintenanceStatus.Completed;

            var vehicle = maintenance.Vehicle;
            if(vehicle != null)
            {
                //Calculate next due
                var today = DateTime.UtcNow.Date;
                var nextDate = today.AddMonths(vehicle.MaintenanceIntervalMonths);
                var currentMileage = vehicle.OdometerReading ?? 0;
                var nextMileage = currentMileage + vehicle.MaintenanceIntervalKm;

                //Create next schedule record
                var nextRecord = new VehicleMaintenance
                {
                    VehicleId = vehicle.Id,
                    Status = MaintenanceStatus.Scheduled,
                    Type = MaintenanceType.Routine, //default type to routine until changed.
                    ScheduledDate = nextDate,
                    ScheduledMileage = nextMileage,
                    Description = $"Auto-scheduled next {MaintenanceType.Routine}",
                    IsFutureScheduled = true,
                    ServiceDate = DateTime.MinValue
                };

                _context.VehicleMaintenance.Add(nextRecord);

                vehicle.NextMaintenanceDate = nextDate;
                vehicle.NextMaintenanceMileage= nextMileage;

            }            

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Maintenance", maintenance.Id, "Completed",
                $"Maintenance completed by {completedBy}. Next scheduled auto-created for {vehicle?.RegistrationNumber}.");
            
            return new ServiceResult { Success = true, Message = "Maintenance completed. Next service auto-scheduled." };
        }
    }
}