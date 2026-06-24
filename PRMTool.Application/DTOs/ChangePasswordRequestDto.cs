using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class ChangePasswordRequestDto
    {
        public string? OldPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
