using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


using System.ComponentModel.DataAnnotations;
using EaziLease.Data;
using Microsoft.EntityFrameworkCore;

[Area("SuperAdmin")]
[Authorize(Policy = "RequireSuperAdmin")]
public class RateOverrideController : Controller
{
    private readonly ApplicationDbContext _context;

    public RateOverrideController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _context.RateOverrideRequests
            .Include(r => r.Vehicle)
            .Where(r => r.IsApproved == null) // pending only
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id, string? notes)
    {
        var request = await _context.RateOverrideRequests
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return NotFound();

        request.IsApproved = true;
        request.ApprovedBy = User.Identity?.Name;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovalNotes = notes;

        // Apply override
        if (request.IsPermanent)
        {
            request.Vehicle!.DailyRate = request.RequestedDailyRate;
        }
        // For temporary: you'd need logic to apply/revert based on dates (future feature)

        await _context.SaveChangesAsync();

        TempData["success"] = "Rate override approved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string id, string? notes)
    {
        var request = await _context.RateOverrideRequests.FirstOrDefaultAsync(r => r.Id == id);
        if (request == null) return NotFound();

        request.IsApproved = false;
        request.ApprovedBy = User.Identity?.Name;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovalNotes = notes;

        await _context.SaveChangesAsync();

        TempData["info"] = "Rate override rejected.";
        return RedirectToAction(nameof(Index));
    }
}