using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace EaziLease.Domain.Entities
{
    public class VehicleLease : BaseEntity
    {
        public string VehicleId { get; set; } = string.Empty;
        public virtual Vehicle? Vehicle { get; set; }

        public string ClientId { get; set; } = string.Empty;
        public virtual Client? Client { get; set; }

       
        public DateTime LeaseStartDate { get; set; } = 
            DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);


        public DateTime LeaseEndDate { get; set; } = 
            DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

        public decimal MonthlyRate {get; set;}

        public int TermInMonths =>
            ((LeaseEndDate.Year - LeaseStartDate.Year) * 12) +
            (LeaseEndDate.Month - LeaseStartDate.Month);


        public DateTime? ReturnDate {get; set;}
        public decimal? PenaltyFee {get; set;}
        public decimal? FinalAmount {get; set;}

        public LeaseStatus Status {get; set;} = LeaseStatus.Active;
        public bool IsActive => ReturnDate == null;

        public decimal ReturnOdometer { get; set; }

        public TerminationReason TerminationReason {get; set;} = TerminationReason.NormalEnd;
        public string? TerminationNotes {get; set;}
        
        public string? ReturnConditionNotes { get; set; }

        public bool IsExtended {get; set;}
        public int ExtensionCount {get; set;}
        public bool IncludeDriver {get; set;} = false; //Default: client provides driver
        public decimal? DriverFee {get; set;} //Optional extra fee for EaziLease driver
        public string? AssignedDriverId {get; set;} // FK to Driver (only if IncludeDriver = true)
        public virtual Driver? Driver {get; set;}
        public virtual ICollection<VehicleMaintenance> MaintenanceHistory { get; set; } = new List<VehicleMaintenance>();


        [NotMapped]
        public decimal? BillableMaintenanceCosts => MaintenanceHistory?
            .Where(m => m.IsBillableToClient)
            .Sum(m => m.BillableAmount ?? 0) ?? 0;

        public void CalculateMonthlyRate(decimal dailyRate)
        {
            const int DAYS_IN_MONTH = 30;
            MonthlyRate = dailyRate * DAYS_IN_MONTH;
        }

        public decimal CalculateProRataAmount(decimal dailyRate)
        {
            if(ReturnDate == null)
                return MonthlyRate;

            var start = LeaseStartDate.Date;
            var end = ReturnDate.Value.Date;

            var daysUsed = (end - start).Days + 1;

            if(daysUsed < 0) daysUsed = 0;

            return dailyRate * daysUsed;
        }

        public void ExtendLease(DateTime newEndDate, decimal newMonthlyRate)
        {
            newEndDate = DateTime.SpecifyKind(newEndDate, DateTimeKind.Utc);
            if(!IsActive)
                throw new InvalidOperationException("Only active leases can be extended.");

            if(newEndDate <= LeaseEndDate)
                throw new ArgumentException("New end date must be later than current end date.");

            LeaseEndDate = newEndDate;
            MonthlyRate = newMonthlyRate;
            IsExtended = true;
            ExtensionCount++;  
        }
    }
}
