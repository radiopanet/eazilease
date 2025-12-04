using EaziLease.Models.Entities;

namespace EaziLease.Models
{
   public class VehicleLease : BaseEntity
{
    public string VehicleId { get; set; } = string.Empty;
    public virtual Vehicle Vehicle { get; set; } = null!;

    public string ClientId { get; set; } = string.Empty;
    public virtual Client Client { get; set; } = null!;

    public DateOnly LeaseStartDate { get; set; }
    public DateOnly LeaseEndDate { get; set; }
    public decimal MonthlyRate { get; set; }
    public int TermInMonths => ((LeaseEndDate.Year - LeaseStartDate.Year) * 12) + LeaseEndDate.Month - LeaseStartDate.Month;
    public bool IsActive => DateOnly.FromDateTime(DateTime.Today) <= LeaseEndDate;

    // Optional return condition
    public decimal? ReturnOdometer { get; set; }
    public string? ReturnConditionNotes { get; set; }
}
}