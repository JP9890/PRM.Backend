using System;
using System.Collections.Generic;
using PRMTool.Domain.Enums;

namespace PRMTool.Domain.Entities
{
    public class Project
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public ProjectStatus Status { get; private set; } = ProjectStatus.PLANNED;
        public int ManagerId { get; private set; }
        public User? Manager { get; private set; }
        public int TotalStoryPoints { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public ICollection<Milestone> Milestones { get; private set; } = new List<Milestone>();
        public ICollection<Allocation> Allocations { get; private set; } = new List<Allocation>();

        protected Project() { }

        public Project(string name, string description, DateTime startDate, DateTime endDate,
            ProjectStatus status, int managerId, int totalStoryPoints)
        {
            Name = name;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            ManagerId = managerId;
            TotalStoryPoints = totalStoryPoints;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string name, string description, DateTime startDate, DateTime endDate,
            ProjectStatus status, int managerId, int totalStoryPoints)
        {
            Name = name;
            Description = description;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
            ManagerId = managerId;
            TotalStoryPoints = totalStoryPoints;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
