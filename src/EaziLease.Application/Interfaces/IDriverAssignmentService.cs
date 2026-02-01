using EaziLease.Infrastucture.Services.ServiceModels;
using EaziLease.Domain.Entities;

namespace EaziLease.Application.Interfaces
{
    public interface IDriverAssignmentService
    {
        Task<ServiceResult> AssignDriverAsync(VehicleAssignment assignment, string userName);
        Task<ServiceResult> ReturnDriverAsync(string driverId, DateTime? returnDate, string userName);
        Task<bool> IsDriverAvailableAsync(string driverId);
    }
}