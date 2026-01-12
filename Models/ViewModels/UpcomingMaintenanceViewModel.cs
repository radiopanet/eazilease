
namespace EaziLease.Models.ViewModels
{
    public class UpcomingMaintenanceViewModel
    {
        public string VehicleId {get; set;} = string.Empty;
        public string? RegistrationNumber {get; set;} = string.Empty;
        public DateTime? NextMaintenanceDate {get; set;}
        public int? NextMaintenanceMileage {get; set;}
        public int DaysRemaining {get; set;}
        public int? KmRemaining {get; set;}
        public MaintenanceType Type {get; set;}
        public string StatusDisplay => DaysRemaining < 0 ? "Overdue" :
                                       DaysRemaining <= 7 ? "Due soon" : "Upcoming";
        public string StatusColor =>   DaysRemaining < 0 ? "danger":
                                       DaysRemaining <= 7 ? "warning" : "success";

    }
}