using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class AISearchRequestDto
    {
        [Required]
        public int ManagerId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [MinLength(3)]
        public string Query { get; set; } = string.Empty;
    }
}
