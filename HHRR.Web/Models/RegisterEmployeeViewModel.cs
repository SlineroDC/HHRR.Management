using System.ComponentModel.DataAnnotations;

namespace HHRR.Web.Models;

public class RegisterEmployeeViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;
}
