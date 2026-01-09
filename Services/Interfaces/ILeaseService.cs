using EaziLease.Services.ServiceModels;
using EaziLease.Models;
using EaziLease.Models.ViewModels;


namespace EaziLease.Services.Interfaces;
public interface ILeaseService
{
    Task<ServiceResult> StartLeaseAsync(VehicleLease lease, string userName);
    Task<ServiceResult> EndLeaseAsync(string vehicleId, EndLeaseDto dto, string userName);
    Task<ServiceResult> ExtendLeaseAsync(string leaseId, DateTime newEndDate, string userName);
    Task<bool> CanLeaseToClientAsync(string clientId, decimal newMonthlyRate);
    
}