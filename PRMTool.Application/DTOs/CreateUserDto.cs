using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        /// <summary>Required for Manager/Employee roles. Leave null for Admin.</summary>
        public string? Department { get; set; }
    }
}
