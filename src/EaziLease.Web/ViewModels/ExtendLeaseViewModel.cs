namespace EaziLease.Web.ViewModels
{
    public class ExtendLeaseViewModel
    {
        public string LeaseId { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime CurrentEndDate { get; set; }
        public decimal DailyRate { get; set; }
    }
}