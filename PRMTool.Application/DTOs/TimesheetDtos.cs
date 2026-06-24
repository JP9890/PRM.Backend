using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class TimesheetDto
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public string WeekStartDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalHours { get; set; }
        public List<TimesheetEntryDto> Entries { get; set; } = new();
    }

    public class TimesheetEntryDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int HoursWorked { get; set; }

        /// <summary>Resolved tag names — from catalogue or custom text.</summary>
        public List<string> ActivityTags { get; set; } = new();
    }

    public class SubmitTimesheetDto
    {
        [Required]
        public string WeekStartDate { get; set; } = string.Empty;

        public List<SubmitTimesheetEntryDto> Entries { get; set; } = new();
    }

    public class SubmitTimesheetEntryDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [Range(1, 168)]
        public int HoursWorked { get; set; }

        /// <summary>IDs of selected ActivityTagCatalogue entries.</summary>
        public List<int> ActivityTagIds { get; set; } = new();

        /// <summary>Populated only if "Other" is selected by the employee.</summary>
        public string? CustomTag { get; set; }
    }

    public class ActivityTagDto
    {
        public int Id { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string DisplayLabel { get; set; } = string.Empty;
    }
}
