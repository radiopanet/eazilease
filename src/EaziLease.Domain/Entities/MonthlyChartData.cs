namespace EaziLease.Domain.Entities
{
    public class MonthlyChartData
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
    }
}