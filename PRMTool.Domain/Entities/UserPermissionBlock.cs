using System;

namespace PRMTool.Domain.Entities
{
    /// <summary>
    /// Per-user permission deny list. A row here means the user is explicitly BLOCKED
    /// from exercising the given permission, regardless of what their role grants.
    /// Used to freeze SUBMIT_TIMESHEET access for employees who ignore reminders.
    /// </summary>
    public class UserPermissionBlock
    {
        public int Id { get; private set; }

        /// <summary>FK → Users.Id — the user whose permission is blocked.</summary>
        public int UserId { get; private set; }
        public User? User { get; private set; }

        /// <summary>FK → Permissions.Id — the permission being denied.</summary>
        public int PermissionId { get; private set; }
        public Permission? Permission { get; private set; }

        public DateTime BlockedAt { get; private set; }

        protected UserPermissionBlock() { }

        public UserPermissionBlock(int userId, int permissionId)
        {
            UserId = userId;
            PermissionId = permissionId;
            BlockedAt = DateTime.UtcNow;
        }
    }
}
