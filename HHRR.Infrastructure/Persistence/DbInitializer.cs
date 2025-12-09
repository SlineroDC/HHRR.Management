using HHRR.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HHRR.Infrastructure.Persistence;

public class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!context.Departments.Any())
        {
            var departments = new List<Department>
            {
                new Department { Name = "Logístic", Description = "Supply chain management" },
                new Department { Name = "Marketing", Description = "Advertising and branding" },
                new Department { Name = "R.R.H.H", Description = "Human talent management" },
                new Department { Name = "Operations", Description = "Central processes" },
                new Department { Name = "Sales", Description = "Marketing" },
                new Department { Name = "Technology", Description = "Systems and development" },
                new Department { Name = "Accounting", Description = "Finance and audit" }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }
    }
}