using System;

namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Scheduler-computed health snapshot per project.
    /// One live row per project (UNIQUE on ProjectId).
    /// </summary>
    public class ProjectHealth
    {
        public int Id { get; private set; }

        /// <summary>UNIQUE — one live row per project.</summary>
        public int ProjectId { get; private set; }
        public Project? Project { get; private set; }

        /// <summary>ON_TRACK | ATTENTION | AT_RISK</summary>
        public string HealthStatus { get; private set; } = string.Empty;

        public bool FlagOverdueMilestone { get; private set; }
        public bool FlagLowHours { get; private set; }
        public bool FlagUnderResourced { get; private set; }
        public string RiskDetail { get; private set; } = string.Empty;
        public DateTime ComputedAt { get; private set; }

        protected ProjectHealth() { }

        public ProjectHealth(int projectId, string healthStatus, bool flagOverdueMilestone,
            bool flagLowHours, bool flagUnderResourced, string riskDetail)
        {
            ProjectId = projectId;
            HealthStatus = healthStatus;
            FlagOverdueMilestone = flagOverdueMilestone;
            FlagLowHours = flagLowHours;
            FlagUnderResourced = flagUnderResourced;
            RiskDetail = riskDetail;
            ComputedAt = DateTime.UtcNow;
        }

        public void Update(string healthStatus, bool flagOverdueMilestone,
            bool flagLowHours, bool flagUnderResourced, string riskDetail)
        {
            HealthStatus = healthStatus;
            FlagOverdueMilestone = flagOverdueMilestone;
            FlagLowHours = flagLowHours;
            FlagUnderResourced = flagUnderResourced;
            RiskDetail = riskDetail;
            ComputedAt = DateTime.UtcNow;
        }
    }
}
