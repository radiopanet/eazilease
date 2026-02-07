using EaziLease.Infrastructure.Persistence;
using EaziLease.Domain.Enums;
using EaziLease.Domain.Entities;
using EaziLease.Application.Interfaces;
using EaziLease.Infrastucture.Services.ServiceModels;
using Microsoft.EntityFrameworkCore;


namespace EaziLease.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        private readonly IVehicleService _vehicleService;

        public MaintenanceService(ApplicationDbContext context, AuditService auditService, 
            IVehicleService vehicleService)
        {
            _context = context;
            _auditService = auditService;
            _vehicleService = vehicleService;
        }

        public async Task<ServiceResult> RecordMaintenanceAsync(VehicleMaintenance maintenance, string userName)
        {
            maintenance.ServiceDate = DateTime.SpecifyKind(maintenance.ServiceDate, DateTimeKind.Utc);
            maintenance.ScheduledDate = DateTime.SpecifyKind(maintenance.ScheduledDate ?? DateTime.MinValue, DateTimeKind.Utc);
            
            var vehicle = await _context.Vehicles
                .Include(v => v.CurrentDriver)
                .FirstOrDefaultAsync(v => v.Id == maintenance.VehicleId);

            if (vehicle == null)
                return new ServiceResult { Success = false, Message = "Vehicle not found." };

            // Prevent future dates for immediate maintenance
            if (!maintenance.IsFutureScheduled && maintenance.ServiceDate > DateTime.Today)
                return new ServiceResult { Success = false, Message = "Cannot record future maintenance date for immediate service." };


            //Prevent 0 cost for immediate work
            if (!maintenance.IsFutureScheduled && !maintenance.IsWarrantyWork && maintenance.Cost <= 0)
                return new ServiceResult { Success = false, Message = "Maintenance cost must be greater than zero." };

            // For immediate maintenance: require mileage
            if (!maintenance.IsFutureScheduled && !maintenance.MileageAtService.HasValue)
                return new ServiceResult { Success = false, Message = "Mileage at service is required for immediate maintenance." };


            //Warranty work overrides
            if(maintenance.IsWarrantyWork)
            {
                maintenance.Cost = 0;
                maintenance.IsBillableToClient = false;
                maintenance.BillableAmount = 0;
            }    

            //Billable work logic
            if(maintenance.IsBillableToClient)
            {
                maintenance.BillableAmount = maintenance.BillableAmount ?? maintenance.Cost; //default to full cost
            }
            else
            {
                maintenance.BillableAmount = 0;
            }

            //Insurance claim overrides
            if(maintenance.InsuranceClaimStatus == InsuranceClaimStatus.Approved)
            {
                maintenance.BillableAmount = 0; //Claim paid -> not billable to client
                maintenance.Cost = 0; 
            }



            maintenance.Id = Guid.NewGuid().ToString();

            //Handle Historical Records.
            if(maintenance.IsHistorical)
            {
                maintenance.ServiceDate = DateTime.SpecifyKind(maintenance.ServiceDate, DateTimeKind.Utc);
                vehicle.LastServiceDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                maintenance.ScheduledDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                //Past record: must be in the past
                if(maintenance.ServiceDate > DateTime.Today)
                    return new ServiceResult { Success = false, Message="Historical records must have a date in the past."};

                //Force complete (Maintenance already happened.)
                maintenance.Status = MaintenanceStatus.Completed;

                //Do not affect current availability status
                maintenance.AffectsAvailability = false;

                //Update LastServiceDate if ONLY if this is more recent than current
                if(maintenance.ServiceDate > vehicle.LastServiceDate)
                    vehicle.LastServiceDate = maintenance.ServiceDate;

                bool isRecent = (DateTime.Today - maintenance.ServiceDate).TotalDays <= (vehicle.MaintenanceIntervalMonths * 30);
                if(isRecent)
                {
                    // Recent past record → auto-create next scheduled
                    var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
                    var nextDate = today.AddMonths(vehicle.MaintenanceIntervalMonths);
                    var currentMileage = vehicle.OdometerReading ?? 0;
                    var nextMileage = currentMileage + vehicle.MaintenanceIntervalKm;

                    var nextRecord = new VehicleMaintenance
                    {
                        VehicleId = vehicle.Id,
                        Status = MaintenanceStatus.Scheduled,
                        Type = MaintenanceType.Routine,
                        ScheduledDate = nextDate,
                        ScheduledMileage = nextMileage,
                        Description = $"Auto-scheduled next {MaintenanceType.Routine} (triggered by recent historical record)",
                        IsFutureScheduled = true,
                        ServiceDate = DateTime.MinValue
                    };

                    _context.VehicleMaintenance.Add(nextRecord);

                    vehicle.NextMaintenanceDate = nextDate;
                    vehicle.NextMaintenanceMileage = nextMileage;

                    await _auditService.LogAsync("Maintenance", nextRecord.Id, "AutoScheduled",
                        $"Next scheduled maintenance auto-created after recent historical record {maintenance.Id} by {userName}");
                }
                else
                {
                    await _auditService.LogAsync("Maintenance", maintenance.Id, "HistoricalRecord",
                        $"Very old historical maintenance recorded (date: {maintenance.ServiceDate:dd MMM yyyy}) " +
                        $"on {vehicle.RegistrationNumber} by {userName} - no future scheduling triggered.");
                }
            }
            // Handle future/scheduled maintenance
            else if (maintenance.IsFutureScheduled)
            {
                maintenance.ScheduledDate = DateTime.SpecifyKind((DateTime)maintenance.ScheduledDate,
                     DateTimeKind.Utc);
                // Required fields validation for scheduled
                if (!maintenance.ScheduledDate.HasValue)
                    return new ServiceResult { Success = false, Message = "Scheduled date is required for future maintenance." };

                if (!maintenance.ScheduledMileage.HasValue)
                    return new ServiceResult { Success = false, Message = "Scheduled mileage is required for future maintenance." };

                if (maintenance.ScheduledDate <= DateTime.Today)
                    return new ServiceResult { Success = false, Message = "Scheduled date must be in the future." };

                maintenance.Status = MaintenanceStatus.Scheduled;
                maintenance.ServiceDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc); // Not yet performed



                // Calculate next due (allow manual override if needed)
                var nextDate = maintenance.ScheduledDate.Value.AddMonths(vehicle.MaintenanceIntervalMonths);
                var nextMileage = maintenance.ScheduledMileage.Value + vehicle.MaintenanceIntervalKm;

                // Update vehicle next due
                vehicle.NextMaintenanceDate = nextDate;
                vehicle.NextMaintenanceMileage = nextMileage;

                nextDate = DateTime.SpecifyKind(nextDate, DateTimeKind.Utc);
            }
            else
            {
                // Immediate maintenance
                maintenance.Status = MaintenanceStatus.InProgress;
                maintenance.ServiceDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

                // Update vehicle odometer with latest reading
                if (maintenance.MileageAtService.HasValue)
                    vehicle.OdometerReading = maintenance.MileageAtService.Value;

                // If admin checked "makes vehicle unavailable"
                if (maintenance.AffectsAvailability)
                {
                    vehicle.Status = VehicleStatus.InMaintenance;

                    // Auto-return driver if currently assigned
                    var assignment = await _context.VehicleAssignments
                        .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id && a.ReturnedDate == null);

                    if (assignment != null)
                    {
                        assignment.ReturnedDate = DateTime.UtcNow;
                        vehicle.CurrentDriverId = null;

                        await _auditService.LogAsync("DriverAssignment", assignment.Id, "AutoReturned",
                            $"Driver auto-returned due to maintenance start (ID: {maintenance.Id}) by {userName}");
                    }

                    await _auditService.LogAsync("Vehicle", vehicle.Id, "StatusChange",
                        $"Vehicle status changed to InMaintenance due to maintenance record {maintenance.Id} by {userName}");
                }
            }



            try
            {
                _context.VehicleMaintenance.Add(maintenance);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            decimal currentScore = 0m;
            if (vehicle.OdometerReading > 0 && vehicle.PurchasePrice > 0)
            {
                var completed = vehicle.MaintenanceHistory
                    .Where(m => m.Status == MaintenanceStatus.Completed)
                    .ToList();

                decimal totalCost = completed.Sum(m => m.Cost ?? 0);
                int repairCount = completed.Count(m => m.Type == MaintenanceType.Repair);

                decimal costFactor = (totalCost / vehicle.PurchasePrice) * 10m ?? 0;
                decimal freqFactor = (repairCount * 10000m) / vehicle.OdometerReading ?? 0;
                currentScore = Math.Min(10m, Math.Round(costFactor + freqFactor, 1));
            }

            // Trigger snapshot on high cost or high score
            if (maintenance.Cost > 5000m || currentScore >= 7.0m)
            {
                await _vehicleService.CreateUsageSnapshotAsync(vehicle.Id, "HighMaintenanceEvent", userName);
            }

            await _auditService.LogAsync("Maintenance", maintenance.Id, "Recorded",
                $"Maintenance {(maintenance.IsFutureScheduled ? "scheduled" : "recorded")} on {vehicle.RegistrationNumber}: " +
                $"{maintenance.Description} @ R{maintenance.Cost} by {userName}");

            return new ServiceResult { Success = true, Message = "Maintenance record saved successfully." };
        }
        public async Task<ServiceResult> CompleteMaintenanceAsync(string maintenanceId, string completedBy)
        {
            var maintenance = await _context.VehicleMaintenance
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == maintenanceId);

            if (maintenance == null)
                return new ServiceResult { Success = false, Message = "Maintenance record not found." };

            if (maintenance.Status != MaintenanceStatus.InProgress)
                return new ServiceResult { Success = false, Message = "Only InProgress records can be completed." };

            maintenance.Status = MaintenanceStatus.Completed;

            var vehicle = maintenance.Vehicle;
            if (vehicle != null)
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
                vehicle.NextMaintenanceMileage = nextMileage;

            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Maintenance", maintenance.Id, "Completed",
                $"Maintenance completed by {completedBy}. Next scheduled auto-created for {vehicle?.RegistrationNumber}.");

            return new ServiceResult { Success = true, Message = "Maintenance completed. Next service auto-scheduled." };
        }
    }
}