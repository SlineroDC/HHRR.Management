using HHRR.Core.Entities;

namespace HHRR.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<IEnumerable<Department>> GetAllAsync();
}
