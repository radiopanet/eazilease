using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EaziLease.Models
{
    public class Driver : BaseEntity
    {
        [Display(Name ="First Name")]
        public string FirstName { get; set; } = string.Empty;
        [Display(Name ="Last Name")]
        public string LastName { get; set; } = string.Empty;
        [Display(Name ="Full Name")]
        public string FullName => $"{FirstName} {LastName}";
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; } = string.Empty;
        [Display(Name ="License Expiry Date")]
        public DateTime? LicenseExpiry { get; set; } =
            DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        [Display(Name ="Phone Number")]    
        public string Phone { get; set; } = string.Empty;
        [Display(Name ="Email Address")]
        public string? Email { get; set; }
        [Display(Name ="Accident Count")]
        public int AccidentCount { get; set; } = 0;
        [Display(Name ="Is Driver Active?")]
        public bool IsActive { get; set; } = true;

        // Current assignment (one driver → one vehicle at a time)

        public string? CurrentVehicleId { get; set; }
        [ForeignKey(nameof(CurrentVehicleId))]
        [InverseProperty("CurrentDriver")]
        public virtual Vehicle? CurrentVehicle { get; set; }

        // History
        public virtual ICollection<VehicleAssignment>? AssignmentHistory { get; set; } = null;
        public bool IsCurrentlyAssigned => !string.IsNullOrEmpty(CurrentVehicleId);
    }
}