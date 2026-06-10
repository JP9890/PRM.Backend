using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PRMTool.Application.Interfaces;
using PRMTool.Domain.Interfaces;

namespace PRMTool.Application.Services
{
    public class AIService : IAIService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IProjectRepository _projectRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public AIService(IConfiguration configuration, IProjectRepository projectRepository, IEmployeeRepository employeeRepository)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _projectRepository = projectRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<string> GetRiskSummaryAsync(int projectId)
        {
            var apiKey = _configuration["LlmApiKey"];
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if (project == null) return "Project not found.";

            if (string.IsNullOrEmpty(apiKey))
            {
                // Mock response
                return $"[MOCK AI] Project '{project.Name}' is currently ON TRACK, but watch out for timeline slippage in Milestone 2.";
            }

            // Real Gemini Call Placeholder
            try
            {
                // Here we would construct the Gemini prompt and payload
                // var response = await _httpClient.PostAsync("https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key=" + apiKey, content);
                return $"[GEMINI AI] Based on current allocations and milestone completion, '{project.Name}' has a moderate risk of delay.";
            }
            catch (Exception ex)
            {
                return "Error calling AI service: " + ex.Message;
            }
        }

        public async Task<IEnumerable<string>> GetSkillMatchAsync(int projectId, int employeeId)
        {
            var apiKey = _configuration["LlmApiKey"];
            var project = await _projectRepository.GetByIdAsync(projectId);
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (string.IsNullOrEmpty(apiKey))
            {
                // Mock response
                return new List<string>
                {
                    "[MOCK AI] 85% Match",
                    $"Employee has matching skills for {project?.Name ?? "project"}."
                };
            }

            // Real Gemini Call Placeholder
            return new List<string>
            {
                "[GEMINI AI] 90% Match",
                $"Employee {employee?.FullName} has strong proficiency in skills required by {project?.Name}."
            };
        }
    }
}
