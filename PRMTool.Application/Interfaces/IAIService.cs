using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface IAIService
    {
        Task<string> GetRiskSummaryAsync(int projectId);
        Task<IEnumerable<string>> GetSkillMatchAsync(int projectId, int employeeId);
        Task<IEnumerable<AIMatchResultDto>> SearchTeamResourcesAsync(int managerId, string query, int projectId);
        Task<TeamBuilderResponseDto> BuildTeamAsync(int managerId, TeamBuilderRequestDto request);
    }
}
