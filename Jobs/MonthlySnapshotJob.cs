using EaziLease.Services.Interfaces;
using EaziLease.Data;
using Microsoft.EntityFrameworkCore;

namespace EaziLease.Jobs
{
    public class MonthlySnapshotJob
    {
        private ApplicationDbContext _context;
        private readonly IVehicleService _vehicleService;

        public MonthlySnapshotJob(ApplicationDbContext context, IVehicleService vehicleService)
        {
            _context = context;
            _vehicleService = vehicleService;
        }
       

        public async Task Execute()
        {
            var vehicles = await _context.Vehicles.Where(v => !v.IsDeleted).ToListAsync();

            foreach (var v in vehicles)
            {
                await _vehicleService.CreateUsageSnapshotAsync(v.Id, "MonthlySnapshot", "System");
            }
        }

    }
}