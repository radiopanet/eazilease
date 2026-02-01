namespace EaziLease.Domain.Entities
{

    public class RateOverrideRequest : BaseEntity
    {
        public string VehicleId { get; set; } = string.Empty;
        public virtual Vehicle? Vehicle { get; set; }

        public decimal RequestedDailyRate { get; set; }
        public decimal OriginalDailyRate { get; set; }

        public bool IsPermanent { get; set; } = false;
        public DateTime? EffectiveFrom { get; set; } // for temporary
        public DateTime? EffectiveTo { get; set; }   // for temporary
        public string RequestedBy { get; set; } = string.Empty; // User.Identity.Name
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; } // mandatory
        public string? ApprovedBy { get; set; } // SuperAdmin who approved/rejected
        public DateTime? ApprovedAt { get; set; }
        public bool? IsApproved { get; set; } // null = pending, true = approved, false = rejected
        public string? ApprovalNotes { get; set; } // optional rejection reason
    }
}