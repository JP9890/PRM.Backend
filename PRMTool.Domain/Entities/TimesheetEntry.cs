using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    public class TimesheetEntry
    {
        public int Id { get; private set; }
        public int TimesheetId { get; private set; }
        public Timesheet? Timesheet { get; private set; }

        public int ProjectId { get; private set; }
        public Project? Project { get; private set; }

        public int HoursWorked { get; private set; }

        /// <summary>Tags applied to this entry via the junction table.</summary>
        public ICollection<TimesheetActivityTag> ActivityTags { get; private set; } = new List<TimesheetActivityTag>();

        protected TimesheetEntry() { }

        public TimesheetEntry(int timesheetId, int projectId, int hoursWorked)
        {
            TimesheetId = timesheetId;
            ProjectId = projectId;
            HoursWorked = hoursWorked;
        }
    }
}
