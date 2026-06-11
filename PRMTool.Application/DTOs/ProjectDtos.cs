using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace PRMTool.Application.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public int TotalStoryPoints { get; set; }
        public int CompletedStoryPoints { get; set; }
        public List<MilestoneDto> Milestones { get; set; } = new();
    }

    public class MilestoneDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public int StoryPoints { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateProjectDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int ProjectStatusId { get; set; }

        [Required]
        public int ManagerId { get; set; }

        [Required]
        public int TotalStoryPoints { get; set; }
    }

    public class UpdateProjectDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int ProjectStatusId { get; set; }

        [Required]
        public int ManagerId { get; set; }

        [Required]
        public int TotalStoryPoints { get; set; }
    }

    public class CreateMilestoneDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int StoryPoints { get; set; }
    }

    public class UpdateMilestoneStatusDto
    {
        [Required]
        public int MilestoneStatusId { get; set; }
    }
}
