using Microsoft.AspNetCore.Mvc;
using EaziLease.Data;
using EaziLease.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EaziLease.Models.ViewModels;

namespace EaziLease.Areas.Client
{
    [Area("Portal")]
    [Authorize(Policy = "ClientOnly")]
    public class PortalController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public PortalController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var client = await _context.Clients
                    .Include(c => c.Leases)
                    .ThenInclude(l => l.Vehicle)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (client == null)
                return NotFound("No client profile found for this user.");

            var model = new ClientDashboardViewModel
            {
                CompanyName = client.CompanyName,
                ActiveLeasesCount = client.Leases.Count(l => l.Status == LeaseStatus.Active),
                TotalVehiclesLeased = client.Leases.Select(l => l.VehicleId).Distinct().Count()
            };


            return View(model);
        }
    }
}