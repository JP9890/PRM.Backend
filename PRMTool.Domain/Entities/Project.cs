using System;
using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    public class Project
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        /// <summary>FK → ProjectStatus.Id (PLANNED | ACTIVE | ON_HOLD | COMPLETED)</summary>
        public int ProjectStatusId { get; private set; }
        public ProjectStatus? Status { get; private set; }

        /// <summary>FK → Users.Id — must be a MANAGER-role user</summary>
        public int ManagerUserId { get; private set; }
        public User? Manager { get; private set; }

        public int TotalStoryPoints { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public ICollection<Milestone> Milestones { get; private set; } = new List<Milestone>();
        public ICollection<Allocation> Allocations { get; private set; } = new List<Allocation>();
        public ProjectHealth? Health { get; private set; }

        protected Project() { }

        public Project(string name, string description, DateTime startDate, DateTime endDate,
            int projectStatusId, int managerUserId, int totalStoryPoints)
        {
            Name = name;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            ProjectStatusId = projectStatusId;
            ManagerUserId = managerUserId;
            TotalStoryPoints = totalStoryPoints;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string name, string description, DateTime startDate, DateTime endDate,
            int projectStatusId, int managerUserId, int totalStoryPoints)
        {
            Name = name;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            ProjectStatusId = projectStatusId;
            ManagerUserId = managerUserId;
            TotalStoryPoints = totalStoryPoints;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
