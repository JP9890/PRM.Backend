using System.Collections.Generic;

namespace PRMTool.Application.DTOs
{
    public class RoleAssignmentDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string MatchedRole { get; set; } = string.Empty;
        public int MatchingScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }

    public class RoleGapDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string GapReason { get; set; } = string.Empty;
    }

    public class TeamBuilderResponseDto
    {
        public List<RoleAssignmentDto> Assignments { get; set; } = new List<RoleAssignmentDto>();
        public List<RoleGapDto> Gaps { get; set; } = new List<RoleGapDto>();
    }
}
