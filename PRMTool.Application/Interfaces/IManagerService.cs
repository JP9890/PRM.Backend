using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface IManagerService
    {
        Task<ManagerDashboardDto> GetDashboardAsync(int managerId);
        Task<IEnumerable<ManagerProjectDto>> GetManagerProjectsAsync(int managerId);
        Task<ManagerProjectDetailDto?> GetProjectDetailAsync(int managerId, int projectId);
        Task<IEnumerable<EmployeeDashboardDto>> GetTeamEmployeesAsync(int managerId);
    }
}

