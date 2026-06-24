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
        private readonly ITimesheetReminderLogRepository _reminderLogRepository;
        private readonly IUserPermissionBlockRepository _permissionBlockRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        // Permission ID for SUBMIT_TIMESHEET (seeded as Id=7 in AppDbContext)
        private const int SubmitTimesheetPermissionId = 7;
        private const string SubmitTimesheetPermissionCode = "SUBMIT_TIMESHEET";
        // Timesheet Status IDs (seeded in AppDbContext)
        private const int StatusMissed = 2;

        public TimesheetService(
            ITimesheetRepository timesheetRepository,
            IResourceProfileRepository profileRepository,
            IActivityTagRepository activityTagRepository,
            ITimesheetReminderLogRepository reminderLogRepository,
            IUserPermissionBlockRepository permissionBlockRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _timesheetRepository = timesheetRepository;
            _profileRepository = profileRepository;
            _activityTagRepository = activityTagRepository;
            _reminderLogRepository = reminderLogRepository;
            _permissionBlockRepository = permissionBlockRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<TimesheetDto> SubmitTimesheetAsync(int resourceId, SubmitTimesheetDto dto)
        {
            var profile = await _profileRepository.GetByIdAsync(resourceId)
                ?? throw new InvalidOperationException("Resource profile not found.");

            // Check if the user's SUBMIT_TIMESHEET permission is blocked (account freeze)
            var userId = profile.UserId;
            if (await _permissionBlockRepository.IsBlockedAsync(userId, SubmitTimesheetPermissionCode))
                throw new UnauthorizedAccessException(
                    "Your timesheet submission access is currently suspended. Please contact your reporting manager to restore access.");

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

        /// <summary>
        /// Progresses each frozen/reminded employee through the reminder lifecycle:
        /// Remind 1 → Remind 2 → Freeze → Notify.
        /// Safe to call multiple times per day — date comparisons prevent duplicate actions.
        /// </summary>
        public async Task ProcessTimesheetRemindersAndFreezesAsync()
        {
            var today = DateTime.UtcNow.Date;
            var lastWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(-7);

            var allProfiles = await _profileRepository.GetAllAsync();
            foreach (var profile in allProfiles)
            {
                if (!profile.IsActive) continue;

                // Only process employees who missed last week's timesheet
                var missed = await _timesheetRepository.GetByResourceAndWeekAsync(
                    profile.Id, lastWeekStart.ToString("yyyy-MM-dd"));
                if (missed == null || missed.TimesheetStatusId != StatusMissed) continue;

                var employee = profile.User;
                if (employee == null) continue;

                // Get or create the reminder log for this employee + missed week
                var log = await _reminderLogRepository.GetByResourceAndWeekAsync(profile.Id, lastWeekStart);
                if (log == null)
                {
                    log = new TimesheetReminderLog(profile.Id, lastWeekStart);
                    await _reminderLogRepository.AddAsync(log);
                }

                // Already frozen & restored by manager — skip
                if (log.RestoredAt.HasValue) continue;

                // STEP 1: Send Reminder 1 (once, on first scheduler run detecting the miss)
                if (!log.Reminder1SentAt.HasValue)
                {
                    await _notificationService.SendTimesheetReminder1Async(employee, lastWeekStart);
                    log.MarkReminder1Sent();
                    await _reminderLogRepository.UpdateAsync(log);
                    continue;
                }

                // STEP 2: Send Reminder 2 (next day after Reminder 1)
                if (!log.Reminder2SentAt.HasValue &&
                    today > log.Reminder1SentAt.Value.Date)
                {
                    await _notificationService.SendTimesheetReminder2Async(employee, lastWeekStart);
                    log.MarkReminder2Sent();
                    await _reminderLogRepository.UpdateAsync(log);
                    continue;
                }

                // STEP 3: Freeze (next day after Reminder 2)
                if (!log.FrozenAt.HasValue &&
                    log.Reminder2SentAt.HasValue &&
                    today > log.Reminder2SentAt.Value.Date)
                {
                    // Block the SUBMIT_TIMESHEET permission for this user
                    var alreadyBlocked = await _permissionBlockRepository.IsBlockedAsync(
                        employee.Id, SubmitTimesheetPermissionCode);

                    if (!alreadyBlocked)
                        await _permissionBlockRepository.AddAsync(
                            new UserPermissionBlock(employee.Id, SubmitTimesheetPermissionId));

                    log.MarkFrozen();
                    await _reminderLogRepository.UpdateAsync(log);

                    // Notify both employee and reporting manager
                    var manager = profile.Manager;
                    if (manager != null)
                        await _notificationService.SendTimesheetFreezeNotificationAsync(employee, manager, lastWeekStart);

                    continue;
                }

                // STEP 4: Already frozen — do nothing, waiting for manager to restore
            }
        }

        /// <summary>
        /// Restores timesheet submission access for a frozen employee.
        /// Only the reporting manager (or Admin) should call this.
        /// </summary>
        public async Task RestoreTimesheetAccessAsync(int resourceId, int managerId)
        {
            var profile = await _profileRepository.GetByIdAsync(resourceId)
                ?? throw new InvalidOperationException("Resource profile not found.");

            // Remove the permission block
            await _permissionBlockRepository.RemoveAsync(profile.UserId, SubmitTimesheetPermissionCode);

            // Update the most recent reminder log
            var today = DateTime.UtcNow.Date;
            var lastWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday).AddDays(-7);
            var log = await _reminderLogRepository.GetByResourceAndWeekAsync(profile.Id, lastWeekStart);
            if (log != null && log.FrozenAt.HasValue && !log.RestoredAt.HasValue)
            {
                log.MarkRestored(managerId);
                await _reminderLogRepository.UpdateAsync(log);
            }
        }
    }
}
