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
        private readonly IResourceProfileRepository _profileRepository;
        private readonly IActivityTagRepository _activityTagRepository;

        public TimesheetService(
            ITimesheetRepository timesheetRepository,
            IResourceProfileRepository profileRepository,
            IActivityTagRepository activityTagRepository)
        {
            _timesheetRepository = timesheetRepository;
            _profileRepository = profileRepository;
            _activityTagRepository = activityTagRepository;
        }

        public async Task<TimesheetDto> SubmitTimesheetAsync(int resourceId, SubmitTimesheetDto dto)
        {
            var profile = await _profileRepository.GetByIdAsync(resourceId)
                ?? throw new InvalidOperationException("Resource profile not found.");

            var existing = await _timesheetRepository.GetByResourceAndWeekAsync(resourceId, dto.WeekStartDate);
            if (existing != null)
                throw new InvalidOperationException("Timesheet already submitted for this week.");

            var weekStart = DateTime.Parse(dto.WeekStartDate);
            var totalHours = dto.Entries.Sum(e => e.HoursWorked);

            // StatusId 1 = SUBMITTED
            var timesheet = new Timesheet(resourceId, timesheetStatusId: 1, weekStart, totalHours);

            foreach (var entryDto in dto.Entries)
            {
                var entry = new TimesheetEntry(0, entryDto.ProjectId, entryDto.HoursWorked);

                foreach (var tagId in entryDto.ActivityTagIds)
                    entry.ActivityTags.Add(new TimesheetActivityTag(0, tagId));

                if (!string.IsNullOrWhiteSpace(entryDto.CustomTag))
                    entry.ActivityTags.Add(new TimesheetActivityTag(0, null, entryDto.CustomTag));

                timesheet.Entries.Add(entry);
            }

            await _timesheetRepository.AddAsync(timesheet);

            var created = await _timesheetRepository.GetByIdAsync(timesheet.Id);
            return MapToDto(created!);
        }

        public async Task<IEnumerable<TimesheetDto>> GetMyTimesheetsAsync(int resourceId)
        {
            var timesheets = await _timesheetRepository.GetByResourceIdAsync(resourceId);
            return timesheets.Select(MapToDto);
        }

        public async Task<IEnumerable<TimesheetDto>> GetTeamTimesheetsAsync(int managerId, string weekStart)
        {
            var timesheets = await _timesheetRepository.GetByManagerIdAndWeekAsync(managerId, weekStart);
            return timesheets.Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityTagDto>> GetActivityTagsAsync()
        {
            var tags = await _activityTagRepository.GetAllAsync();
            return tags.Select(t => new ActivityTagDto
            {
                Id = t.Id,
                TagName = t.TagName,
                DisplayLabel = t.DisplayLabel
            });
        }

        private static TimesheetDto MapToDto(Timesheet t)
        {
            return new TimesheetDto
            {
                Id = t.Id,
                ResourceId = t.ResourceId,
                ResourceName = t.Resource?.User?.FullName ?? "Unknown",
                WeekStartDate = t.WeekStartDate.ToString("yyyy-MM-dd"),
                Status = t.TimesheetStatus?.StatusCode ?? string.Empty,
                TotalHours = t.TotalHours,
                Entries = t.Entries?.Select(e => new TimesheetEntryDto
                {
                    Id = e.Id,
                    ProjectId = e.ProjectId,
                    ProjectName = e.Project?.Name ?? "Unknown",
                    HoursWorked = e.HoursWorked,
                    ActivityTags = e.ActivityTags
                        .Select(at => at.ActivityTag?.TagName ?? at.CustomTag ?? string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList()
                }).ToList() ?? new List<TimesheetEntryDto>()
            };
        }
        public async Task AuditMissedTimesheetsAsync()
        {
            var today = DateTime.UtcNow.Date;
            if (today.DayOfWeek != DayOfWeek.Monday && today.DayOfWeek != DayOfWeek.Tuesday)
                return;

            var lastWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(-7);
            
            var allProfiles = await _profileRepository.GetAllAsync();
            foreach (var profile in allProfiles)
            {
                if (!profile.IsActive) continue;

                var existing = await _timesheetRepository.GetByResourceAndWeekAsync(profile.Id, lastWeekStart.ToString("yyyy-MM-dd"));
                if (existing == null)
                {
                    // StatusId 2 = MISSED
                    var missedTimesheet = new Timesheet(profile.Id, timesheetStatusId: 2, lastWeekStart, 0);
                    await _timesheetRepository.AddAsync(missedTimesheet);
                }
            }
        }
    }
}
