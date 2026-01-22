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
        [DataType(DataType.DateTime)]
        public DateTime? LastServiceDate { get; set; } 
        public decimal? PurchasePrice { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime PurchaseDate { get; set; } 

        //Maitenance scheduling intervals (configurable per vehicle).
        public int MaintenanceIntervalKm {get; set;} = 10000; //default per 10 000 kilometers
        public int MaintenanceIntervalMonths {get; set;} = 6; //default to 6 months.

        //Next due calculations (updated on completion)
        public decimal? NextMaintenanceMileage {get; set;} //e.g 90 000Km, nullable meaning no upcoming maintenance scheduled.
        [DataType(DataType.DateTime)]
        public DateTime? NextMaintenanceDate {get; set;} 

        [NotMapped]
        public decimal TotalBillableMaintenance
        {
            get
            {
                if(CurrentLease == null) return 0m;
                return CurrentLease.BillableMaintenanceCosts ?? 0m;
            }
        }

        [NotMapped]
        public decimal MaintenanceScore
        {
            get
            {
                if (OdometerReading <= 0 || PurchasePrice <= 0) return 0m;

                var completed = MaintenanceHistory
                    .Where(m => m.Status == MaintenanceStatus.Completed)
                    .ToList();

                if (!completed.Any()) return 0m;

                decimal totalCost = completed.Sum(m => m.Cost ?? 0);
                int repairCount = completed.Count(m => m.Type == MaintenanceType.Repair);

                decimal costFactor = (totalCost / PurchasePrice) * 10m ?? 0;
                decimal freqFactor = (repairCount * 10000m) / OdometerReading ?? 0;

                return Math.Min(10m, Math.Round(costFactor + freqFactor, 1));
            }
        }

        [NotMapped]
        public bool IsHighMaintenance => MaintenanceScore >= 7.0m;

        [NotMapped]
        public decimal AdjustedDailyRate
        {
            get
            {
                if (OverrideHighMaintenanceRate) return DailyRate; // SuperAdmin override
                return IsHighMaintenance ? DailyRate * 1.20m : DailyRate;
            }
        }


        /// <summary>
        /// If true, SuperAdmin has manually overridden the high-maintenance auto-rate increase
        /// </summary>
        public bool OverrideHighMaintenanceRate {get; set;} = false;

        /// <summary>
        /// If true, SuperAdmin has manually allowed leasing despite high-maintenance block
        /// </summary>
        public bool OverrideLeasingBlock {get; set;} = false;

        /// <summary>
        /// Optional notes, explaining the overrides
        /// </summary>
        public string? OverrideRateNotes {get; set;}
        

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