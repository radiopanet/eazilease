using EaziLease.Infrastructure.Persistence;
using EaziLease.Domain.Entities;

namespace EaziLease.Infrastructure.Services;
public class AuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entityType, string entityId, string action, string? details = null)
    {
        var user = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
        var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        var log = new AuditLogs
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PerformedBy = user,
            Details = details,
            IpAddress = ip
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}