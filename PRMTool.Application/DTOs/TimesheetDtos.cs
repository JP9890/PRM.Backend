using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class TimesheetDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string WeekStartDate { get; set; }
        public string Status { get; set; }
        public int TotalHours { get; set; }
        public List<TimesheetEntryDto> Entries { get; set; } = new();
    }

    public class TimesheetEntryDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int HoursWorked { get; set; }
        public string ActivityTags { get; set; }
    }

    public class SubmitTimesheetDto
    {
        [Required]
        public string WeekStartDate { get; set; }

        public List<SubmitTimesheetEntryDto> Entries { get; set; } = new();
    }

    public class SubmitTimesheetEntryDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [Range(1, 168)]
        public int HoursWorked { get; set; }

        public string ActivityTags { get; set; }
    }
}
