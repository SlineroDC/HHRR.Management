using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using HHRR.Web.Models; // Si usas ViewModels
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HHRR.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeRepository _repository;
    private readonly IExcelService _excelService; // Necesitamos inyectar esto
    private readonly IPdfService _pdfService;     // Y esto

    public EmployeesController(IEmployeeRepository repository, IExcelService excelService, IPdfService pdfService)
    {
        _repository = repository;
        _excelService = excelService;
        _pdfService = pdfService;
    }

    // 1. LISTAR EMPLEADOS
    public async Task<IActionResult> Index()
    {
        var employees = await _repository.GetAllAsync();
        // Ordenamos por fecha de ingreso descendente
        return View(employees.OrderByDescending(e => e.HiringDate));
    }

    // 2. SUBIR EXCEL (IMPORTAR)
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["Error"] = "Por favor selecciona un archivo válido.";
            return RedirectToAction("Index");
        }

        try
        {
            using var stream = excelFile.OpenReadStream();
            // El servicio devuelve DTOs, necesitamos mapearlos a Entidades
            var employeeDtos = _excelService.ParseExcel(stream);
            
            int count = 0;
            foreach (var dto in employeeDtos)
            {
                // Mapeo manual rápido (o usa AutoMapper si lo tienes)
                var employee = new Employee
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    JobTitle = dto.JobTitle,
                    Salary = dto.Salary,
                    HiringDate = dto.HiringDate,
                    DepartmentId = dto.DepartmentId,
                    Status = Core.Enums.Status.Active, // Asumimos activo al importar
                    CreatedAt = DateTime.UtcNow
                };
                
                await _repository.AddAsync(employee);
                count++;
            }
            
            TempData["Success"] = $"¡Éxito! Se importaron {count} empleados.";
        }
        catch (Exception ex)
        {
            // Extraemos el error interno si existe (clave para ver errores de BD)
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            TempData["Error"] = $"Error importando: {msg}";
        }

        return RedirectToAction("Index");
    }

    // 3. DESCARGAR HOJA DE VIDA (PDF)
    public async Task<IActionResult> DownloadCv(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return NotFound();

        // Usamos el servicio de PDF que ya creamos en Infraestructura
        var pdfBytes = await _pdfService.GeneratePdfAsync(employee);

        return File(pdfBytes, "application/pdf", $"CV_{employee.Name.Replace(" ", "_")}.pdf");
    }
}