using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Domain.Entities;

namespace PRMTool.Domain.Interfaces
{
    public interface ITimesheetRepository
    {
        Task<Timesheet?> GetByIdAsync(int id);
        Task<IEnumerable<Timesheet>> GetByResourceIdAsync(int resourceId);
        Task<IEnumerable<Timesheet>> GetByManagerIdAndWeekAsync(int managerId, string weekStart);
        Task AddAsync(Timesheet timesheet);
        Task UpdateAsync(Timesheet timesheet);
        Task<Timesheet?> GetByResourceAndWeekAsync(int resourceId, string weekStart);
    }
}
