using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EaziLease.Domain.Entities
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
        [Required]
        [DataType(DataType.Currency)]
        [Display(Name ="Credit Limit (R)")]
        [Range(0, double.MaxValue, ErrorMessage="Credit Limit must me non-negative.")]
        public decimal CreditLimit { get; set; } = 0m;

        [NotMapped]
        public decimal CurrentCommittedAmount
        {
            get
            {
                if(Leases == null) return 0m;
                return Leases
                            .Where(l => l.IsActive)
                            .Sum(l => l.MonthlyRate);
            }
        }

        [NotMapped]
        public bool HasAvailableCredit => CreditLimit >= CurrentCommittedAmount;

        [NotMapped]
        public decimal AvailableCredit => CreditLimit - CurrentCommittedAmount;

        //Navigation
        public virtual ICollection<VehicleLease>? Leases {get; set;} = default!;
        public int ActiveLeaseCount => Leases?.Count(l => l.IsActive) ?? 0;
        public string? UserId {get; set;}  
        public virtual ApplicationUser? User {get; set;}
       
    }
}