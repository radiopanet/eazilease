using EaziLease.Models.Entities;

namespace EaziLease.Models
{
    public class Branch : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // e.g., JHB001
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? ManagerId { get; set; }

        // Navigation
        public virtual ApplicationUser? Manager { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; } = default!;
    }
}
