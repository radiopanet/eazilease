using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EaziLease.Models
{
    public class Driver : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string LicenseNumber { get; set; } = string.Empty;
        public DateOnly LicenseExpiry { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int AccidentCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Current assignment (one driver → one vehicle at a time)

        public string? CurrentVehicleId { get; set; }
        [ForeignKey(nameof(CurrentVehicleId))]
        [InverseProperty("CurrentDriver")]
        public virtual Vehicle? CurrentVehicle { get; set; }

        // History
        public virtual ICollection<VehicleAssignment> AssignmentHistory { get; set; } = default!;
        public bool IsCurrentlyAssigned => !string.IsNullOrEmpty(CurrentVehicleId);
    }
}