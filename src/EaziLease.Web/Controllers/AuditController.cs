using EaziLease.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EaziLease.Web.Controllers
{
    [Authorize(Policy = "RequireSuperAdmin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? entityType = null, string? entityId = null)
        {
            var logs = _context.AuditLogs
                 .Where(l => (entityType == null || l.EntityType == entityType) &&
                             (entityId == null || l.EntityId == entityId))
                 .OrderByDescending(l => l.PerformedAt);

            return View(await logs.ToListAsync());
        }
    }
}