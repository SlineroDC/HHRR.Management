using FluentValidation;
using HHRR.Application.DTOs;
using HHRR.Application.Interfaces;
using HHRR.Core.Entities;

namespace HHRR.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IExcelService _excelService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IValidator<EmployeeDto> _validator;

    public EmployeeService(IExcelService excelService, IEmployeeRepository employeeRepository, IValidator<EmployeeDto> validator)
    {
        _excelService = excelService;
        _employeeRepository = employeeRepository;
        _validator = validator;
    }

    public async Task ImportEmployeesAsync(Stream fileStream)
    {
        var dtos = _excelService.ParseExcel(fileStream);
        var errors = new List<string>();

        foreach (var dto in dtos)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                // Collect errors or log them. For now, we skip invalid rows or log them.
                // In a real app, we might return a report of failed rows.
                foreach (var error in validationResult.Errors)
                {
                    errors.Add($"Row for {dto.Email}: {error.ErrorMessage}");
                }
                continue; 
            }

            var existingEmployee = (await _employeeRepository.GetAllAsync())
                .FirstOrDefault(e => e.Email == dto.Email);

            if (existingEmployee != null)
            {
                // Update existing employee
                existingEmployee.Name = dto.Name;
                existingEmployee.JobTitle = dto.JobTitle;
                existingEmployee.Salary = dto.Salary;
                existingEmployee.HiringDate = dto.HiringDate;
                existingEmployee.DepartmentId = dto.DepartmentId;
                existingEmployee.Status = dto.Status;
                
                // AuditableEntity fields like LastModifiedBy would be set here or in DbContext
                
                await _employeeRepository.UpdateAsync(existingEmployee);
            }
            else
            {
                // Create new employee
                var newEmployee = new Employee
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    JobTitle = dto.JobTitle,
                    Salary = dto.Salary,
                    HiringDate = dto.HiringDate,
                    DepartmentId = dto.DepartmentId,
                    Status = dto.Status
                };

                await _employeeRepository.AddAsync(newEmployee);
            }
        }
        
        if (errors.Any())
        {
            // Ideally, throw an exception with the list of errors or return a result object.
            // For this implementation, we'll just log or let the valid ones pass.
            // throw new ValidationException(string.Join("\n", errors));
        }
    }
}
