
using EaziLease.Models;
using EaziLease.Services.ServiceModels;
namespace EaziLease.Services.Interfaces
{
    public interface IMaintenanceService
    {
        Task<ServiceResult> RecordMaintenanceAsync(VehicleMaintenance maintenance, string userName);
    }
}