// Models/ViewModels/FinanceDashboardViewModel.cs
namespace EaziLease.Models.ViewModels
{
    public class FinanceDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalPenalties { get; set; }
        public decimal TotalBillableMaintenance { get; set; }
        public decimal TotalCosts { get; set; } // Non-billable costs (maintenance + pro-rata depreciation)

        // Net Profit = (Revenue + Penalties + Billable) - Costs
        public decimal NetProfit => (TotalRevenue + TotalPenalties + TotalBillableMaintenance) - TotalCosts;

        // ROI Calculation
        public decimal RoiPercentage 
        {
            get 
            {
                if (TotalCosts <= 0) return 0;
                return (NetProfit / TotalCosts) * 100;
            }
        }

        public List<LeaseFinacialSummary> RecentSummaries { get; set; } = new();
    }
}