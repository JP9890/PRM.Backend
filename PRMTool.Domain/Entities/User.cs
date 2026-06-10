using System;

namespace PRMTool.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public bool RequiresPasswordChange { get; private set; } = false;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        // One-to-one relationship with Role
        public int RoleId { get; private set; }
        public Role? Role { get; private set; }

        // Parameterless constructor for EF Core
        protected User() { }

        public User(string fullName, string username, string email, string passwordHash, int roleId)
        {
            FullName = fullName;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            RoleId = roleId;
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

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            RequiresPasswordChange = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void FlagForPasswordChange()
        {
            RequiresPasswordChange = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string fullName, string email, int roleId, bool isActive)
        {
            FullName = fullName;
            Email = email;
            RoleId = roleId;
            IsActive = isActive;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ResetPassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            RequiresPasswordChange = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
