using EaziLease.Domain.Entities;

namespace EaziLease.Web.ViewModels
{
    public class ClientDashboardViewModel
    {
        public string CompanyName {get; set;} = string.Empty;
        public int ActiveLeasesCount {get; set;}
        public int TotalVehiclesLeased {get; set;}
        public List<VehicleMaintenance>? UpcomingMaintenance {get; set;}
        public decimal NextInvoiceAmount {get; set;}
    }
}