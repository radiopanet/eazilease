using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace EaziLease.Models
{
    public class Client: BaseEntity
    {
        [Display(Name="Company Name")]
        public string CompanyName { get; set; } = string.Empty;
        [Display(Name="Registration Number")]
        public string? RegistrationNumber { get; set; }
        [Display(Name="Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;
        [Display(Name="Email Address")]
        public string ContactEmail { get; set; } = string.Empty;
        [Display(Name="Phone Number")]
        public string ContactPhone { get; set; } = string.Empty;
        [Display(Name ="Credit Limit")]
        public string? CreditLimit { get; set; }

        //Navigation
        public virtual ICollection<VehicleLease>? Leases {get; set;} = default!;
        public int ActiveLeaseCount => Leases?.Count(l => l.IsActive) ?? 0;
       
    }
}