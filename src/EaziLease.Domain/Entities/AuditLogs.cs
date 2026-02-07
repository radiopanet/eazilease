using EaziLease.Domain.Entitiess.Entities;

namespace EaziLease.Domain.Entities
{
    public class AuditLogs : BaseEntity
    {
        public string EntityType { get; set; } = string.Empty; //e.g Vehicle, Supplier
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
    }
}