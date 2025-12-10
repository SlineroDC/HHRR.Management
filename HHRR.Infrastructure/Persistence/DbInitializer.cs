using HHRR.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HHRR.Infrastructure.Persistence;

public class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, ApplicationDbContext context)
    {
       
        await context.Database.MigrateAsync();

        if (!context.Departments.Any())
        {
            var departments = new List<Department>
            {
                new Department { Name = "General", Description = "General" },
                new Department { Name = "Logístic", Description = "Supply chain management" },
                new Department { Name = "Marketing", Description = "Advertising and branding" },
                new Department { Name = "R.R.H.H", Description = "Human talent management" },
                new Department { Name = "Operations", Description = "Central processes" },
                new Department { Name = "Sales", Description = "Commercial strategies" },
                new Department { Name = "Technology", Description = "Systems and development" },
                new Department { Name = "Accounting", Description = "Finance and audit" }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        if (!context.Clients.Any())
        {
            var clients = new List<Client>
            {
                new Client { Name = "Acme Corp", Email = "contact@acme.com", Company = "Acme Inc." },
                new Client { Name = "Globex", Email = "info@globex.com", Company = "Globex Corporation" },
                new Client { Name = "Soylent Corp", Email = "sales@soylent.com", Company = "Soylent Corp" }
            };
            
            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();
        }

    
        // 3. Seed Roles
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 4. Seed Admin User
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = "admin@hhrr.io";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true 
            };

            var result = await userManager.CreateAsync(adminUser, "Password123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new Exception("Error creando Admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Ensure existing admin has the role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}