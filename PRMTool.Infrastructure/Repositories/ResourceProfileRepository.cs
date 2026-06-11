using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class ResourceProfileRepository : IResourceProfileRepository
    {
        private readonly AppDbContext _context;

        public ResourceProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResourceProfile?> GetByIdAsync(int id)
        {
            return await _context.ResourceProfiles
                .Include(rp => rp.User)
                .Include(rp => rp.Manager)
                .Include(rp => rp.Skills)
                    .ThenInclude(rs => rs.Skill)
                        .ThenInclude(s => s!.SkillCategory)
                .Include(rp => rp.Skills)
                    .ThenInclude(rs => rs.ProficiencyLevel)
                .Include(rp => rp.Allocations)
                    .ThenInclude(a => a.Project)
                .FirstOrDefaultAsync(rp => rp.Id == id);
        }

        public async Task<ResourceProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.ResourceProfiles
                .Include(rp => rp.User)
                .Include(rp => rp.Skills)
                    .ThenInclude(rs => rs.Skill)
                .Include(rp => rp.Allocations)
                    .ThenInclude(a => a.Project)
                .FirstOrDefaultAsync(rp => rp.UserId == userId);
        }

        public async Task<IEnumerable<ResourceProfile>> GetAllAsync()
        {
            return await _context.ResourceProfiles
                .Include(rp => rp.User)
                .Include(rp => rp.Skills)
                    .ThenInclude(rs => rs.Skill)
                .Include(rp => rp.Allocations)
                .Where(rp => rp.IsActive)
                .OrderBy(rp => rp.User!.FullName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ResourceProfile>> GetByManagerIdAsync(int managerId)
        {
            return await _context.ResourceProfiles
                .Include(rp => rp.User)
                .Include(rp => rp.Skills)
                    .ThenInclude(rs => rs.Skill)
                .Include(rp => rp.Allocations)
                    .ThenInclude(a => a.Project)
                .Where(rp => rp.ManagerId == managerId && rp.IsActive)
                .OrderBy(rp => rp.User!.FullName)
                .ToListAsync();
        }

        public async Task AddAsync(ResourceProfile profile)
        {
            await _context.ResourceProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ResourceProfile profile)
        {
            _context.ResourceProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}
