using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class AllocationRepository : IAllocationRepository
    {
        private readonly AppDbContext _context;

        public AllocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Allocation>> GetAllActiveAsync(DateTime? asOfDate = null)
        {
            var date = (asOfDate ?? DateTime.UtcNow).Date;
            return await _context.Allocations
                .Include(a => a.Resource)
                    .ThenInclude(rp => rp!.User)
                .Include(a => a.Project)
                .Where(a => a.IsActive && a.FromDate.Date <= date && a.ToDate.Date >= date)
                .OrderBy(a => a.Resource!.User!.FullName)
                .ThenBy(a => a.Project!.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Allocation>> GetActiveByResourceIdAsync(int resourceId, DateTime? asOfDate = null)
        {
            var date = (asOfDate ?? DateTime.UtcNow).Date;
            return await _context.Allocations
                .Include(a => a.Project)
                .Where(a => a.ResourceId == resourceId
                    && a.IsActive
                    && a.FromDate.Date <= date
                    && a.ToDate.Date >= date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Allocation>> GetActiveByProjectIdAsync(int projectId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Allocations
                .Include(a => a.Resource)
                    .ThenInclude(rp => rp!.User)
                .Include(a => a.Project)
                .Where(a => a.ProjectId == projectId
                    && a.IsActive
                    && a.FromDate.Date <= today
                    && a.ToDate.Date >= today)
                .ToListAsync();
        }

        public async Task<Allocation?> GetByIdAsync(int id)
        {
            return await _context.Allocations
                .Include(a => a.Resource)
                    .ThenInclude(rp => rp!.User)
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Allocation allocation)
        {
            await _context.Allocations.AddAsync(allocation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Allocation allocation)
        {
            _context.Allocations.Update(allocation);
            await _context.SaveChangesAsync();
        }
    }
}
