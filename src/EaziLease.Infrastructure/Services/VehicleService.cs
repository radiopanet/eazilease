using EaziLease.Data;
using EaziLease.Domain.Enums;
using EaziLease.Domain.Entities;
using EaziLease.Application.Interfaces;
using EaziLease.Infrastucture.Services.ServiceModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EaziLease.Services
{
    public class VehicleService: IVehicleService
    {
        public readonly ApplicationDbContext _context;
        public readonly AuditService _auditService;

        public VehicleService(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }
        public async Task<ServiceResult> CreateUsageSnapshotAsync(string vehicleId, string triggerEvent, string calculatedBy)
        {
            var vehicle = await _context.Vehicles
                .Include(m => m.MaintenanceHistory)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if(vehicle == null)
                return new ServiceResult {Success = false, Message = "Vehicle not found."};

            var completedMaintenance = vehicle.MaintenanceHistory
                .Where(m => m.Status == MaintenanceStatus.Completed)
                .ToList();

            decimal totalCost = completedMaintenance.Sum(m => m.Cost ?? 0);
            decimal totalKm = vehicle.OdometerReading ?? 0;
            int totalRecords = completedMaintenance.Count;
            int repairCount = completedMaintenance.Count(m => m.Type == MaintenanceType.Repair);

            decimal score = 0;
            if(totalKm > 0 && vehicle.PurchasePrice > 0)
            {
                decimal costFactor = (totalCost / vehicle.PurchasePrice) * 10m ?? 0;
                decimal freqFactor = (repairCount * 10000m) / totalKm;
                score = Math.Min(10m, Math.Round(costFactor + freqFactor, 1));
            }

            var snapshot =  new VehicleUsageSnapshot
            {
                VehicleId = vehicleId,
                SnapshotDate = DateTime.UtcNow,
                PeriodName = triggerEvent,
                MaintenanceScore = score,
                TotalMaintenanceCost = totalCost,
                TotalKmDriven = totalKm,
                TotalMaintenanceRecords = totalRecords,
                RepairRecordsCount = repairCount,
                CalculatedBy = calculatedBy,
                TriggerEvent = triggerEvent
            };

            _context.VehicleUsageSnapshots.Add(snapshot);
                await _context.SaveChangesAsync();

            await _auditService.LogAsync("VehicleUsage", vehicleId, "SnapshotCreated",
                $"Usage snapshot created for {triggerEvent}. Score: {score:F1}, Cost/km: {snapshot.CostPerKm:F3}");


            return new ServiceResult { Success = true, Message = ""};
        }

        public async Task<VehicleUsageSnapshot?> GetLatestSnapshotAsync(string vehicleId)
        {
            return await _context.VehicleUsageSnapshots
                .Where(s => s.VehicleId == vehicleId)
                .OrderByDescending(s => s.SnapshotDate)
                .FirstOrDefaultAsync();
        }
    }
}