using HHRR.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HHRR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPdfService _pdfService;

    public EmployeesController(IEmployeeService employeeService, IEmployeeRepository employeeRepository, IPdfService pdfService)
    {
        _employeeService = employeeService;
        _employeeRepository = employeeRepository;
        _pdfService = pdfService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                await _employeeService.ImportEmployeesAsync(stream);
            }

            return Ok(new { message = "Import successful" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Import failed", error = ex.Message });
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GeneratePdf(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound("Employee not found");
        }

        var pdfBytes = await _pdfService.GeneratePdfAsync(employee);
        return File(pdfBytes, "application/pdf", $"cv_{employee.Name.Replace(" ", "_")}.pdf");
    }
}