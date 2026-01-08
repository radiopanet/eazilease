using EaziLease.Services.ServiceModels;
using EaziLease.Models;


namespace EaziLease.Services.Interfaces;
public interface ILeaseService
{
    Task<ServiceResult> StartLeasAsync(VehicleLease lease, string userName);
    Task<ServiceResult> EndLeaseAsync(string vehicleId, DateTime returnDate, decimal finalOdometer, decimal? penalty, string notes, string userName);
    Task<ServiceResult> ExtendLeaseAsync(string leaseId, DateTime newEndDate, string userName);
}