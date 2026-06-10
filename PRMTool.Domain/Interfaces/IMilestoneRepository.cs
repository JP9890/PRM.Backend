using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface IMilestoneRepository
    {
        Task<IEnumerable<Milestone>> GetByProjectIdAsync(int projectId);
        Task<Milestone?> GetByIdAsync(int id);
        Task AddAsync(Milestone milestone);
        Task UpdateAsync(Milestone milestone);
    }
}
