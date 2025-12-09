using System.ComponentModel.DataAnnotations;
using HHRR.Core.Common;

namespace HHRR.Core.Entities;

public class Department : AuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}