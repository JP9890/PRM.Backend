using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimesheetsController : ControllerBase
    {
        private readonly ITimesheetService _timesheetService;

        public TimesheetsController(ITimesheetService timesheetService)
        {
            _timesheetService = timesheetService;
        }

        [HttpPost("{employeeId}")]
        [Authorize(Roles = "Employee,Admin,Manager")]
        public async Task<IActionResult> SubmitTimesheet(int employeeId, [FromBody] SubmitTimesheetDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var timesheet = await _timesheetService.SubmitTimesheetAsync(employeeId, dto);
                return Ok(timesheet);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my/{employeeId}")]
        [Authorize(Roles = "Employee,Admin,Manager")]
        public async Task<IActionResult> GetMyTimesheets(int employeeId)
        {
            var timesheets = await _timesheetService.GetMyTimesheetsAsync(employeeId);
            return Ok(timesheets);
        }

        [HttpGet("team/{managerId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetTeamTimesheets(int managerId, [FromQuery] string weekStart)
        {
            if (string.IsNullOrEmpty(weekStart))
            {
                return BadRequest("weekStart is required");
            }
            
            var timesheets = await _timesheetService.GetTeamTimesheetsAsync(managerId, weekStart);
            return Ok(timesheets);
        }
    }
}
