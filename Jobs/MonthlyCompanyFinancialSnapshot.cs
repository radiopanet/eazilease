using EaziLease.Models.Entities;
using EaziLease.Models;
using EaziLease.Data;
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
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            var start = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
            var end = start.AddMonths(1).AddDays(-1);

            var endedLeases = await _context.VehicleLeases
                .Where(l => l.Status == LeaseStatus.Completed && l.ReturnDate <= end)
                .ToListAsync();

            decimal revenue = endedLeases.Sum(l => l.FinalAmount ?? 0);
            decimal penalties = endedLeases.Sum(l => l.PenaltyFee ?? 0);
            decimal billable = endedLeases.Sum(l => l.BillableMaintenanceCosts ?? 0);
            decimal costs = endedLeases.Sum(l => l.CalculateProRataAmount(l.Vehicle.DailyRate));

            var snapshot = new CompanyFinancialSnapshot
            {
                PeriodStart = DateTime.SpecifyKind(start, DateTimeKind.Utc),
                PeriodEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc),
                TotalRevenue = revenue,
                TotalPenalties = penalties,
                TotalBillableMaintenance = billable,
                TotalNonBillableCosts = costs,
                ActiveLeaseCount = await _context.VehicleLeases.CountAsync(l => l.Status == LeaseStatus.Active),
                EndedLeaseCount = endedLeases.Count
            };

            _context.CompanyFinancialSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
        }
    }
}