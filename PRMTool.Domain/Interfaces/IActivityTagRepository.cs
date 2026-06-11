using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface IActivityTagRepository
    {
        Task<IEnumerable<ActivityTagCatalogue>> GetAllAsync();
        Task<ActivityTagCatalogue?> GetByIdAsync(int id);
    }
}
