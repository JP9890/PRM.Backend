using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.Status)
                .Include(p => p.Manager)
                .Include(p => p.Milestones)
                    .ThenInclude(m => m.Status)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects
                .Include(p => p.Status)
                .Include(p => p.Manager)
                .Include(p => p.Milestones)
                    .ThenInclude(m => m.Status)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetByManagerIdAsync(int managerId)
        {
            return await _context.Projects
                .Include(p => p.Status)
                .Include(p => p.Manager)
                .Include(p => p.Milestones)
                    .ThenInclude(m => m.Status)
                .Where(p => p.ManagerUserId == managerId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
