using HHRR.Application.Interfaces;
using HHRR.Core.Entities;
using HHRR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHRR.Infrastructure.Repositories;

// Debe heredar de IDepartmentRepository (definida en Application.Interfaces)
public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Usado por el Excel Smart Import para mapear Nombre a ID
    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments.ToListAsync();
    }
    
    // Usado si tienes un lookup por nombre de departamento
    public async Task<Department?> GetByNameAsync(string name)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == name);
    }
    
    // Usado por los dropdowns en la Web Admin
    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments.FindAsync(id);
    }

    // Mínimo para que compile: Implementa un método que devuelva IEnumerable<Department>
    // Dependiendo de tu interfaz, debes tener GetById, Add, Update, Delete también.

    // Ejemplo de un método que devuelve solo los nombres y IDs (para la IA/Excel Lookup)
    public async Task<Dictionary<string, int>> GetNameIdDictionaryAsync()
    {
        return await _context.Departments
            .ToDictionaryAsync(d => d.Name, d => d.Id);
    }
    
    // Implementación mínima de otros métodos requeridos por la interfaz IDepartmentRepository:

    public Task AddAsync(Department department)
    {
        _context.Departments.Add(department);
        return _context.SaveChangesAsync();
    }

    public void Update(Department department)
    {
        _context.Departments.Update(department);
        _context.SaveChanges();
    }

    public void Delete(Department department)
    {
        _context.Departments.Remove(department);
        _context.SaveChanges();
    }
}