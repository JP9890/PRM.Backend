using System;
using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    public class Timesheet
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        
        public DateTime WeekStartDate { get; set; }
        public string Status { get; set; } // e.g., "SUBMITTED", "MISSED"
        
        public ICollection<TimesheetEntry> Entries { get; set; } = new List<TimesheetEntry>();
    }
}
