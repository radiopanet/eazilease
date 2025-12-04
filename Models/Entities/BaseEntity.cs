namespace EaziLease.Models.Entities
{
    public abstract class BaseEntity
    {
        public string Id {get; set;} = Guid.NewGuid().ToString();
        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
        public string CreatedBy {get; set;} = "system";
        public DateTime? UpdatedAt {get; set;}
        public string? UpdatedBy {get; set;}
        public bool IsDeleted {get; set;}
        public DateTime? DeletedAt {get; set;}
        public string? DeletedBy {get; set;}
    }
}