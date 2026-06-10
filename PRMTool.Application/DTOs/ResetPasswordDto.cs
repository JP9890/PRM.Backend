using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
