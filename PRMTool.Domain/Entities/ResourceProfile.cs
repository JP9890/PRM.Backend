using System;
using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Renamed from EMPLOYEES. Holds only resource-domain data.
    /// Identity fields (FullName, Department) now live on the linked User entity.
    /// </summary>
    public class ResourceProfile
    {
        public int Id { get; private set; }

        /// <summary>1-to-1 → Users.Id (EMPLOYEE or MANAGER role)</summary>
        public int UserId { get; private set; }
        public User? User { get; private set; }

        /// <summary>FK → Users.Id — must be a MANAGER-role user</summary>
        public int? ManagerId { get; private set; }
        public User? Manager { get; private set; }

        /// <summary>Resource-level active flag. Can be deactivated independently of user account.</summary>
        public bool IsActive { get; private set; } = true;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public ICollection<ResourceSkill> Skills { get; private set; } = new List<ResourceSkill>();
        public ICollection<Allocation> Allocations { get; private set; } = new List<Allocation>();
        public ICollection<Timesheet> Timesheets { get; private set; } = new List<Timesheet>();

        protected ResourceProfile() { }

        public ResourceProfile(int userId, int? managerId = null)
        {
            UserId = userId;
            ManagerId = managerId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AssignManager(int? managerId)
        {
            ManagerId = managerId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reactivate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
