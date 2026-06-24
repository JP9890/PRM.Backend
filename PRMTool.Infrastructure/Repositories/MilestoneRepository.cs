using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class MilestoneRepository : IMilestoneRepository
    {
        private readonly AppDbContext _context;

        public MilestoneRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Milestone>> GetByProjectIdAsync(int projectId)
        {
            return await _context.Milestones
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<Milestone?> GetByIdAsync(int id)
        {
            return await _context.Milestones.FindAsync(id);
        }

        public async Task AddAsync(Milestone milestone)
        {
            await _context.Milestones.AddAsync(milestone);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Milestone milestone)
        {
            _context.Milestones.Update(milestone);
            await _context.SaveChangesAsync();
        }
    }
}
