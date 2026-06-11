using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface IResourceProfileRepository
    {
        Task<ResourceProfile?> GetByIdAsync(int id);
        Task<ResourceProfile?> GetByUserIdAsync(int userId);
        Task<IEnumerable<ResourceProfile>> GetAllAsync();
        Task<IEnumerable<ResourceProfile>> GetByManagerIdAsync(int managerId);
        Task AddAsync(ResourceProfile profile);
        Task UpdateAsync(ResourceProfile profile);
    }
}
