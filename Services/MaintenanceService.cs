using EaziLease.Data;
using EaziLease.Models;
using EaziLease.Services.Interfaces;
using EaziLease.Services.ServiceModels;

namespace EaziLease.Services
{
    public class MaintenanceService: IMaintenanceService
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

            maintenance.ServiceDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            if(vehicle == null)
                return new ServiceResult { Success = false, Message = "Vehicle not found."};

            if(maintenance.ServiceDate > DateTime.Today)
                return new ServiceResult { Success = false, Message = "Cannot record future maintenance date."}; 

            if(maintenance.Cost <= 0)
                return new ServiceResult { Success = false, Message = "Maintenance cost must be greater than zero."};

            //Update vehicle last service date with the record service date.
            vehicle.LastServiceDate = maintenance.ServiceDate;

            maintenance.Id = Guid.NewGuid().ToString();
            maintenance.ServiceDate = DateTime.UtcNow.Date;

            _context.VehicleMaintenance.Add(maintenance);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Maintenance", maintenance.Id, "ServiceRecorded",
                $"Maintenance on {vehicle.RegistrationNumber}: {maintenance.Description} @ R{maintenance.Cost} by {userName}");
            return new ServiceResult { Success = true, Message = "Maintenance record added successfully." };
        }
    }
}