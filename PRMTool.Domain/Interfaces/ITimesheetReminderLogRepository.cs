using System;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface ITimesheetReminderLogRepository
    {
        Task<TimesheetReminderLog?> GetByResourceAndWeekAsync(int resourceId, DateTime weekStartDate);
        Task AddAsync(TimesheetReminderLog log);
        Task UpdateAsync(TimesheetReminderLog log);
    }
}
