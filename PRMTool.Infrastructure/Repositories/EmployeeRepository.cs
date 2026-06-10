using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Enums;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Skills)
                .Include(e => e.User)
                .Include(e => e.Manager)
                .Include(e => e.Allocations)
                    .ThenInclude(a => a.Project)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Employee?> GetByUserIdAsync(int userId)
        {
            return await _context.Employees
                .Include(e => e.Skills)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Include(e => e.Skills)
                .Include(e => e.User)
                .Include(e => e.Allocations)
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetFilteredAsync(EmployeeStatus? status, string? department)
        {
            var query = _context.Employees
                .Include(e => e.Skills)
                .Include(e => e.User)
                .Where(e => e.IsActive);

            if (status.HasValue)
                query = query.Where(e => e.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(e => e.Department == department);

            return await query.OrderBy(e => e.FullName).ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByManagerIdAsync(int managerId)
        {
            return await _context.Employees
                .Include(e => e.Skills)
                .Include(e => e.User)
                .Include(e => e.Allocations)
                .Where(e => e.ManagerId == managerId && e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }
    }
}
