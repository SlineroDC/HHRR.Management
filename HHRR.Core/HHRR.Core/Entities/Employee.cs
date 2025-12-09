using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HHRR.Core.Common;
using HHRR.Core.Enums;

namespace HHRR.Core.Entities;

public class Employee : AuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string JobTitle { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    public DateTime HiringDate { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department? Department { get; set; }

    [Required]
    public Status Status { get; set; } = Status.Active;

    // Link to ASP.NET Identity User
    public string? IdentityUserId { get; set; }
}