using EaziLease.Models;
using Microsoft.AspNetCore.Identity;

namespace EaziLease.Data
{
    public static class ClientSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Roles
            string[] roles = { "Admin", "SuperAdmin", "ClientUser" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Test client user
            var clientEmail = "client@demo.com";
            var client = await userManager.FindByEmailAsync(clientEmail);
            if (client == null)
            {
                client = new ApplicationUser { UserName = clientEmail, Email = clientEmail, EmailConfirmed = true };
                await userManager.CreateAsync(client, "ClientPass123!");
                await userManager.AddToRoleAsync(client, "ClientUser");
            }
        }
    }
}