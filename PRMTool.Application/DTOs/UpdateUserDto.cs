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
    }
}
