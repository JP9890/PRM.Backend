namespace PRMTool.Application.DTOs
{
    public class AIMatchResultDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string SkillsMatch { get; set; } = string.Empty;
        public int AvailabilityPercentage { get; set; } // current utilization pct or available capacity pct
        public string AvailabilityStatus { get; set; } = string.Empty;
        public string MatchReason { get; set; } = string.Empty;
        public int MatchingScore { get; set; } // 0 - 100
        public string RecentActivity { get; set; } = string.Empty;
    }
}
