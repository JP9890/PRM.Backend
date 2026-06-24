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

        /// <summary>Submit timesheet for a resource profile (resourceId = ResourceProfile.Id).</summary>
        [HttpPost("{resourceId}")]
        [Authorize(Roles = "Employee,Admin,Manager")]
        public async Task<IActionResult> SubmitTimesheet(int resourceId, [FromBody] SubmitTimesheetDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var timesheet = await _timesheetService.SubmitTimesheetAsync(resourceId, dto);
                return Ok(timesheet);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Get my timesheets by resource profile ID.</summary>
        [HttpGet("my/{resourceId}")]
        [Authorize(Roles = "Employee,Admin,Manager")]
        public async Task<IActionResult> GetMyTimesheets(int resourceId)
        {
            var timesheets = await _timesheetService.GetMyTimesheetsAsync(resourceId);
            return Ok(timesheets);
        }

        /// <summary>Manager: Get team timesheets for a given week.</summary>
        [HttpGet("team/{managerId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetTeamTimesheets(int managerId, [FromQuery] string weekStart)
        {
            if (string.IsNullOrEmpty(weekStart))
                return BadRequest("weekStart is required.");

            var timesheets = await _timesheetService.GetTeamTimesheetsAsync(managerId, weekStart);
            return Ok(timesheets);
        }

        /// <summary>Get the activity tag catalogue for timesheet submission dropdowns.</summary>
        [HttpGet("activity-tags")]
        [Authorize(Roles = "Employee,Admin,Manager")]
        public async Task<IActionResult> GetActivityTags()
        {
            var tags = await _timesheetService.GetActivityTagsAsync();
            return Ok(tags);
        }

        /// <summary>Manager restores timesheet submission access for a frozen employee.</summary>
        [HttpPost("restore-access/{resourceId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> RestoreTimesheetAccess(int resourceId, [FromQuery] int managerId)
        {
            try
            {
                await _timesheetService.RestoreTimesheetAccessAsync(resourceId, managerId);
                return Ok(new { message = "Timesheet access restored successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
