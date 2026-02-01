using EaziLease.Domain.Entities;
using EaziLease.Infrastucture.Services.ServiceModels;

namespace EaziLease.Application.Interfaces
{
    public interface IVehicleService
    {
        Task<ServiceResult> CreateUsageSnapshotAsync(string vehicleId, string triggerEvent, string calculatedBy);
        Task<VehicleUsageSnapshot?> GetLatestSnapshotAsync(string vehicleId);
    }
}