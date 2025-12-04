using EaziLease.Models.Entities;

namespace EaziLease.Models
{
    public class Client: BaseEntity
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? CreditLimit { get; set; }

        //Navigation
        public virtual ICollection<VehicleLease> Leases {get; set;} = default!;
        public int ActiveLeaseCount => Leases?.Count(l => l.IsActive) ?? 0;
    }
}