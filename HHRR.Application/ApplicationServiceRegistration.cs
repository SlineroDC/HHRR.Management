using System.Reflection;
using FluentValidation;
using HHRR.Application.Interfaces;
using HHRR.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HHRR.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
