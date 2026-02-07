using EaziLease.Infrastructure.Persistence;
using EaziLease.Services;
using Microsoft.AspNetCore.Mvc;
using EaziLease.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Localization;

namespace EaziLease.Web.Controllers
{
    [Authorize]
    public class ClientsController: Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ClientsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var clients =  _context.Clients
                    .Include(vl => vl.Leases)
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();

            return View(await clients);        
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if(ModelState.IsValid)
            {
                client.CreatedBy = User.Identity!.Name ?? "admin";
                _context.Add(client);
                await _context.SaveChangesAsync();
                TempData["success"] = "Client added";
                await _auditService.LogAsync("Client", client.Id, "Create",
                    $"{client.CompanyName} added by {client.CreatedBy} at {client.CreatedAt}.");
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var client = await _context.Clients.FindAsync(id);
            if(client == null) return NotFound();

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Client client)
        {
            if(id != client.Id) return NotFound();
            if(ModelState.IsValid)
            {
                var existing = await _context.Clients.FindAsync(id);
                existing!.RegistrationNumber = client.RegistrationNumber;
                existing!.CompanyName = client.CompanyName;
                existing.ContactPerson = client.ContactPerson;
                existing.ContactPhone = client.ContactPhone;
                existing.ContactEmail = client.ContactEmail;
                existing.CreditLimit = client.CreditLimit;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = User.Identity!.Name ?? "admin";

                await _context.SaveChangesAsync();
                TempData["success"] = "Client Added";
                await _auditService.LogAsync("Client", client.Id, "Edit",
                    $"{client.CompanyName} updated by {existing.UpdatedBy} at {existing.UpdatedAt}");

                return RedirectToAction(nameof(Index));    

            }

            return View(client);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var client = await _context.Clients.FindAsync(id);
            if(client == null || client.IsDeleted) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var client = await _context.Clients.FindAsync(id);
            if(client != null && !client.IsDeleted)
            {
                client.IsDeleted = true;
                client.DeletedAt = DateTime.UtcNow;
                client.DeletedBy = User.Identity!.Name;
                await _context.SaveChangesAsync();
                TempData["success"] = "Client deleted.";
                await _auditService.LogAsync("Client", client.Id, "Delete",
                    $"{client.CompanyName} deleted by {client.DeletedBy} at {client.DeletedAt}");

            }
            return RedirectToAction(nameof(Index));                
        }

    }
}