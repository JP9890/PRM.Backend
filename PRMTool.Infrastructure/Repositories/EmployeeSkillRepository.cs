using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class EmployeeSkillRepository : IEmployeeSkillRepository
    {
        private readonly AppDbContext _context;

        public EmployeeSkillRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeSkill>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeSkills
                .Where(s => s.EmployeeId == employeeId)
                .OrderBy(s => s.SkillName)
                .ToListAsync();
        }

        public async Task<EmployeeSkill?> GetByIdAsync(int id)
        {
            return await _context.EmployeeSkills.FindAsync(id);
        }

        public async Task AddAsync(EmployeeSkill skill)
        {
            await _context.EmployeeSkills.AddAsync(skill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmployeeSkill skill)
        {
            _context.EmployeeSkills.Update(skill);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(EmployeeSkill skill)
        {
            _context.EmployeeSkills.Remove(skill);
            await _context.SaveChangesAsync();
        }
    }
}
