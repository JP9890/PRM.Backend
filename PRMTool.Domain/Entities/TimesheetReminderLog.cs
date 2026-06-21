using System;

namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Tracks the reminder and freeze lifecycle for a specific employee's missed timesheet week.
    /// One row per ResourceProfile per WeekStartDate.
    /// </summary>
    public class TimesheetReminderLog
    {
        public int Id { get; private set; }

        /// <summary>FK → ResourceProfile.Id — the employee who missed the timesheet.</summary>
        public int ResourceId { get; private set; }
        public ResourceProfile? Resource { get; private set; }

        /// <summary>The Monday date of the missed week.</summary>
        public DateTime WeekStartDate { get; private set; }

        public DateTime? Reminder1SentAt { get; private set; }
        public DateTime? Reminder2SentAt { get; private set; }
        public DateTime? FrozenAt { get; private set; }
        public DateTime? RestoredAt { get; private set; }

        /// <summary>FK → Users.Id — the manager who restored access.</summary>
        public int? RestoredByManagerId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        protected TimesheetReminderLog() { }

        public TimesheetReminderLog(int resourceId, DateTime weekStartDate)
        {
            ResourceId = resourceId;
            WeekStartDate = weekStartDate;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkReminder1Sent()
        {
            Reminder1SentAt = DateTime.UtcNow;
        }

        public void MarkReminder2Sent()
        {
            Reminder2SentAt = DateTime.UtcNow;
        }

        public void MarkFrozen()
        {
            FrozenAt = DateTime.UtcNow;
        }

        public void MarkRestored(int restoredByManagerId)
        {
            RestoredAt = DateTime.UtcNow;
            RestoredByManagerId = restoredByManagerId;
        }
    }
}
