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
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeSkillRepository _skillRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAllocationRepository _allocationRepository;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IEmployeeSkillRepository skillRepository,
            IUserRepository userRepository,
            IAllocationRepository allocationRepository)
        {
            _employeeRepository = employeeRepository;
            _skillRepository = skillRepository;
            _userRepository = userRepository;
            _allocationRepository = allocationRepository;
        }

        public async Task<IEnumerable<EmployeeSummaryDto>> GetAllAsync(EmployeeStatus? status = null, string? department = null)
        {
            var employees = await _employeeRepository.GetFilteredAsync(status, department);
            var result = new List<EmployeeSummaryDto>();

            foreach (var emp in employees)
            {
                var utilization = await GetTotalUtilizationAsync(emp.Id);
                result.Add(new EmployeeSummaryDto
                {
                    Id = emp.Id,
                    FullName = emp.FullName,
                    Department = emp.Department,
                    Status = emp.Status.ToString(),
                    TotalUtilization = utilization
                });
            }

            return result;
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return null;

            return await MapToDtoAsync(employee);
        }

        public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
        {
            if (dto.UserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(dto.UserId.Value);
                if (user == null)
                    throw new InvalidOperationException("Linked user does not exist.");

                var existing = await _employeeRepository.GetByUserIdAsync(dto.UserId.Value);
                if (existing != null)
                    throw new InvalidOperationException("An employee profile already exists for this user.");
            }

            var employee = new Employee(dto.FullName, dto.Department, dto.UserId);
            await _employeeRepository.AddAsync(employee);
            return await MapToDtoAsync(employee);
        }

        public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return null;

            employee.UpdateDetails(dto.FullName, dto.Department);
            await _employeeRepository.UpdateAsync(employee);
            return await MapToDtoAsync(employee);
        }

        public async Task<EmployeeDto?> DeactivateAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                return null;

            var today = DateTime.UtcNow.Date;
            var activeAllocations = employee.Allocations
                .Where(a => a.FromDate.Date <= today && a.ToDate.Date >= today)
                .ToList();

            foreach (var allocation in activeAllocations)
            {
                allocation.EndAllocation(today);
                await _allocationRepository.UpdateAsync(allocation);
            }

            employee.Deactivate();
            employee.SetStatus(EmployeeStatus.BENCH);
            await _employeeRepository.UpdateAsync(employee);

            if (employee.UserId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(employee.UserId.Value);
                if (user != null)
                {
                    user.Deactivate();
                    await _userRepository.UpdateAsync(user);
                }
            }

            return await MapToDtoAsync(employee);
        }

        public async Task<EmployeeDto?> AssignManagerAsync(AssignManagerDto dto)
        {
            var employee = await _employeeRepository.GetByUserIdAsync(dto.EmployeeUserId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found for the given user ID.");

            var manager = await _userRepository.GetByIdAsync(dto.ManagerUserId);
            if (manager == null || manager.Role?.Name != "Manager")
                throw new InvalidOperationException("Manager user not found or user is not a Manager.");

            employee.AssignManager(dto.ManagerUserId);
            await _employeeRepository.UpdateAsync(employee);
            return await MapToDtoAsync(employee);
        }

        public async Task<EmployeeSkillDto> AddSkillAsync(int employeeId, AddSkillDto dto)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            var skill = new EmployeeSkill(employeeId, dto.SkillName, dto.Category, dto.Proficiency);
            await _skillRepository.AddAsync(skill);

            return MapSkillToDto(skill);
        }

        public async Task<EmployeeSkillDto?> UpdateSkillProficiencyAsync(int employeeId, int skillId, UpdateSkillProficiencyDto dto)
        {
            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null || skill.EmployeeId != employeeId)
                return null;

            skill.UpdateProficiency(dto.Proficiency);
            await _skillRepository.UpdateAsync(skill);
            return MapSkillToDto(skill);
        }

        public async Task<bool> RemoveSkillAsync(int employeeId, int skillId)
        {
            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null || skill.EmployeeId != employeeId)
                return false;

            await _skillRepository.DeleteAsync(skill);
            return true;
        }

        private async Task<int> GetTotalUtilizationAsync(int employeeId)
        {
            var allocations = await _allocationRepository.GetActiveByEmployeeIdAsync(employeeId);
            return allocations.Sum(a => a.UtilizationPercent);
        }

        private async Task<EmployeeDto> MapToDtoAsync(Employee employee)
        {
            var utilization = await GetTotalUtilizationAsync(employee.Id);
            var activeAllocations = await _allocationRepository.GetActiveByEmployeeIdAsync(employee.Id);
            var skills = await _skillRepository.GetByEmployeeIdAsync(employee.Id);

            return new EmployeeDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Department = employee.Department,
                Status = employee.Status.ToString(),
                IsActive = employee.IsActive,
                UserId = employee.UserId,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager?.FullName,
                TotalUtilization = utilization,
                Skills = skills.Select(MapSkillToDto).ToList(),
                ActiveAllocations = activeAllocations.Select(a => new ActiveAllocationDto
                {
                    Id = a.Id,
                    ProjectName = a.Project?.Name ?? "",
                    UtilizationPercent = a.UtilizationPercent,
                    FromDate = a.FromDate.ToString("dd-MM-yyyy"),
                    ToDate = a.ToDate.ToString("dd-MM-yyyy")
                }).ToList()
            };
        }

        private static EmployeeSkillDto MapSkillToDto(EmployeeSkill skill)
        {
            return new EmployeeSkillDto
            {
                Id = skill.Id,
                SkillName = skill.SkillName,
                Category = skill.Category.ToString(),
                Proficiency = skill.Proficiency.ToString()
            };
        }
    }
}
