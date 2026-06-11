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
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly IResourceProfileRepository _profileRepository;
        private readonly IProjectRepository _projectRepository;

        public AllocationService(
            IAllocationRepository allocationRepository,
            IResourceProfileRepository profileRepository,
            IProjectRepository projectRepository)
        {
            _allocationRepository = allocationRepository;
            _profileRepository = profileRepository;
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<AllocationDto>> GetAllActiveAsync(int? resourceId = null, int? projectId = null)
        {
            var allocations = await _allocationRepository.GetAllActiveAsync();

            if (resourceId.HasValue)
                allocations = allocations.Where(a => a.ResourceId == resourceId.Value);

            if (projectId.HasValue)
                allocations = allocations.Where(a => a.ProjectId == projectId.Value);

            return allocations.Select(MapToDto);
        }

        public async Task<IEnumerable<AllocationDto>> GetActiveByProjectAsync(int projectId)
        {
            var allocations = await _allocationRepository.GetActiveByProjectIdAsync(projectId);
            return allocations.Select(MapToDto);
        }

        public async Task<IEnumerable<AllocationDto>> GetByUserIdAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                return Enumerable.Empty<AllocationDto>();

            var allocations = await _allocationRepository.GetAllActiveAsync();
            return allocations
                .Where(a => a.ResourceId == profile.Id)
                .Select(MapToDto);
        }

        public async Task<AllocationDto> CreateAllocationAsync(CreateAllocationDto dto)
        {
            ValidateAllocationDates(dto.FromDate, dto.ToDate);

            var profile = await _profileRepository.GetByIdAsync(dto.ResourceId)
                ?? throw new InvalidOperationException("Resource profile not found.");

            var project = await _projectRepository.GetByIdAsync(dto.ProjectId)
                ?? throw new InvalidOperationException("Project not found.");

            if (project.Status?.StatusCode == "COMPLETED")
                throw new InvalidOperationException("Cannot allocate to a completed project.");

            await ValidateUtilisationLimitAsync(dto.ResourceId, dto.UtilisationPct, dto.FromDate, dto.ToDate);

            var allocation = new Allocation(dto.ResourceId, dto.ProjectId, dto.UtilisationPct, dto.FromDate, dto.ToDate);
            await _allocationRepository.AddAsync(allocation);

            var saved = await _allocationRepository.GetByIdAsync(allocation.Id)
                ?? throw new InvalidOperationException("Failed to retrieve saved allocation.");

            return MapToDto(saved);
        }

        public async Task<EndAllocationResultDto> EndAllocationAsync(int allocationId)
        {
            var allocation = await _allocationRepository.GetByIdAsync(allocationId)
                ?? throw new InvalidOperationException("Allocation not found.");

            var today = DateTime.UtcNow.Date;
            allocation.EndAllocation(today);
            await _allocationRepository.UpdateAsync(allocation);

            return new EndAllocationResultDto
            {
                AllocationId = allocationId,
                ResourceName = allocation.Resource?.User?.FullName ?? string.Empty,
                ProjectName = allocation.Project?.Name ?? string.Empty,
                EndedOnDate = today.ToString("dd-MM-yyyy"),
                Message = $"{allocation.Resource?.User?.FullName} freed from {allocation.Project?.Name} as of {today:dd-MMM-yyyy}."
            };
        }

        private async Task ValidateUtilisationLimitAsync(int resourceId, int newPercent, DateTime fromDate, DateTime toDate)
        {
            var existingAllocations = await _allocationRepository.GetAllActiveAsync();
            var overlapping = existingAllocations
                .Where(a => a.ResourceId == resourceId
                    && a.FromDate.Date <= toDate.Date
                    && a.ToDate.Date >= fromDate.Date);

            var currentTotal = overlapping.Sum(a => a.UtilisationPct);
            if (currentTotal + newPercent > 100)
                throw new InvalidOperationException(
                    $"Allocation would exceed 100% utilisation. Current: {currentTotal}%, Adding: {newPercent}%.");
        }

        private static void ValidateAllocationDates(DateTime fromDate, DateTime toDate)
        {
            if (fromDate >= toDate)
                throw new InvalidOperationException("From Date must be before To Date.");
        }

        private static AllocationDto MapToDto(Allocation a)
        {
            return new AllocationDto
            {
                Id = a.Id,
                ResourceId = a.ResourceId,
                ProjectId = a.ProjectId,
                ResourceName = a.Resource?.User?.FullName ?? string.Empty,
                ProjectName = a.Project?.Name ?? string.Empty,
                UtilisationPct = a.UtilisationPct,
                FromDate = a.FromDate.ToString("dd-MM-yyyy"),
                ToDate = a.ToDate.ToString("dd-MM-yyyy"),
                IsActive = a.IsActive
            };
        }
    }
}
