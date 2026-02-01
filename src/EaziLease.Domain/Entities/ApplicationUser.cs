using Microsoft.AspNetCore.Identity;

namespace EaziLease.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName {get; set;} = string.Empty;
        public string? BranchId {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    }
}