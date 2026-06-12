using System.Collections.Generic;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface IResourceProfileService
    {
        Task<IEnumerable<ResourceProfileSummaryDto>> GetAllAsync();
        Task<ResourceProfileDto?> GetByIdAsync(int id);
        Task<ResourceProfileDto?> GetByUserIdAsync(int userId);
        Task<ResourceProfileDto?> DeactivateAsync(int id);
        Task<ResourceProfileDto?> AssignManagerAsync(AssignManagerDto dto);

        // Skills
        Task<ResourceSkillDto> AddSkillAsync(int resourceProfileId, AddSkillDto dto);
        Task<ResourceSkillDto?> UpdateSkillProficiencyAsync(int resourceProfileId, int skillId, UpdateSkillProficiencyDto dto);
        Task<bool> RemoveSkillAsync(int resourceProfileId, int skillId);
        Task RecomputeAllUtilisationAsync();

        // Lookups
        Task<IEnumerable<SkillLookupDto>> GetSkillsAsync();
        Task<IEnumerable<SkillCategoryDto>> GetSkillCategoriesAsync();
        Task<IEnumerable<ProficiencyLevelDto>> GetProficiencyLevelsAsync();
    }
}
