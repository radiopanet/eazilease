namespace EaziLease.Domain.Entities
{
    //Calculated monthly/quarterly aggregates – persisted by job
    public class CompanyFinancialSnapshot: BaseEntity
    {
        public DateTime PeriodStart {get; set;} //e.g first day of the month
        public DateTime PeriodEnd {get; set;} //e.g last day of the month

        public decimal TotalRevenue {get; set;}
        public decimal TotalPenalties {get; set;}
        public decimal TotalBillableMaintenance {get; set;}
        public decimal TotalNonBillableCosts {get; set;}
        public decimal NetProfit => TotalRevenue + TotalPenalties+ TotalBillableMaintenance - TotalNonBillableCosts; 

        //Lease
        public int ActiveLeaseCount {get; set;}
        public int EndedLeaseCount {get; set;}

        public string PeriodName => $"{PeriodStart: MMM yyyy}";
    }
}