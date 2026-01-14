using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;


namespace EaziLease.Models
{
    public class VehicleUsageSnapshot: BaseEntity
    {
        [Required]
        public string VehicleId {get; set;} = string.Empty;
        public virtual Vehicle? Vehicle {get; set;}

        /// <summary>
        /// The period this snapshot represents (e.g. end of month, quarter, or event-based)
        /// </summary>
        
        [Required]
        public DateTime SnapshotDate {get; set;} = DateTime.UtcNow;
        
        ///<summary>
        /// Name of the period (e.g. "2025-Q4", "Lease End 2025-12-31")
        /// </summary>
        [MaxLength(100)]
        public string? PeriodName {get; set;}

        //Core Metrics - persist for historical accuracy
        public decimal MaintenanceScore {get; set;} //0-10 (higher = worse)
        public decimal TotalMaintenanceCost {get; set;}
        public decimal TotalKmDriven {get; set;}
        public decimal CostPerKm => TotalKmDriven > 0 ? TotalMaintenanceCost / TotalKmDriven: 0m;
        int TotalMaintenanceRecords {get; set;}
        int RepairRecordsCount {get; set;} //Breakdown frequency
        public decimal BreakdownFrequencyPer10kKm => TotalKmDriven > 0 ?
            (RepairRecordsCount * 10000m) / TotalKmDriven : 0m;

        //Additional flags for quick decisions
        public bool IsMaintenanceHigh => MaintenanceScore >= 7.0m;

        public bool IsRetirementCandidate => TotalMaintenanceCost > (Vehicle?.PurchasePrice * 0.5m ?? 0m);

        //Audit trail - keep for now as the system will change over time
        public string? CalculatedBy {get; set;}
        public string? TriggerEvent {get; set;} //e.g "LeaseEnd", "MonthlySnapshot" etc
    }
}