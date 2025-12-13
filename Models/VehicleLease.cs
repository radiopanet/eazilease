using EaziLease.Models.Entities;

namespace EaziLease.Models
{
    public class VehicleLease : BaseEntity
    {
        public string VehicleId { get; set; } = string.Empty;
        public virtual Vehicle? Vehicle { get; set; }

        public string ClientId { get; set; } = string.Empty;
        public virtual Client? Client { get; set; }

        // UPDATED: DateOnly → DateTime
        public DateTime LeaseStartDate { get; set; }
        public DateTime LeaseEndDate { get; set; }

        public decimal MonthlyRate { get; set; }

        public int TermInMonths =>
            ((LeaseEndDate.Year - LeaseStartDate.Year) * 12) +
            (LeaseEndDate.Month - LeaseStartDate.Month);

        public bool IsActive => DateTime.Today <= LeaseEndDate;

        public decimal? ReturnOdometer { get; set; }
        public string? ReturnConditionNotes { get; set; }
    }
}
