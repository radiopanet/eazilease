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
        public decimal? OdometerReading { get; set; } //in km
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;
        public decimal DailyRate { get; set; }
        public DateTime? LastServiceDate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public decimal? PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        //Maitenance scheduling intervals (configurable per vehicle).
        public int MaintenanceIntervalKm {get; set;} = 10000; //default per 10 000 kilometers
        public int MaintenanceIntervalMonths {get; set;} = 6; //default to 6 months.

        //Next due calculations (updated on completion)
        public decimal? NextMaintenanceMileage {get; set;} //e.g 90 000Km, nullable meaning no upcoming maintenance scheduled.
        public DateTime? NextMaintenanceDate {get; set;} = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc); //e.g 2026-07-11 

        [NotMapped]
        public decimal TotalBillableMaintenance
        {
            get
            {
                if(CurrentLease == null) return 0m;
                return CurrentLease.BillableMaintenanceCosts ?? 0m;
            }
        }

        /// <summary>
        /// If true, SuperAdmin has manually overridden the high-maintenance auto-rate increase
        /// </summary>
        public bool OverrideHighMaintenanceRate {get; set;} = false;

        /// <summary>
        /// If true, SuperAdmin has manually allowed leasing despite high-maintenance block
        /// </summary>
        public bool OverrideHighMaintenanceBlock {get; set;} = false;

        /// <summary>
        /// Optional notes, explaining the overrides
        /// </summary>
        public string? OverrideNotes {get; set;}
        

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