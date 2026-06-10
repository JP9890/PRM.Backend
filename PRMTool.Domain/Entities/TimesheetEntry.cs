using System;

namespace PRMTool.Domain.Entities
{
    public class TimesheetEntry
    {
        public int Id { get; set; }
        
        public int TimesheetId { get; set; }
        public Timesheet Timesheet { get; set; }
        
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        
        public int HoursWorked { get; set; }
        public string ActivityTags { get; set; } // Comma-separated list of tags
    }
}
