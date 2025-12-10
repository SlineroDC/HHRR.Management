using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using HHRR.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HHRR.Web.Controllers;

[Authorize(Roles = "Admin, Manager")]
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
        departments ??= new List<Department>();
        
        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        return View(new EmployeeCreateViewModel());
    }

    // 3. CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeCreateViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var employee = new Employee
            {
                Name = $"{viewModel.FirstName.Trim()} {viewModel.LastName.Trim()}",
                Email = viewModel.Email,
                JobTitle = viewModel.JobTitle,
                Salary = viewModel.Salary,
                HiringDate = viewModel.HiringDate.ToUniversalTime(), // Ensure UTC
                DepartmentId = viewModel.DepartmentId,
                Status = Core.Enums.Status.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(employee);
            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        var departments = await _departmentRepository.GetAllAsync();
        departments ??= new List<Department>();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", viewModel.DepartmentId);
        return View(viewModel);
    }

    // 4. EDIT (GET)
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return NotFound();

        // Split Name into First and Last
        var names = (employee.Name ?? "").Split(' ', 2);
        var firstName = names.Length > 0 ? names[0] : "";
        var lastName = names.Length > 1 ? names[1] : "";

        var viewModel = new EmployeeCreateViewModel
        {
            Id = employee.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = employee.Email,
            JobTitle = employee.JobTitle,
            Salary = employee.Salary,
            HiringDate = employee.HiringDate,
            DepartmentId = employee.DepartmentId
        };

        var departments = await _departmentRepository.GetAllAsync();
        departments ??= new List<Department>();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", employee.DepartmentId);
        return View("Create", viewModel); // Reusing the Create view which now supports Edit via Id
    }

    // 5. EDIT (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeCreateViewModel viewModel)
    {
        if (id != viewModel.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var employee = await _repository.GetByIdAsync(id);
                if (employee == null) return NotFound();

                employee.Name = $"{viewModel.FirstName.Trim()} {viewModel.LastName.Trim()}";
                employee.Email = viewModel.Email;
                employee.JobTitle = viewModel.JobTitle;
                employee.Salary = viewModel.Salary;
                employee.HiringDate = viewModel.HiringDate.ToUniversalTime(); // Ensure UTC
                employee.DepartmentId = viewModel.DepartmentId;
                // Keep existing Status and CreatedAt

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
        departments ??= new List<Department>();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", viewModel.DepartmentId);
        return View("Create", viewModel);
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
            var departments = await _departmentRepository.GetAllAsync();
            var departmentLookup = departments.ToDictionary(d => d.Name.Trim().ToLower(), d => d.Id);

            using var stream = excelFile.OpenReadStream();
            var employeeDtos = _excelService.ParseExcel(stream);
            
            int addedCount = 0;
            int updatedCount = 0;

            foreach (var dto in employeeDtos)
            {
                int departmentId = 0;
                if (!string.IsNullOrEmpty(dto.DepartmentName) && 
                    departmentLookup.TryGetValue(dto.DepartmentName.Trim().ToLower(), out var id))
                {
                    departmentId = id;
                }

                var existingEmployee = await _repository.GetByEmailAsync(dto.Email);

                if (existingEmployee != null)
                {
                    existingEmployee.Name = dto.Name;
                    existingEmployee.JobTitle = dto.JobTitle;
                    existingEmployee.Salary = dto.Salary;
                    existingEmployee.HiringDate = dto.HiringDate.ToUniversalTime();
                    existingEmployee.DepartmentId = departmentId != 0 ? departmentId : existingEmployee.DepartmentId;
                    
                    await _repository.UpdateAsync(existingEmployee);
                    updatedCount++;
                }
                else
                {
                    var newEmployee = new Employee
                    {
                        Name = dto.Name,
                        Email = dto.Email,
                        JobTitle = dto.JobTitle,
                        Salary = dto.Salary,
                        HiringDate = dto.HiringDate.ToUniversalTime(),
                        DepartmentId = departmentId,
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

        var pdfBytes = await _pdfService.GeneratePdfAsync(employee);

        return File(pdfBytes, "application/pdf", $"CV_{employee.Name.Replace(" ", "_")}.pdf");
    }
}