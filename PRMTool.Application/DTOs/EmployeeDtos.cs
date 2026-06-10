using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PRMTool.Domain.Enums;

namespace PRMTool.Application.DTOs
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? UserId { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public int TotalUtilization { get; set; }
        public List<EmployeeSkillDto> Skills { get; set; } = new();
        public List<ActiveAllocationDto> ActiveAllocations { get; set; } = new();
    }

    public class EmployeeSkillDto
    {
        public int Id { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
    }

    public class ActiveAllocationDto
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int UtilizationPercent { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
    }

    public class EmployeeSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalUtilization { get; set; }
    }

    public class CreateEmployeeDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;

        public int? UserId { get; set; }
    }

    public class UpdateEmployeeDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Department { get; set; } = string.Empty;
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
        public SkillCategory Category { get; set; }

        [Required]
        public ProficiencyLevel Proficiency { get; set; }
    }

    public class UpdateSkillProficiencyDto
    {
        [Required]
        public ProficiencyLevel Proficiency { get; set; }
    }
}
