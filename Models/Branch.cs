using EaziLease.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace EaziLease.Models
{
    public class Branch : BaseEntity
    {
        [Display(Name="Branch Name")]
        public string Name { get; set; } = string.Empty;
        [Display(Name="Branch Code")]
        public string Code { get; set; } = string.Empty; // e.g., JHB001
        [Display(Name="Address")]
        public string Address { get; set; } = string.Empty;
        [Display(Name="City")]
        public string City { get; set; } = string.Empty;
        public string? ManagerId { get; set; }

        // Navigation
        public virtual ApplicationUser? Manager { get; set; }
        public virtual ICollection<Vehicle>? Vehicles { get; set; } = null;
    }
}
