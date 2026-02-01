using EaziLease.Infrastucture.Services.ServiceModels;
using EaziLease.Domain.Entities;
using EaziLease.Application.DTOs;


namespace EaziLease.Application.Interfaces;
public interface ILeaseService
{
    Task<ServiceResult> StartLeaseAsync(VehicleLease lease, string userName);
    Task<ServiceResult> EndLeaseAsync(string vehicleId, EndLeaseDto dto, string userName);
    Task<ServiceResult> ExtendLeaseAsync(string leaseId, DateTime newEndDate, string userName);
    Task<bool> CanLeaseToClientAsync(string clientId, decimal newMonthlyRate);
    
}