using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HHRR.Application.DTOs;
using HHRR.Application.Interfaces;
using HHRR.Application.Services;
using HHRR.Core.Entities;
using HHRR.Core.Enums;
using Moq;
using Xunit;

namespace HHRR.Tests.UnitTests;

public class EmployeeServiceTests
{
    private readonly Mock<IExcelService> _mockExcelService;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IValidator<EmployeeDto>> _mockValidator;
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _mockExcelService = new Mock<IExcelService>();
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockValidator = new Mock<IValidator<EmployeeDto>>();
        
        _employeeService = new EmployeeService(
            _mockExcelService.Object,
            _mockEmployeeRepository.Object,
            _mockValidator.Object
        );
    }

    [Fact]
    public async Task GetAllEmployeesAsync_ShouldReturnListOfEmployees()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "John Doe", Email = "john@example.com", Status = Status.Active },
            new Employee { Id = 2, Name = "Jane Doe", Email = "jane@example.com", Status = Status.Inactive }
        };

        _mockEmployeeRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(employees);

        // Act
        var result = await _employeeService.GetAllEmployeesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        _mockEmployeeRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldSetDefaultStatusToActive()
    {
        // Arrange
        var employeeDto = new EmployeeDto
        {
            Name = "New Employee",
            Email = "new@example.com",
            JobTitle = "Developer",
            Salary = 50000,
            DepartmentId = 1,
            HiringDate = DateTime.UtcNow
            // Status is NOT set here, relying on default logic
        };

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<EmployeeDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _mockEmployeeRepository.Setup(repo => repo.AddAsync(It.IsAny<Employee>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _employeeService.CreateEmployeeAsync(employeeDto);

        // Assert
        result.Status.Should().Be(Status.Active);
        
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.Is<Employee>(e => 
            e.Status == Status.Active && 
            e.Email == employeeDto.Email
        )), Times.Once);
    }
}
