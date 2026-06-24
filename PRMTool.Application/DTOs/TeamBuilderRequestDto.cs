using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class RoleRequirementDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        public string RequiredSkill { get; set; } = string.Empty;

        [Required]
        public string DesiredProficiency { get; set; } = string.Empty;
    }

    public class TeamBuilderRequestDto
    {
        [Required]
        public int ProjectId { get; set; }

        public string Prompt { get; set; } = string.Empty;

        public List<RoleRequirementDto> Roles { get; set; } = new List<RoleRequirementDto>();
    }
}
