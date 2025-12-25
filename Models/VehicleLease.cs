using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace EaziLease.Models
{
    public class VehicleLease : BaseEntity
    {
        public string VehicleId { get; set; } = string.Empty;
        public virtual Vehicle? Vehicle { get; set; }

        public string ClientId { get; set; } = string.Empty;
        public virtual Client? Client { get; set; }

        // UPDATED: DateOnly → DateTime
        [Required]
        [DataType(DataType.Date)]
        public DateTime LeaseStartDate { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [DataType(DataType.Date)]
        public DateTime LeaseEndDate { get; set; } = DateTime.UtcNow.Date;

        public decimal MonthlyRate { get; set; }

        public int TermInMonths =>
            ((LeaseEndDate.Year - LeaseStartDate.Year) * 12) +
            (LeaseEndDate.Month - LeaseStartDate.Month);

        public bool IsActive => DateTime.UtcNow.Date <= LeaseEndDate.Date;

        public decimal? ReturnOdometer { get; set; }
        public string? ReturnConditionNotes { get; set; }
    }
}
