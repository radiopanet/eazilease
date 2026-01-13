using EaziLease.Models.Entities;

namespace EaziLease.Models;

public class VehicleMaintenance: BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public Vehicle? Vehicle { get; set; } = null!;
    public DateTime ServiceDate { get; set; }
    public string Description { get; set; } = string.Empty; // e.g. "Oil change + filter", "Brake pads replacement"
    public string? GarageId { get; set; }
    public Garage? Garage {get; set;}
    public decimal? Cost { get; set; } = 0.0m;
    public decimal? MileageAtService { get; set; } // km at time of service
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsWarrantyWork { get; set; } = false;
    public MaintenanceStatus Status {get; set;} = MaintenanceStatus.Scheduled;
    public MaintenanceType Type = MaintenanceType.Routine;

    public DateTime? ScheduledDate {get; set;}
    public decimal? ScheduledMileage {get; set;}

    //Flag to distiguish planning/actual
    public bool IsFutureScheduled {get; set;}
    public bool AffectsAvailability { get; set; } = false;

}