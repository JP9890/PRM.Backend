using System;
using System.Collections.Generic;

namespace PRMTool.Application.DTOs
{
    public class ManagerDashboardDto
    {
        public IEnumerable<EmployeeDashboardDto> OnBench { get; set; } = new List<EmployeeDashboardDto>();
        public IEnumerable<EmployeeDashboardDto> Active { get; set; } = new List<EmployeeDashboardDto>();
        public int BenchCount { get; set; }
        public int PartialCount { get; set; }
    }

    public class EmployeeDashboardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty; // Comma-separated
        public int AllocationPercentage { get; set; }
        public string Availability { get; set; } = string.Empty; // e.g., "FULL", "25% free"
        public string RecentActivityTags { get; set; } = string.Empty;
        public IEnumerable<EmployeeAllocationSummaryDto> ActiveAllocations { get; set; } = new List<EmployeeAllocationSummaryDto>();
    }

    public class EmployeeAllocationSummaryDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public int UtilizationPercent { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
    }

    public class ManagerProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Health { get; set; } = string.Empty; // e.g., "🔴 AT RISK", "🟢 ON TRACK", "🟡 ATTENTION"
    }

    public class ManagerProjectDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Health { get; set; } = string.Empty;
        public IEnumerable<string> RiskFlags { get; set; } = new List<string>();
        public IEnumerable<MilestoneDetailDto> Milestones { get; set; } = new List<MilestoneDetailDto>();
        public IEnumerable<ResourceAllocationDto> AllocatedResources { get; set; } = new List<ResourceAllocationDto>();
        public int TotalStoryPoints { get; set; }
        public int CompletedStoryPoints { get; set; }
    }

    public class MilestoneDetailDto
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public int StoryPoints { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public class ResourceAllocationDto
    {
        public int AllocationId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int UtilizationPercent { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
    }
}

