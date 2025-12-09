using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHRR.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeRepository employeeRepository, IEmployeeService employeeService)
    {
        _employeeRepository = employeeRepository;
        _employeeService = employeeService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return View(employees);
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    await _employeeService.ImportEmployeesAsync(stream);
                }
                TempData["SuccessMessage"] = "Employees imported successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Import failed: {ex.Message}";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Please select a file.";
        }

        return RedirectToAction(nameof(Index));
    }
}
