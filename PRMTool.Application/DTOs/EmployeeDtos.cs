using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    // ──────────────────────────────────────────────────────
    // ResourceProfile (renamed from Employee) DTOs
    // Identity fields (FullName, Department) now come from User.
    // ──────────────────────────────────────────────────────

    public class ResourceProfileDto
    {
        public int Id { get; set; }

        // From linked User
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // Manager info
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public bool IsActive { get; set; }
        public int TotalUtilisation { get; set; }

        public List<ResourceSkillDto> Skills { get; set; } = new();
        public List<ActiveAllocationDto> ActiveAllocations { get; set; } = new();
    }

    /// <summary>Lightweight summary for lists/tables.</summary>
    public class ResourceProfileSummaryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }

        /// <summary>Computed: BENCH | ALLOCATED | OVER_ALLOCATED based on utilisation.</summary>
        public string AllocationStatus { get; set; } = string.Empty;

        public int TotalUtilisation { get; set; }
    }

    public class ResourceSkillDto
    {
        public int Id { get; set; }
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int ProficiencyLevelId { get; set; }
        public string Proficiency { get; set; } = string.Empty;
    }

    public class ActiveAllocationDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int UtilisationPct { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
    }

    public class UpdateEmployeeBasicDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
    }

    public class AssignManagerDto
    {
        [Required]
        public int EmployeeUserId { get; set; }

        [Required]
        public int ManagerUserId { get; set; }
    }

    public class AddSkillDto
    {
        [Required]
        public string SkillName { get; set; } = string.Empty;

        [Required]
        public int ProficiencyLevelId { get; set; }
    }

    public class UpdateSkillProficiencyDto
    {
        [Required]
        public int ProficiencyLevelId { get; set; }
    }

    public class SkillLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryLabel { get; set; } = string.Empty;
    }

    public class ProficiencyLevelDto
    {
        public int Id { get; set; }
        public string LevelCode { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class SkillCategoryDto
    {
        public int Id { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
