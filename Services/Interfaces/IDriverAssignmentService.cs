using EaziLease.Services.ServiceModels;
using EaziLease.Models;

namespace EaziLease.Services.Interfaces
{
    public interface IDriverAssignmentService
    {
        Task<ServiceResult> AssignDriverAsync(VehicleAssignment assignment, string userName);
        Task<ServiceResult> ReturnDriverAsync(string driverId, DateTime? returnDate, string userName);
        Task<bool> IsDriverAvailableAsync(string driverId);
    }
}