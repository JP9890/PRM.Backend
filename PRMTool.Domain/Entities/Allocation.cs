using System;

namespace PRMTool.Domain.Entities
{
    public class Allocation
    {
        public int Id { get; private set; }
        public int EmployeeId { get; private set; }
        public Employee? Employee { get; private set; }
        public int ProjectId { get; private set; }
        public Project? Project { get; private set; }
        public int UtilizationPercent { get; private set; }
        public DateTime FromDate { get; private set; }
        public DateTime ToDate { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        protected Allocation() { }

        public Allocation(int employeeId, int projectId, int utilizationPercent, DateTime fromDate, DateTime toDate)
        {
            EmployeeId = employeeId;
            ProjectId = projectId;
            UtilizationPercent = utilizationPercent;
            FromDate = fromDate;
            ToDate = toDate;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void EndAllocation(DateTime endDate)
        {
            ToDate = endDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsActiveOn(DateTime date)
        {
            return FromDate.Date <= date.Date && ToDate.Date >= date.Date;
        }
    }
}
