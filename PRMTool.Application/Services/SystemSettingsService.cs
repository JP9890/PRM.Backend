using System;
using System.Threading.Tasks;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ISystemSettingRepository _settingRepository;

        public SystemSettingsService(ISystemSettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<SystemSettingsDto> GetSettingsAsync()
        {
            var provider = await GetValueAsync("LlmProvider") ?? "Gemini";
            var apiKey = await GetValueAsync("LlmApiKey") ?? "";
            var interval = int.TryParse(await GetValueAsync("SchedulerIntervalHours"), out var h) ? h : 4;
            var maxHours = int.TryParse(await GetValueAsync("MaxWeeklyHours"), out var m) ? m : 40;

            return new SystemSettingsDto
            {
                LlmProvider = provider,
                LlmApiKeyMasked = string.IsNullOrEmpty(apiKey) ? "" : new string('*', Math.Min(apiKey.Length, 20)),
                SchedulerIntervalHours = interval,
                MaxWeeklyHours = maxHours
            };
        }

        public async Task UpdateLlmApiKeyAsync(string apiKey)
        {
            await SetValueAsync("LlmApiKey", apiKey);
        }

        public async Task UpdateLlmProviderAsync(string provider)
        {
            if (provider != "Gemini" && provider != "Groq" && provider != "gemma")
                throw new InvalidOperationException("Provider must be Gemini, Groq, or gemma.");

            await SetValueAsync("LlmProvider", provider);
        }

        public async Task UpdateSchedulerIntervalAsync(int intervalHours)
        {
            if (intervalHours < 1 || intervalHours > 168)
                throw new InvalidOperationException("Scheduler interval must be between 1 and 168 hours.");

            await SetValueAsync("SchedulerIntervalHours", intervalHours.ToString());
        }

        public async Task UpdateMaxWeeklyHoursAsync(int maxWeeklyHours)
        {
            if (maxWeeklyHours < 1 || maxWeeklyHours > 168)
                throw new InvalidOperationException("Max weekly hours must be between 1 and 168.");

            await SetValueAsync("MaxWeeklyHours", maxWeeklyHours.ToString());
        }

        private async Task<string?> GetValueAsync(string key)
        {
            var setting = await _settingRepository.GetByKeyAsync(key);
            return setting?.Value;
        }

        private async Task SetValueAsync(string key, string value)
        {
            var setting = await _settingRepository.GetByKeyAsync(key);
            if (setting == null)
            {
                await _settingRepository.AddAsync(new SystemSetting(key, value));
            }
            else
            {
                setting.UpdateValue(value);
                await _settingRepository.UpdateAsync(setting);
            }
        }
    }
}
