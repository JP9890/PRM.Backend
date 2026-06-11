using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly IUserRepository _userRepository;

        public ProjectService(
            IProjectRepository projectRepository,
            IMilestoneRepository milestoneRepository,
            IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _milestoneRepository = milestoneRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            return projects.Select(MapToDto);
        }

        public async Task<ProjectDto?> GetByIdAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            return project == null ? null : MapToDto(project);
        }

        public async Task<ProjectDto> CreateAsync(CreateProjectDto dto)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new InvalidOperationException("Start date must be before end date.");

            var manager = await _userRepository.GetByIdAsync(dto.ManagerId);
            if (manager == null || manager.Role?.Name != "Manager")
                throw new InvalidOperationException("Manager not found or user is not a Manager.");

            var project = new Project(
                dto.Name, dto.Description, dto.StartDate, dto.EndDate,
                dto.ProjectStatusId, dto.ManagerId, dto.TotalStoryPoints);

            await _projectRepository.AddAsync(project);
            var created = await _projectRepository.GetByIdAsync(project.Id);
            return MapToDto(created!);
        }

        public async Task<ProjectDto?> UpdateAsync(int id, UpdateProjectDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
                return null;

            if (dto.StartDate >= dto.EndDate)
                throw new InvalidOperationException("Start date must be before end date.");

            var manager = await _userRepository.GetByIdAsync(dto.ManagerId);
            if (manager == null || manager.Role?.Name != "Manager")
                throw new InvalidOperationException("Manager not found or user is not a Manager.");

            project.UpdateDetails(
                dto.Name, dto.Description, dto.StartDate, dto.EndDate,
                dto.ProjectStatusId, dto.ManagerId, dto.TotalStoryPoints);

            await _projectRepository.UpdateAsync(project);
            var updated = await _projectRepository.GetByIdAsync(id);
            return MapToDto(updated!);
        }

        public async Task<MilestoneDto> AddMilestoneAsync(int projectId, CreateMilestoneDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new InvalidOperationException("Project not found.");

            var existing = await _milestoneRepository.GetByProjectIdAsync(projectId);
            var sortOrder = existing.Any() ? existing.Max(m => m.SortOrder) + 1 : 1;

            // milestoneStatusId: 1 = NOT_STARTED
            var milestone = new Milestone(projectId, dto.Title, dto.DueDate, dto.StoryPoints, sortOrder, milestoneStatusId: 1);
            await _milestoneRepository.AddAsync(milestone);
            return MapMilestoneToDto(milestone);
        }

        public async Task<MilestoneDto?> UpdateMilestoneStatusAsync(int projectId, int milestoneId, UpdateMilestoneStatusDto dto)
        {
            var milestone = await _milestoneRepository.GetByIdAsync(milestoneId);
            if (milestone == null || milestone.ProjectId != projectId)
                return null;

            milestone.UpdateStatus(dto.MilestoneStatusId);
            await _milestoneRepository.UpdateAsync(milestone);
            return MapMilestoneToDto(milestone);
        }

        private static ProjectDto MapToDto(Project project)
        {
            var completedSp = project.Milestones
                .Where(m => m.IsDone)
                .Sum(m => m.StoryPoints);

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate.ToString("dd-MM-yyyy"),
                EndDate = project.EndDate.ToString("dd-MM-yyyy"),
                Status = project.Status?.StatusCode ?? string.Empty,
                ManagerId = project.ManagerUserId,
                ManagerName = project.Manager?.FullName,
                TotalStoryPoints = project.TotalStoryPoints,
                CompletedStoryPoints = completedSp,
                Milestones = project.Milestones
                    .OrderBy(m => m.SortOrder)
                    .Select(MapMilestoneToDto)
                    .ToList()
            };
        }

        private static MilestoneDto MapMilestoneToDto(Milestone milestone)
        {
            return new MilestoneDto
            {
                Id = milestone.Id,
                ProjectId = milestone.ProjectId,
                SortOrder = milestone.SortOrder,
                Title = milestone.Title,
                DueDate = milestone.DueDate.ToString("dd-MM-yyyy"),
                StoryPoints = milestone.StoryPoints,
                Status = milestone.Status?.StatusCode ?? string.Empty
            };
        }
    }
}
