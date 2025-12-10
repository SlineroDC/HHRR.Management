using HHRR.Application.Interfaces;
using HHRR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HHRR.Core.Enums; // Para EmployeeStatus.Active

namespace HHRR.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IEmployeeRepository _repository;
    private readonly IAIService _aiService;

    public HomeController(IEmployeeRepository repository, IAIService aiService)
    {
        _repository = repository;
        _aiService = aiService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _repository.GetAllAsync();

        var model = new DashboardViewModel
        {
            TotalEmployees = employees.Count(),
            // Filtramos por Status = 1 (Active) asumiendo que es un Enum o Int
            ActiveEmployees = employees.Count(e => e.Status == Status.Active), 
            DepartmentsCount = employees.Select(e => e.DepartmentId).Distinct().Count()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AskAI([FromBody] QuestionRequest request)
    {
        // 1. Validación básica
        if (request == null || string.IsNullOrWhiteSpace(request.Question)) 
            return BadRequest(new { answer = "Por favor escribe una pregunta válida." });

        try 
        {
            // 2. Llamada al servicio
            var answer = await _aiService.GenerateContentAsync(request.Question);
            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            // 3. Manejo de error controlado
            return Ok(new { answer = $"Error interno: {ex.Message}" });
        }
    }

    // Asegúrate de que esta clase sea pública
    public class QuestionRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}