using EaziLease.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EaziLease.Infrastructure.Persistence
{
    public static class IdentitySeedData
    {
        private const string AdminEmail = "admin@eazilease.com";
        private const string AdminPassword = "Admin@123";
        private const string AdminRole = "Admin";

        public static async Task Seed(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();


            //Create Admin Role
            if(!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }

            const string SuperCandidateRole = "SuperAdminCandidate";
            if(!await roleManager.RoleExistsAsync(SuperCandidateRole))
                await roleManager.CreateAsync(new IdentityRole(SuperCandidateRole));

            if(!await roleManager.RoleExistsAsync("ClientUser"))
            {
                await roleManager.CreateAsync(new IdentityRole("ClientUser"));
            }

            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if(adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                }

                
            }
            else
            {
                //Ensure existing user has admin roles
                if(!await userManager.IsInRoleAsync(adminUser, AdminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                }
                if (!await userManager.IsInRoleAsync(adminUser, SuperCandidateRole))
                    await userManager.AddToRoleAsync(adminUser, SuperCandidateRole);
            }
        }
    }
}