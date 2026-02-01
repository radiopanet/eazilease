
using EaziLease.Domain.Entities;
using EaziLease.Infrastucture.Services.ServiceModels;

namespace EaziLease.Application.Interfaces
{
    public interface IMaintenanceService
    {
        Task<ServiceResult> RecordMaintenanceAsync(VehicleMaintenance maintenance, string userName);
        Task<ServiceResult> CompleteMaintenanceAsync(string maintenanceId, string completedBy);
    }
}