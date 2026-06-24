using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRMTool.Application.DTOs;
using PRMTool.Application.Interfaces;

namespace PRMTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SystemSettingsController : ControllerBase
    {
        private readonly ISystemSettingsService _settingsService;

        public SystemSettingsController(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Ok(settings);
        }

        [HttpPut("llm-api-key")]
        public async Task<IActionResult> UpdateLlmApiKey([FromBody] UpdateLlmApiKeyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _settingsService.UpdateLlmApiKeyAsync(dto.ApiKey);
            return Ok(new { message = "LLM API key updated successfully" });
        }

        [HttpPut("llm-provider")]
        public async Task<IActionResult> UpdateLlmProvider([FromBody] UpdateLlmProviderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _settingsService.UpdateLlmProviderAsync(dto.Provider);
                return Ok(new { message = "LLM provider updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("scheduler-interval")]
        public async Task<IActionResult> UpdateSchedulerInterval([FromBody] UpdateSchedulerIntervalDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _settingsService.UpdateSchedulerIntervalAsync(dto.IntervalHours);
                return Ok(new { message = "Scheduler interval updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("max-weekly-hours")]
        public async Task<IActionResult> UpdateMaxWeeklyHours([FromBody] UpdateMaxWeeklyHoursDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _settingsService.UpdateMaxWeeklyHoursAsync(dto.MaxWeeklyHours);
                return Ok(new { message = "Max weekly hours updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
