using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class SkillRepository : ISkillRepository
    {
        private readonly AppDbContext _context;

        public SkillRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Skill>> GetAllAsync()
        {
            return await _context.Skills
                .Include(s => s.SkillCategory)
                .OrderBy(s => s.SkillCategory!.SortOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<SkillCategory>> GetAllCategoriesAsync()
        {
            return await _context.SkillCategories
                .OrderBy(sc => sc.SortOrder)
                .ToListAsync();
        }

        public async Task<Skill?> GetByIdAsync(int id)
        {
            return await _context.Skills
                .Include(s => s.SkillCategory)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Skill?> GetByNameAsync(string name)
        {
            return await _context.Skills
                .FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task AddAsync(Skill skill)
        {
            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();
        }
    }
}
