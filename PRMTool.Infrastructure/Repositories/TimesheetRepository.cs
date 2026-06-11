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
    public class TimesheetRepository : ITimesheetRepository
    {
        private readonly AppDbContext _context;

        public TimesheetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Timesheet?> GetByIdAsync(int id)
        {
            return await _context.Timesheets
                .Include(t => t.Entries)
                    .ThenInclude(e => e.ActivityTags)
                        .ThenInclude(at => at.ActivityTag)
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Project)
                .Include(t => t.Resource)
                    .ThenInclude(rp => rp!.User)
                .Include(t => t.TimesheetStatus)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Timesheet>> GetByResourceIdAsync(int resourceId)
        {
            return await _context.Timesheets
                .Include(t => t.Entries)
                    .ThenInclude(e => e.ActivityTags)
                        .ThenInclude(at => at.ActivityTag)
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Project)
                .Include(t => t.Resource)
                    .ThenInclude(rp => rp!.User)
                .Include(t => t.TimesheetStatus)
                .Where(t => t.ResourceId == resourceId)
                .OrderByDescending(t => t.WeekStartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Timesheet>> GetByManagerIdAndWeekAsync(int managerId, string weekStart)
        {
            var date = DateTime.Parse(weekStart);
            return await _context.Timesheets
                .Include(t => t.Entries)
                    .ThenInclude(e => e.ActivityTags)
                        .ThenInclude(at => at.ActivityTag)
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Project)
                .Include(t => t.Resource)
                    .ThenInclude(rp => rp!.User)
                .Include(t => t.TimesheetStatus)
                .Where(t => t.Resource!.ManagerId == managerId && t.WeekStartDate.Date == date.Date)
                .ToListAsync();
        }

        public async Task<Timesheet?> GetByResourceAndWeekAsync(int resourceId, string weekStart)
        {
            var date = DateTime.Parse(weekStart);
            return await _context.Timesheets
                .Include(t => t.Entries)
                .FirstOrDefaultAsync(t => t.ResourceId == resourceId && t.WeekStartDate.Date == date.Date);
        }

        public async Task AddAsync(Timesheet timesheet)
        {
            await _context.Timesheets.AddAsync(timesheet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Timesheet timesheet)
        {
            _context.Timesheets.Update(timesheet);
            await _context.SaveChangesAsync();
        }
    }
}
