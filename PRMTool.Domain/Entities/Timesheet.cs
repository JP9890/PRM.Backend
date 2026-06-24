using System;
using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    public class Timesheet
    {
        public int Id { get; private set; }

        /// <summary>FK → ResourceProfile.Id (renamed from EmployeeId)</summary>
        public int ResourceId { get; private set; }
        public ResourceProfile? Resource { get; private set; }

        /// <summary>FK → TimesheetStatus.Id (SUBMITTED | MISSED)</summary>
        public int TimesheetStatusId { get; private set; }
        public TimesheetStatus? TimesheetStatus { get; private set; }

        /// <summary>Always Monday. UNIQUE per ResourceId + WeekStartDate.</summary>
        public DateTime WeekStartDate { get; private set; }

        public int TotalHours { get; private set; }
        public DateTime SubmittedAt { get; private set; }

        public ICollection<TimesheetEntry> Entries { get; private set; } = new List<TimesheetEntry>();

        protected Timesheet() { }

        public Timesheet(int resourceId, int timesheetStatusId, DateTime weekStartDate, int totalHours)
        {
            ResourceId = resourceId;
            TimesheetStatusId = timesheetStatusId;
            WeekStartDate = weekStartDate;
            TotalHours = totalHours;
            SubmittedAt = DateTime.UtcNow;
        }
    }
}
