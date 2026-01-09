using EaziLease.Data;
using EaziLease.Models;
using EaziLease.Models.ViewModels;
using EaziLease.Services.Interfaces;
using EaziLease.Services.ServiceModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
        var client = await _context.Clients
            .Include(c => c.Leases)
            .FirstOrDefaultAsync(c => c.Id == lease.ClientId);


        //1. Validation Logic <-> Moved from VehiclesController
        if(vehicle == null || vehicle.Status == VehicleStatus.Leased)
            return new ServiceResult { Success = false, Message = "Vehicle Unavailable."};

        if(client ==  null || (client.CurrentCommittedAmount + lease.MonthlyRate) > client.CreditLimit)
            return new ServiceResult { Success = false, Message = "Client credit limit exceeded."};

        if(lease.LeaseEndDate <= lease.LeaseStartDate)
            return new ServiceResult {Success = false, Message="End date must be after start date"};

        decimal currentCommitted = client.Leases?
                .Where(l => l.IsActive)
                .Sum(l => l.MonthlyRate) ?? 0;

        decimal newTotal = currentCommitted + lease.MonthlyRate;

        if(newTotal > client.CreditLimit)
            return new ServiceResult {Success = false,
             Message = $"Client credit limit exceeded. Current: {currentCommitted:C}, " +
             $"New total: {newTotal:C} (limit: {client.CreditLimit})"};

        //Prevent same client leasing same vehicle twice   
        var hasLeasedBefore = await _context.VehicleLeases
            .AnyAsync(l => l.VehicleId == lease.VehicleId && l.ClientId == lease.ClientId);

        if(hasLeasedBefore)
        {
            return new ServiceResult {Success = false, Message="This client has already leased this vehicle before."};
        }

        //Apply business logic
        lease.CalculateMonthlyRate(vehicle.DailyRate);
        lease.Id = Guid.NewGuid().ToString();
        lease.Vehicle = vehicle;
        lease.Client = client;

        _context.VehicleLeases.Add(lease);
        await _context.SaveChangesAsync();

        //State change. Update vehicle status.
        vehicle.CurrentLeaseId = lease.Id;
        vehicle.Status = VehicleStatus.Leased;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Lease", lease.Id, "Started", $"Vehicle {vehicle.RegistrationNumber} leased to {client.CompanyName} by {userName}");

        return new ServiceResult { Success = true, Message ="Lease successfully started."};
            
    }

    public async Task<ServiceResult> EndLeaseAsync(string vehicleId, EndLeaseDto dto, string userName)
    {
        dto.ReturnDate = DateTime.SpecifyKind(dto.ReturnDate, DateTimeKind.Utc);
        var vehicle = await _context.Vehicles
            .Include(v => v.CurrentLease)
            .ThenInclude(l => l!.Client)
            .Include (v => v.CurrentDriver)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        var driver = await _context.Drivers
                .Include(v => v.CurrentVehicle)
                .FirstOrDefaultAsync(v => v.CurrentVehicleId == vehicleId);

        if(vehicle == null || vehicle.CurrentLease == null)
            return new ServiceResult { Success = false, Message ="No active leases found for this vehicle."};

        var lease = vehicle.CurrentLease;

        //Update lease
        lease.ReturnDate = dto.ReturnDate.Date;      
        if(lease.ReturnDate < lease.LeaseEndDate)
        {
            lease.Status = LeaseStatus.Terminated;
        }
        lease.Status = LeaseStatus.Completed;
        lease.ReturnOdometer = dto.FinalOdometerReading;
        lease.ReturnConditionNotes = dto.ReturnNotes;
        lease.PenaltyFee = dto.PenaltyFee;
        //Calculate final amount
        lease.FinalAmount = lease.CalculateProRataAmount(vehicle.DailyRate) + (dto.PenaltyFee ?? 0);

        //Auto return driver if assigned
        if(vehicle.CurrentDriverId != null)
        {
            var assignment = await _context.VehicleAssignments
                .FirstOrDefaultAsync(a => a.VehicleId == vehicle.Id && a.ReturnedDate == null);
            if(assignment != null)
            {
                assignment.ReturnedDate = dto.ReturnDate;
                vehicle.CurrentDriverId = null;
                vehicle.CurrentDriver = null;  
                driver.CurrentVehicle = null;
                driver.CurrentVehicleId = null;            
            }    
        }

        //Update vehicle
        vehicle.OdometerReading = dto.FinalOdometerReading;
        vehicle.CurrentLeaseId = null;
        vehicle.Status = VehicleStatus.Available;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync("Lease", lease.Id, "LeaseEnded",
                $"Lease ended for {vehicle.RegistrationNumber}. Final amount: R{lease.FinalAmount}");

        return new ServiceResult {Success = true, Message = "Lease ended successfully. Vehicle is now available."};
    }

    public async Task<ServiceResult> ExtendLeaseAsync(string leaseId, DateTime newEndDate, string userName)
    {
        var lease = await _context.VehicleLeases
            .Include(l => l.Vehicle)
            .FirstOrDefaultAsync(l => l.Id == leaseId && l.ReturnDate == null);

        if(lease == null)
            return new ServiceResult { Success = false, Message = "Active lease not found."}; 

        if(newEndDate <= lease.LeaseEndDate)
            return new ServiceResult { Success = false, Message = "New end date must be after current end date."};

        var newMonthlyRate = lease.Vehicle!.DailyRate * 30.4167m;

        await _context.SaveChangesAsync();
        lease.ExtendLease(newEndDate, newMonthlyRate);           

        await _auditService.LogAsync("Lease", lease.Id, "LeaseExtended",
                $"Lease for {lease.Vehicle?.RegistrationNumber} extended to {newEndDate:dd MMM yyyy} by {userName}");

        return new ServiceResult {Success = true, Message = "Lease extended successfully."};
    }

    public async Task<bool> CanLeaseToClientAsync(string clientId, decimal newMonthlyRate)
    {
        var client = await _context.Clients
            .Include(v => v.Leases)
            .Where(c => c.CreditLimit > newMonthlyRate)
            .FirstOrDefaultAsync();

        if(client == null)
            return false;

        if(client.CreditLimit < newMonthlyRate)
            return false;
                    
        return true;
    }
}