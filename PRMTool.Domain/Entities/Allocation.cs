using System;

namespace PRMTool.Domain.Entities
{
    public class Allocation
    {
        public int Id { get; private set; }

        /// <summary>FK → ResourceProfile.Id (renamed from EmployeeId)</summary>
        public int ResourceId { get; private set; }
        public ResourceProfile? Resource { get; private set; }

        public int ProjectId { get; private set; }
        public Project? Project { get; private set; }

        /// <summary>1–100. Sum across overlapping date ranges must not exceed 100.</summary>
        public int UtilisationPct { get; private set; }

        public DateTime FromDate { get; private set; }
        public DateTime ToDate { get; private set; }
        public bool IsActive { get; private set; } = true;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        protected Allocation() { }

        public Allocation(int resourceId, int projectId, int utilisationPct, DateTime fromDate, DateTime toDate)
        {
            ResourceId = resourceId;
            ProjectId = projectId;
            UtilisationPct = utilisationPct;
            FromDate = fromDate;
            ToDate = toDate;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void EndAllocation(DateTime endDate)
        {
            ToDate = endDate;
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsActiveOn(DateTime date)
        {
            return IsActive && FromDate.Date <= date.Date && ToDate.Date >= date.Date;
        }
    }
}
