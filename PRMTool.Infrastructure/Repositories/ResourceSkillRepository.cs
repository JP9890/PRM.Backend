using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class ResourceSkillRepository : IResourceSkillRepository
    {
        private readonly AppDbContext _context;

        public ResourceSkillRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResourceSkill?> GetByIdAsync(int id)
        {
            return await _context.ResourceSkills
                .Include(rs => rs.Skill)
                .Include(rs => rs.ProficiencyLevel)
                .FirstOrDefaultAsync(rs => rs.Id == id);
        }

        public async Task<IEnumerable<ResourceSkill>> GetByResourceProfileIdAsync(int resourceProfileId)
        {
            return await _context.ResourceSkills
                .Include(rs => rs.Skill)
                    .ThenInclude(s => s!.SkillCategory)
                .Include(rs => rs.ProficiencyLevel)
                .Where(rs => rs.ResourceProfileId == resourceProfileId)
                .ToListAsync();
        }

        public async Task AddAsync(ResourceSkill skill)
        {
            await _context.ResourceSkills.AddAsync(skill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ResourceSkill skill)
        {
            _context.ResourceSkills.Update(skill);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ResourceSkill skill)
        {
            _context.ResourceSkills.Remove(skill);
            await _context.SaveChangesAsync();
        }
    }
}
