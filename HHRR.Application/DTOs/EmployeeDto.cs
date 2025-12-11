using HHRR.Core.Enums;

namespace HHRR.Application.DTOs;

public class EmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HiringDate { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Status Status { get; set; }
}
