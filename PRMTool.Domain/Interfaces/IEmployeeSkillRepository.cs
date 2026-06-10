using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface IEmployeeSkillRepository
    {
        Task<IEnumerable<EmployeeSkill>> GetByEmployeeIdAsync(int employeeId);
        Task<EmployeeSkill?> GetByIdAsync(int id);
        Task AddAsync(EmployeeSkill skill);
        Task UpdateAsync(EmployeeSkill skill);
        Task DeleteAsync(EmployeeSkill skill);
    }
}
