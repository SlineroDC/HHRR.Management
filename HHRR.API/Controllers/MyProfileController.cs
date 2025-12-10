using System.Security.Claims;
using HHRR.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHRR.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MyProfileController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPdfService _pdfService;

    public MyProfileController(IEmployeeRepository employeeRepository, IPdfService pdfService)
    {
        _employeeRepository = employeeRepository;
        _pdfService = pdfService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var employee = await _employeeRepository.GetByEmailAsync(email);
        if (employee == null) return NotFound("Employee profile not found.");

        return Ok(employee);
    }

    [HttpGet("me/cv")]
    public async Task<IActionResult> GetMyCv()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var employee = await _employeeRepository.GetByEmailAsync(email);
        if (employee == null) return NotFound("Employee profile not found.");

        var pdfBytes = await _pdfService.GeneratePdfAsync(employee);
        return File(pdfBytes, "application/pdf", $"CV_{employee.Name.Replace(" ", "_")}.pdf");
    }
}
