using EaziLease.Data;
using EaziLease.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EaziLease.Controllers
{
    public class FinanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinanceController(ApplicationDbContext context)
        {
            _context = context;
        }
        // Controllers/FinanceController.cs
        public async Task<IActionResult> Index()
        {
            // Get the most recent monthly snapshot
            var latestSnapshot = await _context.CompanyFinancialSnapshots
                .OrderByDescending(s => s.PeriodEnd)
                .FirstOrDefaultAsync();

            // Get the individual lease summaries for the table
            var recentSummaries = await _context.LeaseFinancialSummaries
                .Include(s => s.Lease)
                    .ThenInclude(l => l.Client)
                .Include(s => s.Lease)
                    .ThenInclude(l => l.Vehicle)
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Map to a ViewModel
            var viewModel = new FinanceDashboardViewModel
            {
                TotalRevenue = latestSnapshot?.TotalRevenue ?? 0,
                TotalCosts = latestSnapshot?.TotalNonBillableCosts ?? 0,
                RecentSummaries = recentSummaries
            };

            return View(viewModel);
        }
    }
}