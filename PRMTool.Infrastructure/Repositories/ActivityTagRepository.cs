using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class ActivityTagRepository : IActivityTagRepository
    {
        private readonly AppDbContext _context;

        public ActivityTagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ActivityTagCatalogue>> GetAllAsync()
        {
            return await _context.ActivityTagCatalogues
                .OrderBy(t => t.SortOrder)
                .ToListAsync();
        }

        public async Task<ActivityTagCatalogue?> GetByIdAsync(int id)
        {
            return await _context.ActivityTagCatalogues.FindAsync(id);
        }
    }
}
