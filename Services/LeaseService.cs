using EaziLease.Data;
using EaziLease.Models;
using EaziLease.Services.Interfaces;
using EaziLease.Services.ServiceModels;
using Microsoft.AspNetCore.Http.HttpResults;
namespace EaziLease.Services;

public class LeaseService: ILeaseService
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _auditService;

    public LeaseService(ApplicationDbContext context, AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ServiceResult> StartLeaseAsync(VehicleLease lease, string userName)
    {
        var vehicle =  await _context.Vehicles.FindAsync(lease.VehicleId);
        var client = await _context.Clients.FindAsync(lease.ClientId);

        //1. Validation Logic <-> Moved from VehiclesController
        if(vehicle == null || vehicle.Status == VehicleStatus.Leased)
            return new ServiceResult { Success = false, Messsage = "Vehicle Unavailable."};

        if(client ==  null || (client.CurrentCommittedAmount + (vehicle.DailyRate * 30)) > client.CreditLimit)
            return new ServiceResult { Success = false, Messsage = "Client credit limit exceeded."};

        //2. Business Logic <-> Moved from Vehicles Controller
        lease.Id = Guid.NewGuid().ToString();
        lease.CalculateMonthlyRate(vehicle.DailyRate);
        lease.CreatedBy = userName;

        //3.State Changes
        vehicle.Status = VehicleStatus.Leased;
        vehicle.CurrentLeaseId = lease.Id;

        _context.VehicleLeases.Add(lease);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Lease", lease.Id, "Started", $"Leased to {client.CompanyName}");

        return new ServiceResult { Success = true, Messsage ="Lease successfully started."};
            
    }
}