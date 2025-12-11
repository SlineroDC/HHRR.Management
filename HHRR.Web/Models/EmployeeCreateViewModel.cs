using System.ComponentModel.DataAnnotations;

namespace HHRR.Web.Models;

public class EmployeeCreateViewModel
{
    public int Id { get; set; } 

    [Required(ErrorMessage = "First Name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job Title is required")]
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Salary is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Hiring Date is required")]
    [Display(Name = "Hiring Date")]
    [DataType(DataType.Date)]
    public DateTime HiringDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Department is required")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }
}
