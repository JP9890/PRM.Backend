using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class TimesheetService : ITimesheetService
    {
        private readonly ITimesheetRepository _timesheetRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public TimesheetService(ITimesheetRepository timesheetRepository, IEmployeeRepository employeeRepository)
        {
            _timesheetRepository = timesheetRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<TimesheetDto> SubmitTimesheetAsync(int employeeId, SubmitTimesheetDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found");

            var existing = await _timesheetRepository.GetByEmployeeAndWeekAsync(employeeId, dto.WeekStartDate);
            if (existing != null)
                throw new InvalidOperationException("Timesheet already submitted for this week");

            var timesheet = new Timesheet
            {
                EmployeeId = employeeId,
                WeekStartDate = DateTime.Parse(dto.WeekStartDate),
                Status = "SUBMITTED",
                Entries = dto.Entries.Select(e => new TimesheetEntry
                {
                    ProjectId = e.ProjectId,
                    HoursWorked = e.HoursWorked,
                    ActivityTags = e.ActivityTags ?? string.Empty
                }).ToList()
            };

            await _timesheetRepository.AddAsync(timesheet);

            var created = await _timesheetRepository.GetByIdAsync(timesheet.Id);
            return MapToDto(created);
        }

        public async Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int employeeId)
        {
            var timesheets = await _timesheetRepository.GetByEmployeeIdAsync(employeeId);
            return timesheets.Select(MapToDto);
        }

        public async Task<IEnumerable<TimesheetDto>> GetTeamTimesheetsAsync(int managerId, string weekStart)
        {
            var timesheets = await _timesheetRepository.GetByManagerIdAndWeekAsync(managerId, weekStart);
            return timesheets.Select(MapToDto);
        }

        private static TimesheetDto MapToDto(Timesheet t)
        {
            return new TimesheetDto
            {
                Id = t.Id,
                EmployeeId = t.EmployeeId,
                EmployeeName = t.Employee?.FullName ?? "Unknown",
                WeekStartDate = t.WeekStartDate.ToString("yyyy-MM-dd"),
                Status = t.Status,
                TotalHours = t.Entries?.Sum(e => e.HoursWorked) ?? 0,
                Entries = t.Entries?.Select(e => new TimesheetEntryDto
                {
                    Id = e.Id,
                    ProjectId = e.ProjectId,
                    ProjectName = e.Project?.Name ?? "Unknown",
                    HoursWorked = e.HoursWorked,
                    ActivityTags = e.ActivityTags
                }).ToList() ?? new List<TimesheetEntryDto>()
            };
        }
    }
}
