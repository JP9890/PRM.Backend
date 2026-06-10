using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface ITimesheetService
    {
        Task<TimesheetDto> SubmitTimesheetAsync(int employeeId, SubmitTimesheetDto dto);
        Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int employeeId);
        Task<IEnumerable<TimesheetDto>> GetTeamTimesheetsAsync(int managerId, string weekStart);
    }
}
