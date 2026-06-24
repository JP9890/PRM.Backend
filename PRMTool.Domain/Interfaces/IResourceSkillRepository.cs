using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface IResourceSkillRepository
    {
        Task<ResourceSkill?> GetByIdAsync(int id);
        Task<IEnumerable<ResourceSkill>> GetByResourceProfileIdAsync(int resourceProfileId);
        Task AddAsync(ResourceSkill skill);
        Task UpdateAsync(ResourceSkill skill);
        Task DeleteAsync(ResourceSkill skill);
    }
}
