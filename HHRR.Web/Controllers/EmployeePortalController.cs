using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HHRR.Web.Controllers;

[Authorize]
public class EmployeePortalController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPdfService _pdfService;

    public EmployeePortalController(
        UserManager<IdentityUser> userManager,
        IEmployeeRepository employeeRepository,
        IPdfService pdfService)
    {
        _userManager = userManager;
        _employeeRepository = employeeRepository;
        _pdfService = pdfService;
    }

    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var employee = await _employeeRepository.GetByEmailAsync(user.Email!);
        if (employee == null)
        {
            // Fallback if employee record doesn't exist for some reason
            return View(new Employee { Name = "Unknown", Email = user.Email ?? "" });
        }

        return View(employee);
    }

    public async Task<IActionResult> DownloadMyCv()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var employee = await _employeeRepository.GetByEmailAsync(user.Email!);
        if (employee == null) return NotFound("Employee record not found.");

        var pdfBytes = await _pdfService.GeneratePdfAsync(employee);
        return File(pdfBytes, "application/pdf", $"CV_{employee.Name.Replace(" ", "_")}.pdf");
    }
}
