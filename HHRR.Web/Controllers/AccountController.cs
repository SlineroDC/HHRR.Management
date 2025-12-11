using HHRR.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HHRR.Application.Interfaces;
using HHRR.Core.Entities;

namespace HHRR.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public AccountController(
        SignInManager<IdentityUser> signInManager, 
        UserManager<IdentityUser> userManager,
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
    }

    [HttpGet]
    public IActionResult Login()
    {
        Console.WriteLine("[DEBUG] AccountController: GET Login");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        Console.WriteLine($"[DEBUG] AccountController: POST Login for {model.Email}");

        if (ModelState.IsValid)
        {
            // Check if user exists first for debugging
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine($"[DEBUG] User {model.Email} NOT FOUND in DB.");
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Console.WriteLine("[DEBUG] Login SUCCESS. Checking Roles...");
                
                // Redirection Logic based on Role
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    Console.WriteLine("[DEBUG] Role: Admin -> Redirecting to Dashboard.");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("[DEBUG] Role: User/Other -> Redirecting to Employee Portal.");
                    return RedirectToAction("Me", "EmployeePortal");
                }
            }
            
            if (result.IsLockedOut) Console.WriteLine("[DEBUG] Login FAILED: Locked Out.");
            if (result.IsNotAllowed) Console.WriteLine("[DEBUG] Login FAILED: Not Allowed.");
            if (result.RequiresTwoFactor) Console.WriteLine("[DEBUG] Login FAILED: 2FA Required.");
            
            Console.WriteLine("[DEBUG] Login FAILED: Invalid credentials.");
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        else
        {
            Console.WriteLine("[DEBUG] ModelState is INVALID.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"[DEBUG] Validation Error: {error.ErrorMessage}");
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult RegisterEmployee()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterEmployee(RegisterEmployeeViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign role
                await _userManager.AddToRoleAsync(user, "User");

                // Find a valid default department
                var departments = await _departmentRepository.GetAllAsync();
                
                // Priority: 1. Department named "General", 2. First available
                var defaultDept = departments.FirstOrDefault(d => d.Name == "General") 
                                  ?? departments.FirstOrDefault();

                if (defaultDept == null)
                {
                    // Cannot create employee if no departments exist
                    // This only happens if the database is empty (no seeds)
                    ModelState.AddModelError(string.Empty, "Critical error: No departments configured in the system.");
                    // Optional: Delete the created user to avoid orphaned records
                    await _userManager.DeleteAsync(user); 
                    return View(model);
                }

                // Create Employee Record
                var employee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Status = HHRR.Core.Enums.Status.Active,
                    HiringDate = DateTime.UtcNow,
                    JobTitle = "New Hire", 
                    Salary = 0, 
                    IdentityUserId = user.Id,
                    DepartmentId = defaultDept.Id 
                };

                await _employeeRepository.AddAsync(employee);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Me", "EmployeePortal");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Console.WriteLine("[DEBUG] User Logged Out.");
        return RedirectToAction("Login", "Account");
    }
}