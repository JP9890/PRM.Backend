using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;
using PRMTool.Domain.Enums;

namespace PRMTool.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeSummaryDto>> GetAllAsync(EmployeeStatus? status = null, string? department = null);
        Task<EmployeeDto?> GetByIdAsync(int id);
        Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto);
        Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto);
        Task<EmployeeDto?> DeactivateAsync(int id);
        Task<EmployeeDto?> AssignManagerAsync(AssignManagerDto dto);
        Task<EmployeeSkillDto> AddSkillAsync(int employeeId, AddSkillDto dto);
        Task<EmployeeSkillDto?> UpdateSkillProficiencyAsync(int employeeId, int skillId, UpdateSkillProficiencyDto dto);
        Task<bool> RemoveSkillAsync(int employeeId, int skillId);
    }
}
