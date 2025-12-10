using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HHRR.Infrastructure.Persistence.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments.ToListAsync();
    }
}
