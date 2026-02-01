namespace EaziLease.Domain.Entities
{
    public class VehicleAssignment : BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public virtual Vehicle? Vehicle { get; set; }

    public string DriverId { get; set; } = string.Empty;
    public virtual Driver? Driver { get; set; }

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ReturnedDate { get; set; }
    public bool IsCurrent => ReturnedDate == null;
}
}