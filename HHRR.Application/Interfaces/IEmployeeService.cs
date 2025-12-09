using HHRR.Application.DTOs;

namespace HHRR.Application.Interfaces;

public interface IEmployeeService
{
    Task ImportEmployeesAsync(Stream fileStream);
}
