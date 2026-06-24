using System.ComponentModel.DataAnnotations;

namespace PRMTool.Application.DTOs
{
    public class SystemSettingsDto
    {
        public string LlmProvider { get; set; } = string.Empty;
        public string LlmApiKeyMasked { get; set; } = string.Empty;
        public int SchedulerIntervalHours { get; set; }
        public int MaxWeeklyHours { get; set; }
    }

    public class UpdateLlmApiKeyDto
    {
        [Required]
        public string ApiKey { get; set; } = string.Empty;
    }

    public class UpdateLlmProviderDto
    {
        [Required]
        public string Provider { get; set; } = string.Empty;
    }

    public class UpdateSchedulerIntervalDto
    {
        [Required]
        [Range(1, 168)]
        public int IntervalHours { get; set; }
    }

    public class UpdateMaxWeeklyHoursDto
    {
        [Required]
        [Range(1, 168)]
        public int MaxWeeklyHours { get; set; }
    }
}
