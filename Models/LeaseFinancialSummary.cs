using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace EaziLease.Models
{
    //Calculated per lease – persisted on lease end
    public class LeaseFinacialSummary: BaseEntity
    {
        [Required]
        public string LeaseId {get; set;} = string.Empty;
        public virtual VehicleLease? Lease {get; set;}
        public decimal TotalLeaseRevenue {get; set;} //Sum of monthly payments + pro rata
        public decimal TotalPenaltyFees {get; set;}
        public decimal TotalBillableMaitenance {get; set;} //From maitenance records.
        public decimal TotalCost {get; set;} //Non-Billable maintenance + other fees
        public decimal NetProfit => TotalLeaseRevenue + TotalPenaltyFees + TotalBillableMaitenance - TotalCost;
        public decimal RoiPercentage => TotalCost > 0 ? (NetProfit / TotalCost) * 100m : 0m;
        public DateTime CalculatedAt {get; set;} = DateTime.UtcNow;
        public string CalculateBy {get; set;} = string.Empty;
    }
}