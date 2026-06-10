using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface IAllocationService
    {
        Task<IEnumerable<AllocationDto>> GetAllActiveAsync(int? employeeId = null, int? projectId = null);
        Task<IEnumerable<AllocationDto>> GetActiveByProjectAsync(int projectId);
        Task<IEnumerable<AllocationDto>> GetByUserIdAsync(int userId);
        Task<AllocationDto> CreateAllocationAsync(CreateAllocationDto dto);
        Task<EndAllocationResultDto> EndAllocationAsync(int allocationId);
    }
}

