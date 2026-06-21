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
    public class ResourceProfileService : IResourceProfileService
    {
        private readonly IResourceProfileRepository _profileRepository;
        private readonly IResourceSkillRepository _skillRepository;
        private readonly ISkillRepository _skillLookupRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAllocationRepository _allocationRepository;

        public ResourceProfileService(
            IResourceProfileRepository profileRepository,
            IResourceSkillRepository skillRepository,
            ISkillRepository skillLookupRepository,
            IUserRepository userRepository,
            IAllocationRepository allocationRepository)
        {
            _profileRepository = profileRepository;
            _skillRepository = skillRepository;
            _skillLookupRepository = skillLookupRepository;
            _userRepository = userRepository;
            _allocationRepository = allocationRepository;
        }

        public async Task<IEnumerable<ResourceProfileSummaryDto>> GetAllAsync()
        {
            var profiles = await _profileRepository.GetAllAsync();
            var result = new List<ResourceProfileSummaryDto>();

            foreach (var profile in profiles)
            {
                var utilisation = await GetTotalUtilisationAsync(profile.Id);
                result.Add(new ResourceProfileSummaryDto
                {
                    Id = profile.Id,
                    UserId = profile.UserId,
                    FullName = profile.User?.FullName ?? string.Empty,
                    Department = profile.User?.Department,
                    AllocationStatus = ComputeAllocationStatus(utilisation),
                    TotalUtilisation = utilisation
                });
            }

            return result;
        }

        public async Task<ResourceProfileDto?> GetByIdAsync(int id)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            return profile == null ? null : await MapToDtoAsync(profile);
        }

        public async Task<ResourceProfileDto?> GetByUserIdAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            return profile == null ? null : await MapToDtoAsync(profile);
        }

        public async Task<ResourceProfileDto?> DeactivateAsync(int id)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null)
                return null;

            var today = DateTime.UtcNow.Date;
            var activeAllocations = profile.Allocations
                .Where(a => a.IsActiveOn(today))
                .ToList();

            foreach (var allocation in activeAllocations)
            {
                allocation.EndAllocation(today);
                await _allocationRepository.UpdateAsync(allocation);
            }

            profile.Deactivate();
            await _profileRepository.UpdateAsync(profile);

            if (profile.UserId > 0)
            {
                var user = await _userRepository.GetByIdAsync(profile.UserId);
                if (user != null)
                {
                    user.Deactivate();
                    await _userRepository.UpdateAsync(user);
                }
            }

            return await MapToDtoAsync(profile);
        }

        public async Task<ResourceProfileDto?> AssignManagerAsync(AssignManagerDto dto)
        {
            var profile = await _profileRepository.GetByUserIdAsync(dto.EmployeeUserId)
                ?? throw new InvalidOperationException("Resource profile not found for the given user ID.");

            var manager = await _userRepository.GetByIdAsync(dto.ManagerUserId);
            if (manager == null || manager.Role?.Name != "Manager")
                throw new InvalidOperationException("Manager user not found or user is not a Manager.");

            profile.AssignManager(dto.ManagerUserId);
            await _profileRepository.UpdateAsync(profile);
            return await MapToDtoAsync(profile);
        }

        public async Task<ResourceProfileDto?> UpdateEmployeeAsync(int id, UpdateEmployeeBasicDto dto)
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null)
                return null;

            if (profile.UserId > 0)
            {
                var user = await _userRepository.GetByIdAsync(profile.UserId);
                if (user != null)
                {
                    user.UpdateDetails(dto.FullName, user.Email, user.RoleId, user.IsActive, dto.Department);
                    await _userRepository.UpdateAsync(user);
                }
            }

            return await MapToDtoAsync(profile);
        }

        public async Task<ResourceSkillDto> AddSkillAsync(int resourceProfileId, AddSkillDto dto)
        {
            var profile = await _profileRepository.GetByIdAsync(resourceProfileId)
                ?? throw new InvalidOperationException("Resource profile not found.");

            if (string.IsNullOrWhiteSpace(dto.SkillName))
                throw new InvalidOperationException("Skill name is required.");

            var skillName = dto.SkillName.Trim();
            var skill = await _skillLookupRepository.GetByNameAsync(skillName);
            int skillId = 0;

            if (skill != null)
            {
                skillId = skill.Id;
            }
            else
            {
                // Default new skills to Category "Other" (Id = 5)
                var newSkill = new Skill(5, skillName);
                await _skillLookupRepository.AddAsync(newSkill);
                skillId = newSkill.Id;
            }

            var existingSkills = await _skillRepository.GetByResourceProfileIdAsync(resourceProfileId);
            var alreadyHasSkill = existingSkills.FirstOrDefault(s => s.SkillId == skillId);
            if (alreadyHasSkill != null)
            {
                alreadyHasSkill.UpdateProficiency(dto.ProficiencyLevelId);
                await _skillRepository.UpdateAsync(alreadyHasSkill);
                return await MapSkillToDtoAsync(alreadyHasSkill);
            }

            var resourceSkill = new ResourceSkill(resourceProfileId, skillId, dto.ProficiencyLevelId);
            await _skillRepository.AddAsync(resourceSkill);

            return await MapSkillToDtoAsync(resourceSkill);
        }

        public async Task<ResourceSkillDto?> UpdateSkillProficiencyAsync(int resourceProfileId, int skillId, UpdateSkillProficiencyDto dto)
        {
            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null || skill.ResourceProfileId != resourceProfileId)
                return null;

            skill.UpdateProficiency(dto.ProficiencyLevelId);
            await _skillRepository.UpdateAsync(skill);
            return await MapSkillToDtoAsync(skill);
        }

        public async Task<bool> RemoveSkillAsync(int resourceProfileId, int skillId)
        {
            var skill = await _skillRepository.GetByIdAsync(skillId);
            if (skill == null || skill.ResourceProfileId != resourceProfileId)
                return false;

            await _skillRepository.DeleteAsync(skill);
            return true;
        }

        public async Task<IEnumerable<SkillLookupDto>> GetSkillsAsync()
        {
            var skills = await _skillLookupRepository.GetAllAsync();
            return skills.Select(s => new SkillLookupDto
            {
                Id = s.Id,
                Name = s.Name,
                CategoryId = s.SkillCategoryId,
                CategoryCode = s.SkillCategory?.CategoryCode ?? string.Empty,
                CategoryLabel = s.SkillCategory?.Label ?? string.Empty
            });
        }

        public async Task<IEnumerable<SkillCategoryDto>> GetSkillCategoriesAsync()
        {
            var categories = await _skillLookupRepository.GetAllCategoriesAsync();
            return categories.Select(c => new SkillCategoryDto
            {
                Id = c.Id,
                CategoryCode = c.CategoryCode,
                Label = c.Label
            });
        }

        public async Task<IEnumerable<ProficiencyLevelDto>> GetProficiencyLevelsAsync()
        {
            // ProficiencyLevels are in the DbContext; query them via SkillRepository
            // For now return seeded data as DTOs (they're static lookup rows)
            return new List<ProficiencyLevelDto>
            {
                new() { Id = 1, LevelCode = "BEGINNER",     Label = "Beginner"     },
                new() { Id = 2, LevelCode = "INTERMEDIATE", Label = "Intermediate" },
                new() { Id = 3, LevelCode = "ADVANCED",     Label = "Advanced"     }
            };
        }

      

        private async Task<int> GetTotalUtilisationAsync(int resourceProfileId)
        {
            var allocations = await _allocationRepository.GetActiveByResourceIdAsync(resourceProfileId);
            return allocations.Sum(a => a.UtilisationPct);
        }

        private static string ComputeAllocationStatus(int utilisationPct)
        {
            return utilisationPct switch
            {
                0 => "BENCH",
                > 100 => "OVER_ALLOCATED",
                _ => "ALLOCATED"
            };
        }

        private async Task<ResourceProfileDto> MapToDtoAsync(ResourceProfile profile)
        {
            var utilisation = await GetTotalUtilisationAsync(profile.Id);
            var activeAllocations = await _allocationRepository.GetActiveByResourceIdAsync(profile.Id);
            var skills = await _skillRepository.GetByResourceProfileIdAsync(profile.Id);

            return new ResourceProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                FullName = profile.User?.FullName ?? string.Empty,
                Department = profile.User?.Department,
                RoleName = profile.User?.Role?.Name ?? string.Empty,
                ManagerId = profile.ManagerId,
                ManagerName = profile.Manager?.FullName,
                IsActive = profile.IsActive,
                TotalUtilisation = utilisation,
                Skills = skills.Select(MapSkillToDto).ToList(),
                ActiveAllocations = activeAllocations.Select(a => new ActiveAllocationDto
                {
                    Id = a.Id,
                    ProjectName = a.Project?.Name ?? string.Empty,
                    UtilisationPct = a.UtilisationPct,
                    FromDate = a.FromDate.ToString("dd-MM-yyyy"),
                    ToDate = a.ToDate.ToString("dd-MM-yyyy")
                }).ToList()
            };
        }

        private static ResourceSkillDto MapSkillToDto(ResourceSkill rs)
        {
            return new ResourceSkillDto
            {
                Id = rs.Id,
                SkillId = rs.SkillId,
                SkillName = rs.Skill?.Name ?? string.Empty,
                Category = rs.Skill?.SkillCategory?.Label ?? string.Empty,
                ProficiencyLevelId = rs.ProficiencyLevelId,
                Proficiency = rs.ProficiencyLevel?.Label ?? string.Empty
            };
        }

        private async Task<ResourceSkillDto> MapSkillToDtoAsync(ResourceSkill rs)
        {
            var reloaded = await _skillRepository.GetByIdAsync(rs.Id) ?? rs;
            return MapSkillToDto(reloaded);
        }
        public Task RecomputeAllUtilisationAsync()
        {
            // In the updated schema, Utilisation and Status are dynamically computed
            // from the active Allocations at runtime. No database update is needed.
            return Task.CompletedTask;
        }
    }
}
