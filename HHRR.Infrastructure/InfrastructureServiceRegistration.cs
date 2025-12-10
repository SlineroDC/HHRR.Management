using HHRR.Application.Interfaces;
using HHRR.Infrastructure.Persistence;
using HHRR.Infrastructure.Repositories;
using HHRR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HHRR.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                               ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IAIService, AIService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
