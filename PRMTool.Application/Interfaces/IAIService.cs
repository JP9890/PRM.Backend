using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRMTool.Application.Interfaces
{
    public interface IAIService
    {
        Task<string> GetRiskSummaryAsync(int projectId);
        Task<IEnumerable<string>> GetSkillMatchAsync(int projectId, int employeeId);
    }
}
