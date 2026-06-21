using System;
using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class AllocationDto
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public int ProjectId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int UtilizationPercent { get; set; }
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateAllocationDto
    {
        [Required]
        public int ResourceId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Utilisation must be between 1 and 100.")]
        public int UtilisationPct { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }
    }

    public class EndAllocationResultDto
    {
        public int AllocationId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string EndedOnDate { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
