using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface ITimesheetService
    {
        Task<TimesheetDto> SubmitTimesheetAsync(int resourceId, SubmitTimesheetDto dto);
        Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int resourceId);
        Task<IEnumerable<TimesheetDto>> GetTeamTimesheetsAsync(int managerId, string weekStart);
        Task AuditMissedTimesheetsAsync();
        Task<IEnumerable<ActivityTagDto>> GetActivityTagsAsync();
        /// <summary>Runs the full Detect → Remind 1 → Remind 2 → Freeze → Notify cycle. Called by the background scheduler.</summary>
        Task ProcessTimesheetRemindersAndFreezesAsync();
        /// <summary>Manager restores a frozen employee's timesheet submission access.</summary>
        Task RestoreTimesheetAccessAsync(int resourceId, int managerId);
    }
}
