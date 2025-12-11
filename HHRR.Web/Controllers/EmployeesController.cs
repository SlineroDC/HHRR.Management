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

    // 1. List employees
    public async Task<IActionResult> Index()
    {
        var employees = await _repository.GetAllAsync();
        // Sort by hiring date descending
        return View(employees.OrderByDescending(e => e.HiringDate));
    }

    // 2. Create (GET)
    public async Task<IActionResult> Create()
    {
        var departments = await _departmentRepository.GetAllAsync();
        departments ??= new List<Department>();
        
        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        return View(new EmployeeCreateViewModel());
    }

    // 3. Create (POST)
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

    // 4. Edit (GET)
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return NotFound();

        // Split name into first and last
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

    // 5. Edit (POST)
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
                employee.HiringDate = viewModel.HiringDate.ToUniversalTime();
                employee.DepartmentId = viewModel.DepartmentId;
                // Keep existing status and created date

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

    // 6. Delete (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        TempData["Success"] = "Employee deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // 7. Upload Excel (Import)
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["Error"] = "Please select a valid file.";
            return RedirectToAction("Index");
        }

        try
        {
            // 1. Get departments and create dictionary for fast lookup
            var departments = await _departmentRepository.GetAllAsync();
            var departmentLookup = departments.ToDictionary(d => d.Name.Trim().ToLower(), d => d.Id);

            // 2. Define fallback department ID
            // If Excel contains a department that doesn't exist, use "General" or the first available
            var defaultDeptId = departments.FirstOrDefault(d => d.Name == "General")?.Id 
                                ?? departments.FirstOrDefault()?.Id 
                                ?? 0;

            if (defaultDeptId == 0)
            {
                TempData["Error"] = "Critical Error: No departments found in the database.";
                return RedirectToAction("Index");
            }

            using var stream = excelFile.OpenReadStream();
            var employeeDtos = _excelService.ParseExcel(stream);
            
            int addedCount = 0;
            int updatedCount = 0;

            foreach (var dto in employeeDtos)
            {
                // 3. Find department ID
                int departmentId = defaultDeptId; // Assume default first

                if (!string.IsNullOrEmpty(dto.DepartmentName) && 
                    departmentLookup.TryGetValue(dto.DepartmentName.Trim().ToLower(), out var id))
                {
                    departmentId = id; // Found! Use the correct one
                }
                // If not found, keep defaultDeptId to avoid FK error

                // 4. Insert/Update logic
                var existingEmployee = await _repository.GetByEmailAsync(dto.Email);

                if (existingEmployee != null)
                {
                    // Update existing employee
                    existingEmployee.Name = dto.Name;
                    existingEmployee.JobTitle = dto.JobTitle;
                    existingEmployee.Salary = dto.Salary;
                    existingEmployee.HiringDate = dto.HiringDate;
                    existingEmployee.DepartmentId = departmentId;
                    
                    await _repository.UpdateAsync(existingEmployee);
                    updatedCount++;
                }
                else
                {
                    // Insert new employee
                    var newEmployee = new Employee
                    {
                        Name = dto.Name,
                        Email = dto.Email,
                        JobTitle = dto.JobTitle,
                        Salary = dto.Salary,
                        HiringDate = dto.HiringDate,
                        DepartmentId = departmentId, // Will never be 0 at this point
                        Status = Core.Enums.Status.Active,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _repository.AddAsync(newEmployee);
                    addedCount++;
                }
            }
            
            TempData["Success"] = $"Process completed: {addedCount} added, {updatedCount} updated.";
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            TempData["Error"] = $"Import error: {msg}";
        }

        return RedirectToAction("Index");
    }

    // 8. Download CV (PDF)
    public async Task<IActionResult> DownloadCv(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null) return NotFound();

        var pdfBytes = await _pdfService.GeneratePdfAsync(employee);

        return File(pdfBytes, "application/pdf", $"CV_{employee.Name.Replace(" ", "_")}.pdf");
    }
}