using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Enums;

namespace PRMTool.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int id);
        Task<Employee?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<IEnumerable<Employee>> GetFilteredAsync(EmployeeStatus? status, string? department);
        Task<IEnumerable<Employee>> GetByManagerIdAsync(int managerId);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
    }
}
