// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using EaziLease.Data;
// using EaziLease.Models;
// using Microsoft.AspNetCore.Mvc.Rendering;

// namespace EaziLease.Controllers
// {
//     [Authorize]
//     public class DriversController : Controller
//     {
//         private readonly ApplicationDbContext _context;

//         public SuppliersController(ApplicationDbContext context) => _context = context;

//         public async Task<IActionResult> Index() =>
//             View(await _context.Drivers.Where(s => !s.IsDeleted).OrderBy(s => s.Name).ToListAsync());

//         public IActionResult Create() => View();

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create(Driver driver)
//         {
//             if (ModelState.IsValid)
//             {
//                 driver.CreatedBy = User.Identity!.Name ?? "admin";
//                 _context.Add(driver);
//                 await _context.SaveChangesAsync();
//                 TempData["success"] = "Driver added";
//                 return RedirectToAction(nameof(Index));
//             }
//             return View(driver);
//         }

//         public async Task<IActionResult> Edit(string id)
//         {
//             var driver = await _context.Drivers.FindAsync(id);
//             if (driver == null || driver.IsDeleted) return NotFound();
//             return View(driver);
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Edit(string id, Driver driver)
//         {
//             if (id != driver.Id) return NotFound();
//             if (ModelState.IsValid)
//             {
//                 var existing = await _context.Drivers.FindAsync(id);
//                 existing!.Firs = supplier.Name;
//                 existing.ContactPerson = supplier.ContactPerson;
//                 existing.ContactEmail = supplier.ContactEmail;
//                 existing.ContactPhone = supplier.ContactPhone;
//                 existing.Address = supplier.Address;
//                 existing.City = supplier.City;
//                 existing.Country = supplier.Country;
//                 existing.UpdatedAt = DateTime.UtcNow;
//                 existing.UpdatedBy = User.Identity!.Name ?? "admin";

//                 await _context.SaveChangesAsync();
//                 TempData["success"] = "Driver updated";
//                 return RedirectToAction(nameof(Index));
//             }
//             return View(drivers);
//         }

//         [HttpPost, ActionName("Delete")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> DeleteConfirmed(string id)
//         {
//             var driver = await _context.Drivers.FindAsync(id);
//             if (driver != null && !driver.IsDeleted)
//             {
//                 driver.IsDeleted = true;
//                 driver.DeletedAt = DateTime.UtcNow;
//                 driver.DeletedBy = User.Identity!.Name;
//                 await _context.SaveChangesAsync();
//                 TempData["success"] = "Driver deleted";
//             }
//             return RedirectToAction(nameof(Index));
//         }
//     }
// }