using System.Threading.Tasks;
using PRMTool.Application.DTOs;

namespace PRMTool.Application.Interfaces
{
    public interface ISystemSettingsService
    {
        Task<SystemSettingsDto> GetSettingsAsync();
        Task UpdateLlmApiKeyAsync(string apiKey);
        Task UpdateLlmProviderAsync(string provider);
        Task UpdateSchedulerIntervalAsync(int intervalHours);
        Task UpdateMaxWeeklyHoursAsync(int maxWeeklyHours);
    }
}
