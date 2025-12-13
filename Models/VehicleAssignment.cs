using EaziLease.Models.Entities;

namespace EaziLease.Models
{
    public class VehicleAssignment : BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public virtual Vehicle? Vehicle { get; set; }

    public string DriverId { get; set; } = string.Empty;
    public virtual Driver? Driver { get; set; }

    public DateOnly AssignedDate { get; set; }
    public DateOnly? ReturnedDate { get; set; }
    public bool IsCurrent => ReturnedDate == null;
}
}