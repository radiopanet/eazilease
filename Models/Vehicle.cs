using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EaziLease.Models
{
    public class Vehicle: BaseEntity
    {
        public string VIN {get; set;} = string.Empty; //Unique
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty; //e.g Toyota
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public FuelType FuelType { get; set; }
        public TransmissionType Transmission { get; set; }
        public decimal OdometerReading { get; set; } //in km
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public decimal DailyRate { get; set; }
        public DateTime? LastServiceDate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public decimal? PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        //Relationships
        public string SupplierId { get; set; } = string.Empty;
        public virtual Supplier? Supplier { get; set; }

        public string BranchId { get; set; } = string.Empty;

        public virtual Branch? Branch { get; set; }

        //Current Lease (if any)
        public string? CurrentLeaseId { get; set; }
        public virtual VehicleLease? CurrentLease { get; set; }

        //Current Driver
        public string? CurrentDriverId { get; set; }
        
        [InverseProperty("CurrentVehicle")]
        public virtual Driver? CurrentDriver { get; set; }

        //History
        public virtual ICollection<VehicleLease> LeaseHistory { get; set; } = new List<VehicleLease>();
        public virtual ICollection<VehicleAssignment> AssignementHistory { get; set; } = new List<VehicleAssignment>();
        public ICollection<VehicleMaintenance> MaintenanceHistory {get; set; } = new List<VehicleMaintenance>();
    }
}