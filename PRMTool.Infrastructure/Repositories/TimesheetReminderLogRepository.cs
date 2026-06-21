using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using PRMTool.Infrastructure.Data;

namespace PRMTool.Infrastructure.Repositories
{
    public class TimesheetReminderLogRepository : ITimesheetReminderLogRepository
    {
        private readonly AppDbContext _context;

        public TimesheetReminderLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TimesheetReminderLog?> GetByResourceAndWeekAsync(int resourceId, DateTime weekStartDate)
        {
            return await _context.TimesheetReminderLogs
                .FirstOrDefaultAsync(r => r.ResourceId == resourceId && r.WeekStartDate.Date == weekStartDate.Date);
        }

        public async Task AddAsync(TimesheetReminderLog log)
        {
            await _context.TimesheetReminderLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TimesheetReminderLog log)
        {
            _context.TimesheetReminderLogs.Update(log);
            await _context.SaveChangesAsync();
        }
    }
}
