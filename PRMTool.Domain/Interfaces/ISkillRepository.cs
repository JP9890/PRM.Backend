using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface ISkillRepository
    {
        Task<IEnumerable<Skill>> GetAllAsync();
        Task<IEnumerable<SkillCategory>> GetAllCategoriesAsync();
        Task<Skill?> GetByIdAsync(int id);
        Task<Skill?> GetByNameAsync(string name);
        Task AddAsync(Skill skill);
    }
}
