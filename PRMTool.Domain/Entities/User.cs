using System;
using System.Collections.Generic;

namespace PRMTool.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;

        /// <summary>
        /// NULL for ADMIN role. Department is an identity field, not resource-specific.
        /// </summary>
        public string? Department { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;

        /// <summary>
        /// Replaces RequiresPasswordChange bool. If TODAY >= PasswordExpirationDate → force change on login.
        /// NULL = password never expires (user has set their own password).
        /// Set to CURRENT_DATE to force change on next login (new accounts / password reset).
        /// </summary>
        public DateTime? PasswordExpirationDate { get; private set; }

        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // FK → ROLES
        public int RoleId { get; private set; }
        public Role? Role { get; private set; }

        // Navigation: one User can have one ResourceProfile (as Employee/Manager)
        public ResourceProfile? ResourceProfile { get; private set; }

        // Computed — does NOT map to a column; evaluated from PasswordExpirationDate
        public bool RequiresPasswordChange =>
            PasswordExpirationDate.HasValue && DateTime.UtcNow.Date >= PasswordExpirationDate.Value.Date;

        protected User() { }

        public User(string fullName, string username, string email, string passwordHash, int roleId, string? department = null)
        {
            FullName = fullName;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            RoleId = roleId;
            Department = department;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Called when the user successfully sets their own password. Clears the expiration date (never expires).
        /// </summary>
        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            PasswordExpirationDate = null;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Called on account creation or password reset by admin. Sets expiration to today so the
        /// computed RequiresPasswordChange property returns true on next login.
        /// </summary>
        public void FlagForPasswordChange()
        {
            PasswordExpirationDate = DateTime.UtcNow.Date;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string fullName, string email, int roleId, bool isActive, string? department = null)
        {
            FullName = fullName;
            Email = email;
            RoleId = roleId;
            IsActive = isActive;
            Department = department;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ResetPassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            FlagForPasswordChange();
        }
    }
}
