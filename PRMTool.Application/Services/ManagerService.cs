using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Enums;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly ITimesheetRepository _timesheetRepository;

        public ManagerService(
            IEmployeeRepository employeeRepository,
            IProjectRepository projectRepository,
            IAllocationRepository allocationRepository,
            ITimesheetRepository timesheetRepository)
        {
            _employeeRepository = employeeRepository;
            _projectRepository = projectRepository;
            _allocationRepository = allocationRepository;
            _timesheetRepository = timesheetRepository;
        }

        public async Task<ManagerDashboardDto> GetDashboardAsync(int managerId)
        {
            var employees = await _employeeRepository.GetByManagerIdAsync(managerId);

            var dtos = employees.Select(e =>
            {
                var now = DateTime.UtcNow;
                var activeAllocations = e.Allocations.Where(a => a.IsActiveOn(now)).ToList();
                var totalAlloc = e.Status == EmployeeStatus.ALLOCATED
                    ? activeAllocations.Sum(a => a.UtilizationPercent)
                    : 0;
                var freePercent = 100 - totalAlloc;

                return new EmployeeDashboardDto
                {
                    Id = e.Id,
                    Name = e.User?.FullName ?? e.FullName,
                    Department = e.Department,
                    Skills = string.Join(", ", e.Skills.Select(s => s.SkillName)),
                    AllocationPercentage = totalAlloc,
                    Availability = totalAlloc == 0 ? "FULL" : $"{freePercent}% free",
                    ActiveAllocations = activeAllocations.Select(a => new EmployeeAllocationSummaryDto
                    {
                        ProjectName = a.Project?.Name ?? string.Empty,
                        UtilizationPercent = a.UtilizationPercent,
                        FromDate = a.FromDate.ToString("dd-MMM-yy"),
                        ToDate = a.ToDate.ToString("dd-MMM-yy")
                    }).ToList()
                };
            }).ToList();

            return new ManagerDashboardDto
            {
                OnBench = dtos.Where(e => e.AllocationPercentage == 0).ToList(),
                Active = dtos.Where(e => e.AllocationPercentage > 0).ToList(),
                BenchCount = dtos.Count(e => e.AllocationPercentage == 0),
                PartialCount = dtos.Count(e => e.AllocationPercentage > 0 && e.AllocationPercentage < 100)
            };
        }

        public async Task<IEnumerable<ManagerProjectDto>> GetManagerProjectsAsync(int managerId)
        {
            var projects = await _projectRepository.GetByManagerIdAsync(managerId);

            return projects.Select(p =>
            {
                var health = ComputeProjectHealth(p.Milestones.ToList());
                return new ManagerProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    EndDate = p.EndDate.ToString("dd-MMM-yy"),
                    Health = health
                };
            }).ToList();
        }

        public async Task<ManagerProjectDetailDto?> GetProjectDetailAsync(int managerId, int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null || project.ManagerId != managerId)
                return null;

            var today = DateTime.UtcNow.Date;
            var activeAllocations = await _allocationRepository.GetActiveByProjectIdAsync(projectId);
            var milestones = project.Milestones.OrderBy(m => m.SortOrder).ToList();

            var health = ComputeProjectHealth(milestones);
            var riskFlags = BuildRiskFlags(milestones, activeAllocations);

            var completedSp = milestones
                .Where(m => m.Status == MilestoneStatus.DONE)
                .Sum(m => m.StoryPoints);

            return new ManagerProjectDetailDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate.ToString("dd-MMM-yy"),
                EndDate = project.EndDate.ToString("dd-MMM-yy"),
                Status = project.Status.ToString(),
                Health = health,
                RiskFlags = riskFlags,
                TotalStoryPoints = project.TotalStoryPoints,
                CompletedStoryPoints = completedSp,
                Milestones = milestones.Select(m => new MilestoneDetailDto
                {
                    Id = m.Id,
                    SortOrder = m.SortOrder,
                    Title = m.Title,
                    DueDate = m.DueDate.ToString("dd-MMM-yy"),
                    StoryPoints = m.StoryPoints,
                    Status = m.Status.ToString(),
                    IsOverdue = m.Status != MilestoneStatus.DONE && m.DueDate.Date < today
                }).ToList(),
                AllocatedResources = activeAllocations.Select(a => new ResourceAllocationDto
                {
                    AllocationId = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee?.FullName ?? string.Empty,
                    UtilizationPercent = a.UtilizationPercent,
                    FromDate = a.FromDate.ToString("dd-MMM-yy"),
                    ToDate = a.ToDate.ToString("dd-MMM-yy")
                }).ToList()
            };
        }

        public async Task<IEnumerable<EmployeeDashboardDto>> GetTeamEmployeesAsync(int managerId)
        {
            var employees = await _employeeRepository.GetByManagerIdAsync(managerId);
            var now = DateTime.UtcNow;

            return employees.Select(e =>
            {
                var activeAllocations = e.Allocations.Where(a => a.IsActiveOn(now)).ToList();
                var totalAlloc = activeAllocations.Sum(a => a.UtilizationPercent);

                return new EmployeeDashboardDto
                {
                    Id = e.Id,
                    Name = e.User?.FullName ?? e.FullName,
                    Department = e.Department,
                    Skills = string.Join(", ", e.Skills.Select(s => s.SkillName)),
                    AllocationPercentage = totalAlloc,
                    Availability = totalAlloc == 0 ? "FULL" : $"{100 - totalAlloc}% free",
                    ActiveAllocations = activeAllocations.Select(a => new EmployeeAllocationSummaryDto
                    {
                        ProjectName = a.Project?.Name ?? string.Empty,
                        UtilizationPercent = a.UtilizationPercent,
                        FromDate = a.FromDate.ToString("dd-MMM-yy"),
                        ToDate = a.ToDate.ToString("dd-MMM-yy")
                    }).ToList()
                };
            }).ToList();
        }

        private static string ComputeProjectHealth(IList<Domain.Entities.Milestone> milestones)
        {
            var today = DateTime.UtcNow.Date;
            bool hasOverdue = milestones.Any(m => m.Status != MilestoneStatus.DONE && m.DueDate.Date < today);
            bool nearingDeadline = milestones.Any(m =>
                m.Status != MilestoneStatus.DONE &&
                m.DueDate.Date >= today &&
                (m.DueDate.Date - today).TotalDays <= 7);

            if (hasOverdue) return "🔴 AT RISK";
            if (nearingDeadline) return "🟡 ATTENTION";
            return "🟢 ON TRACK";
        }

        private static IEnumerable<string> BuildRiskFlags(
            IList<Domain.Entities.Milestone> milestones,
            IEnumerable<Domain.Entities.Allocation> allocations)
        {
            var today = DateTime.UtcNow.Date;
            var flags = new List<string>();

            foreach (var m in milestones.Where(m => m.Status != MilestoneStatus.DONE && m.DueDate.Date < today))
                flags.Add($"✗  {m.Title} milestone is {(today - m.DueDate.Date).Days} day(s) overdue");

            if (!allocations.Any())
                flags.Add("✗  No resources currently allocated to this project");

            if (!flags.Any())
                flags.Add("✓  Resources are correctly allocated");

            return flags;
        }
    }
}

