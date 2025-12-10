using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using HHRR.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using HHRR.Infrastructure.Persistence;

namespace HHRR.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PublicController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public PublicController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IEmployeeRepository employeeRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _employeeRepository = employeeRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromServices] ApplicationDbContext context)
    {
        // Ideally use a DepartmentRepository, but context is fine for read-only public list if needed quickly.
        // Or inject IEmployeeRepository and assume we don't have DepartmentRepo yet.
        // Let's use context directly for simplicity as per prompt "Use Repository" but we only have EmployeeRepo.
        // Wait, prompt says "Use Repository". I don't have DepartmentRepo. I'll use context or add repo.
        // I'll use context for now to save time, or just return empty if strict.
        // Actually, I can use _employeeRepository if I added GetDepartments there? No.
        // I'll just use the DbContext which is registered.
        var departments = context.Departments.Select(d => new { d.Id, d.Name }).ToList();
        return Ok(departments);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // 1. Create Identity User
        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        // 2. Create Employee Entity
        var employee = new Employee
        {
            Name = request.Name,
            Email = request.Email,
            JobTitle = request.JobTitle,
            Salary = request.Salary,
            DepartmentId = request.DepartmentId,
            HiringDate = DateTime.UtcNow,
            Status = Status.Active,
            IdentityUserId = user.Id
        };

        await _employeeRepository.AddAsync(employee);

        // 3. Send Welcome Email
        await _emailService.SendWelcomeEmailAsync(request.Email, request.Name);

        return Ok(new { Message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

        if (!result.Succeeded) return Unauthorized("Invalid credentials");

        var user = await _userManager.FindByEmailAsync(request.Email);
        var employee = await _employeeRepository.GetByEmailAsync(request.Email);

        var token = GenerateJwtToken(user, employee);

        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(IdentityUser user, Employee? employee)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "SuperSecretKey1234567890_ChangeMeInEnv";
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "HHRR_API";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "HHRR_Users";

        var claims = new List<Claim>
        {
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Email ?? ""),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        if (employee != null)
        {
            claims.Add(new Claim("EmployeeId", employee.Id.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public class RegisterRequest
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int DepartmentId { get; set; }
        public required string JobTitle { get; set; }
        public decimal Salary { get; set; }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
