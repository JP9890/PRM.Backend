using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [Required]
        public int RoleId { get; set; }

        /// <summary>Required for Manager/Employee roles. Leave null for Admin.</summary>
        public string? Department { get; set; }
    }
}
