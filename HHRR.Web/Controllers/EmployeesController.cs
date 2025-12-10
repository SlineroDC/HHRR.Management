using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using HHRR.Web.Models; // Si usas ViewModels
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HHRR.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeRepository _repository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IExcelService _excelService;
    private readonly IPdfService _pdfService;

    public EmployeesController(
        IEmployeeRepository repository, 
        IDepartmentRepository departmentRepository,
        IExcelService excelService, 
        IPdfService pdfService)
    {
        _repository = repository;
        _departmentRepository = departmentRepository;
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

    // 2. CREATE (GET)
    public async Task<IActionResult> Create()
    {
        var departments = await _departmentRepository.GetAllAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        return View();
    }

    // 3. CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (ModelState.IsValid)
        {
            employee.Status = Core.Enums.Status.Active;
            employee.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(employee);
            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        var departments = await _departmentRepository.GetAllAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", employee.DepartmentId);
        return View(employee);
    }

    // 4. EDIT (GET)
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return NotFound();

        var departments = await _departmentRepository.GetAllAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", employee.DepartmentId);
        return View(employee);
    }

    // 5. EDIT (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Retrieve existing to keep some fields if needed, or just update
                // For simplicity, assuming repository handles update correctly
                await _repository.UpdateAsync(employee);
                TempData["Success"] = "Employee updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating employee: {ex.Message}");
            }
        }

        var departments = await _departmentRepository.GetAllAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", employee.DepartmentId);
        return View(employee);
    }

    // 6. DELETE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        TempData["Success"] = "Employee deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // 7. SUBIR EXCEL (IMPORTAR)
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
            // 1. Fetch Departments for Lookup (Name -> Id)
            var departments = await _departmentRepository.GetAllAsync();
            var departmentLookup = departments.ToDictionary(d => d.Name.Trim().ToLower(), d => d.Id);

            using var stream = excelFile.OpenReadStream();
            var employeeDtos = _excelService.ParseExcel(stream);
            
            int addedCount = 0;
            int updatedCount = 0;

            foreach (var dto in employeeDtos)
            {
                // 2. Map Department Name to ID
                int departmentId = 0;
                if (!string.IsNullOrEmpty(dto.DepartmentName) && 
                    departmentLookup.TryGetValue(dto.DepartmentName.Trim().ToLower(), out var id))
                {
                    departmentId = id;
                }
                else
                {
                    // Optional: Handle missing department (e.g., skip, log, or assign default)
                    // For now, we'll leave it as 0 (or nullable if supported) which might cause FK error if not handled
                    // Let's assume 0 means "Unassigned" or handle it if your DB requires valid FK
                }

                // 3. Upsert Logic (Check by Email)
                var existingEmployee = await _repository.GetByEmailAsync(dto.Email);

                if (existingEmployee != null)
                {
                    // UPDATE
                    existingEmployee.Name = dto.Name;
                    existingEmployee.JobTitle = dto.JobTitle;
                    existingEmployee.Salary = dto.Salary;
                    existingEmployee.HiringDate = dto.HiringDate;
                    existingEmployee.DepartmentId = departmentId != 0 ? departmentId : existingEmployee.DepartmentId; // Keep old if new is invalid
                    // Status? Maybe keep existing status or update if provided
                    
                    await _repository.UpdateAsync(existingEmployee);
                    updatedCount++;
                }
                else
                {
                    // INSERT
                    var newEmployee = new Employee
                    {
                        Name = dto.Name,
                        Email = dto.Email,
                        JobTitle = dto.JobTitle,
                        Salary = dto.Salary,
                        HiringDate = dto.HiringDate,
                        DepartmentId = departmentId, // Might be 0 if not found
                        Status = Core.Enums.Status.Active,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _repository.AddAsync(newEmployee);
                    addedCount++;
                }
            }
            
            TempData["Success"] = $"Proceso completado: {addedCount} creados, {updatedCount} actualizados.";
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            TempData["Error"] = $"Error importando: {msg}";
        }

        return RedirectToAction("Index");
    }

    // 8. DESCARGAR HOJA DE VIDA (PDF)
    public async Task<IActionResult> DownloadCv(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return NotFound();

        // Usamos el servicio de PDF que ya creamos en Infraestructura
        var pdfBytes = await _pdfService.GeneratePdfAsync(employee);

        return File(pdfBytes, "application/pdf", $"CV_{employee.Name.Replace(" ", "_")}.pdf");
    }
}