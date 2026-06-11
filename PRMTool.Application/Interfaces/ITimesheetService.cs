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
        Task<IEnumerable<ActivityTagDto>> GetActivityTagsAsync();
    }
}
