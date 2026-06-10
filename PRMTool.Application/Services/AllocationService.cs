using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Enums;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IProjectRepository _projectRepository;

        public AllocationService(
            IAllocationRepository allocationRepository,
            IEmployeeRepository employeeRepository,
            IProjectRepository projectRepository)
        {
            _allocationRepository = allocationRepository;
            _employeeRepository = employeeRepository;
            _projectRepository = projectRepository;
        }

        public async Task<IEnumerable<AllocationDto>> GetAllActiveAsync(int? employeeId = null, int? projectId = null)
        {
            var allocations = await _allocationRepository.GetAllActiveAsync();

            if (employeeId.HasValue)
                allocations = allocations.Where(a => a.EmployeeId == employeeId.Value);

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
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            if (employee == null)
                return Enumerable.Empty<AllocationDto>();

            var allocations = await _allocationRepository.GetAllActiveAsync();
            return allocations
                .Where(a => a.EmployeeId == employee.Id)
                .Select(MapToDto);
        }

        public async Task<AllocationDto> CreateAllocationAsync(CreateAllocationDto dto)
        {
            ValidateAllocationDates(dto.FromDate, dto.ToDate);

            var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId)
                ?? throw new InvalidOperationException("Employee not found.");

            var project = await _projectRepository.GetByIdAsync(dto.ProjectId)
                ?? throw new InvalidOperationException("Project not found.");

            if (project.Status == ProjectStatus.COMPLETED)
                throw new InvalidOperationException("Cannot allocate to a completed project.");

            await ValidateUtilizationLimitAsync(dto.EmployeeId, dto.UtilizationPercent, dto.FromDate, dto.ToDate);

            var allocation = new Allocation(dto.EmployeeId, dto.ProjectId, dto.UtilizationPercent, dto.FromDate, dto.ToDate);
            await _allocationRepository.AddAsync(allocation);

            employee.SetStatus(EmployeeStatus.ALLOCATED);
            await _employeeRepository.UpdateAsync(employee);

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

            // Recompute employee status — if no more active allocations, set to BENCH
            var remainingActive = await _allocationRepository.GetActiveByEmployeeIdAsync(allocation.EmployeeId);
            var employee = await _employeeRepository.GetByIdAsync(allocation.EmployeeId);
            if (employee != null && !remainingActive.Any())
            {
                employee.SetStatus(EmployeeStatus.BENCH);
                await _employeeRepository.UpdateAsync(employee);
            }

            return new EndAllocationResultDto
            {
                AllocationId = allocationId,
                EmployeeName = allocation.Employee?.FullName ?? string.Empty,
                ProjectName = allocation.Project?.Name ?? string.Empty,
                EndedOnDate = today.ToString("dd-MM-yyyy"),
                Message = $"{allocation.Employee?.FullName} freed from {allocation.Project?.Name} as of {today:dd-MMM-yyyy}."
            };
        }

        private async Task ValidateUtilizationLimitAsync(int employeeId, int newPercent, DateTime fromDate, DateTime toDate)
        {
            var existingAllocations = await _allocationRepository.GetAllActiveAsync();
            var overlapping = existingAllocations
                .Where(a => a.EmployeeId == employeeId
                    && a.FromDate.Date <= toDate.Date
                    && a.ToDate.Date >= fromDate.Date);

            var currentTotal = overlapping.Sum(a => a.UtilizationPercent);
            if (currentTotal + newPercent > 100)
                throw new InvalidOperationException(
                    $"Allocation would exceed 100% utilization. Current: {currentTotal}%, Adding: {newPercent}%.");
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
                EmployeeId = a.EmployeeId,
                ProjectId = a.ProjectId,
                EmployeeName = a.Employee?.FullName ?? string.Empty,
                ProjectName = a.Project?.Name ?? string.Empty,
                UtilizationPercent = a.UtilizationPercent,
                FromDate = a.FromDate.ToString("dd-MM-yyyy"),
                ToDate = a.ToDate.ToString("dd-MM-yyyy")
            };
        }
    }
}

