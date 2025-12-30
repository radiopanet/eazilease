using EaziLease.Models.Entities;

namespace EaziLease.Models;

public class VehicleMaintenance: BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public Vehicle? Vehicle { get; set; } = null!;
    public DateTime ServiceDate { get; set; }
    public string Description { get; set; } = string.Empty; // e.g. "Oil change + filter", "Brake pads replacement"
    public string? GarageName { get; set; }
    public decimal Cost { get; set; }
    public int? MileageAtService { get; set; } // km at time of service
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsWarrantyWork { get; set; } = false;
}