using EaziLease.Domain.Entities.Entities;
using EaziLease.Domain.Entities;
using EaziLease.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EaziLease.Jobs
{
    public class MonthlyCompanyFinancialSnapshot : BaseEntity
    {
        private readonly ApplicationDbContext _context;

        public MonthlyCompanyFinancialSnapshot(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateCompanySnapshot()
        {
            var now = DateTime.UtcNow;
            // Ensure we are looking at the current month's bounds in UTC
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);

            // 1. Added parentheses for correct OR/AND logic
            // 2. Changed LeaseStatus to l.Status
            // 3. Added .Include(l => l.Vehicle)
            var endedLeases = await _context.VehicleLeases
                .Include(l => l.Vehicle)
                .Where(l => (l.Status == LeaseStatus.Terminated || l.Status == LeaseStatus.Completed)
                             && l.ReturnDate >= start
                             && l.ReturnDate <= end)
                .ToListAsync();

            // Check if we have data to prevent unnecessary snapshots if desired, 
            // but for debugging, we'll proceed.

            decimal revenue = endedLeases.Sum(l => l.FinalAmount ?? 0m);
            decimal penalties = endedLeases.Sum(l => l.PenaltyFee.GetValueOrDefault());
            decimal billable = endedLeases.Sum(l => l.BillableMaintenanceCosts ?? 0);

            // Ensure Vehicle is not null before calling pro-rata
            decimal costs = endedLeases
                .Where(l => l.Vehicle != null)
                .Sum(l => l.CalculateProRataAmount(l.Vehicle.DailyRate));

            var snapshot = new CompanyFinancialSnapshot
            {
                PeriodStart = start,
                PeriodEnd = end,
                TotalRevenue = revenue,
                TotalPenalties = penalties,
                TotalBillableMaintenance = billable,
                TotalNonBillableCosts = costs,
                ActiveLeaseCount = await _context.VehicleLeases.CountAsync(l => l.Status == LeaseStatus.Active),
                EndedLeaseCount = endedLeases.Count,
                CreatedAt = DateTime.UtcNow // Ensure you track when the snapshot was made
            };

            _context.CompanyFinancialSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
        }
    }
}