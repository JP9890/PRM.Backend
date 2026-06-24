using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IResourceProfileRepository _profileRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly ITimesheetRepository _timesheetRepository;

        public ManagerService(
            IResourceProfileRepository profileRepository,
            IProjectRepository projectRepository,
            IAllocationRepository allocationRepository,
            ITimesheetRepository timesheetRepository)
        {
            _profileRepository = profileRepository;
            _projectRepository = projectRepository;
            _allocationRepository = allocationRepository;
            _timesheetRepository = timesheetRepository;
        }

        public async Task<ManagerDashboardDto> GetDashboardAsync(int managerId)
        {
            var profiles = await _profileRepository.GetByManagerIdAsync(managerId);

            var dtos = profiles.Select(rp =>
            {
                var now = DateTime.UtcNow;
                var activeAllocations = rp.Allocations.Where(a => a.IsActiveOn(now)).ToList();
                var totalAlloc = activeAllocations.Sum(a => a.UtilisationPct);
                var freePercent = 100 - totalAlloc;

                return new EmployeeDashboardDto
                {
                    Id = rp.Id,
                    Name = rp.User?.FullName ?? string.Empty,
                    Department = rp.User?.Department ?? string.Empty,
                    Skills = string.Join(", ", rp.Skills.Select(s => s.Skill?.Name ?? string.Empty)),
                    AllocationPercentage = totalAlloc,
                    Availability = totalAlloc == 0 ? "FULL" : $"{freePercent}% free",
                    ActiveAllocations = activeAllocations.Select(a => new EmployeeAllocationSummaryDto
                    {
                        ProjectName = a.Project?.Name ?? string.Empty,
                        UtilizationPercent = a.UtilisationPct,
                        FromDate = a.FromDate.ToString("dd/MM/yyyy"),
                        ToDate = a.ToDate.ToString("dd/MM/yyyy")
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
                    EndDate = p.EndDate.ToString("dd/MM/yyyy"),
                    Health = health
                };
            }).ToList();
        }

        public async Task<ManagerProjectDetailDto?> GetProjectDetailAsync(int managerId, int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null || project.ManagerUserId != managerId)
                return null;

            var today = DateTime.UtcNow.Date;
            var activeAllocations = await _allocationRepository.GetActiveByProjectIdAsync(projectId);
            var milestones = project.Milestones.OrderBy(m => m.SortOrder).ToList();

            var health = ComputeProjectHealth(milestones);
            var riskFlags = BuildRiskFlags(milestones, activeAllocations);

            var completedSp = milestones
                .Where(m => m.IsDone)
                .Sum(m => m.StoryPoints);

            return new ManagerProjectDetailDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate.ToString("dd/MM/yyyy"),
                EndDate = project.EndDate.ToString("dd/MM/yyyy"),
                Status = project.Status?.StatusCode ?? string.Empty,
                Health = health,
                RiskFlags = riskFlags,
                TotalStoryPoints = project.TotalStoryPoints,
                CompletedStoryPoints = completedSp,
                Milestones = milestones.Select(m => new MilestoneDetailDto
                {
                    Id = m.Id,
                    SortOrder = m.SortOrder,
                    Title = m.Title,
                    DueDate = m.DueDate.ToString("dd/MM/yyyy"),
                    StoryPoints = m.StoryPoints,
                    Status = m.Status?.StatusCode ?? string.Empty,
                    IsOverdue = m.IsOverdue(today)
                }).ToList(),
                AllocatedResources = activeAllocations.Select(a => new ResourceAllocationDto
                {
                    AllocationId = a.Id,
                    EmployeeId = a.ResourceId,
                    EmployeeName = a.Resource?.User?.FullName ?? string.Empty,
                    UtilizationPercent = a.UtilisationPct,
                    FromDate = a.FromDate.ToString("dd/MM/yyyy"),
                    ToDate = a.ToDate.ToString("dd/MM/yyyy")
                }).ToList()
            };
        }

        public async Task<IEnumerable<EmployeeDashboardDto>> GetTeamEmployeesAsync(int managerId)
        {
            var profiles = await _profileRepository.GetByManagerIdAsync(managerId);
            var now = DateTime.UtcNow;

            return profiles.Select(rp =>
            {
                var activeAllocations = rp.Allocations.Where(a => a.IsActiveOn(now)).ToList();
                var totalAlloc = activeAllocations.Sum(a => a.UtilisationPct);

                return new EmployeeDashboardDto
                {
                    Id = rp.Id,
                    Name = rp.User?.FullName ?? string.Empty,
                    Department = rp.User?.Department ?? string.Empty,
                    Skills = string.Join(", ", rp.Skills.Select(s => s.Skill?.Name ?? string.Empty)),
                    AllocationPercentage = totalAlloc,
                    Availability = totalAlloc == 0 ? "FULL" : $"{100 - totalAlloc}% free",
                    ActiveAllocations = activeAllocations.Select(a => new EmployeeAllocationSummaryDto
                    {
                        ProjectName = a.Project?.Name ?? string.Empty,
                        UtilizationPercent = a.UtilisationPct,
                        FromDate = a.FromDate.ToString("dd/MM/yyyy"),
                        ToDate = a.ToDate.ToString("dd/MM/yyyy")
                    }).ToList()
                };
            }).ToList();
        }

        private static string ComputeProjectHealth(IList<Domain.Entities.Milestone> milestones)
        {
            var today = DateTime.UtcNow.Date;
            bool hasOverdue = milestones.Any(m => m.IsOverdue(today));
            bool nearingDeadline = milestones.Any(m =>
                !m.IsDone &&
                m.DueDate.Date >= today &&
                (m.DueDate.Date - today).TotalDays <= 7);

            if (hasOverdue) return "AT RISK";
            if (nearingDeadline) return "ATTENTION";
            return "ON TRACK";
        }

        private static IEnumerable<string> BuildRiskFlags(
            IList<Domain.Entities.Milestone> milestones,
            IEnumerable<Domain.Entities.Allocation> allocations)
        {
            var today = DateTime.UtcNow.Date;
            var flags = new List<string>();

            foreach (var m in milestones.Where(m => m.IsOverdue(today)))
                flags.Add($"  {m.Title} milestone is {(today - m.DueDate.Date).Days} day(s) overdue");

            if (!allocations.Any())
                flags.Add("No resources currently allocated to this project");

            if (!flags.Any())
                flags.Add("Resources are correctly allocated");

            return flags;
        }
    }
}
