using EaziLease.Services.ServiceModels;

namespace EaziLease.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<ServiceResult> CreateUsageSnapshotAsync(string vehicleId, string triggerEvent, string calculatedBy);
    }
}