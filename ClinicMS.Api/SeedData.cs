using ClinicMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicMS.Api
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // ── Seed Roles ────────────────────────────────────
            // Creates Admin, Receptionist, Doctor roles in AspNetRoles table
            await EnsureRoleAsync(roleManager, "Admin");
            await EnsureRoleAsync(roleManager, "Receptionist");
            await EnsureRoleAsync(roleManager, "Doctor");

            // ── Seed Admin User ───────────────────────────────
            // Creates one default admin account for log in
            const string adminEmail = "admin@clinicms.com";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                // User doesn't exist yet — create it
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    IsActive = true,
                    EmailConfirmed = true  // skip email confirmation for dev
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    // Assign Admin role to this user
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            // ── Seed Receptionist User ────────────────────────
            const string receptionistEmail = "receptionist@clinicms.com";
            const string receptionistPassword = "Reception@123";

            var receptionistUser = await userManager.FindByEmailAsync(receptionistEmail);
            if (receptionistUser == null)
            {
                receptionistUser = new ApplicationUser
                {
                    UserName = receptionistEmail,
                    Email = receptionistEmail,
                    FullName = "Front Desk Receptionist",
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(receptionistUser, receptionistPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(receptionistUser, "Receptionist");
                }
            }
        }

        // Private helper — checks if role exists, creates if not
        private static async Task EnsureRoleAsync(
            RoleManager<IdentityRole> roleManager,
            string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}