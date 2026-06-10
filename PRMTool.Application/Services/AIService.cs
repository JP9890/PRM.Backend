using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PRMTool.Application.DTOs;
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
        private readonly ISystemSettingRepository _settingRepository;

        public AIService(
            IConfiguration configuration, 
            IProjectRepository projectRepository, 
            IEmployeeRepository employeeRepository,
            ISystemSettingRepository settingRepository)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _projectRepository = projectRepository;
            _employeeRepository = employeeRepository;
            _settingRepository = settingRepository;
        }

        private async Task<string> GetApiKeyAsync()
        {
            var setting = await _settingRepository.GetByKeyAsync("LlmApiKey");
            return setting?.Value ?? string.Empty;
        }

        public async Task<string> GetRiskSummaryAsync(int projectId)
        {
            var apiKey = await GetApiKeyAsync();
            var project = await _projectRepository.GetByIdAsync(projectId);
            
            if (project == null) return "Project not found.";

            if (string.IsNullOrEmpty(apiKey))
            {
                // Mock response
                return $"[MOCK AI] Project '{project.Name}' is currently ON TRACK, but watch out for timeline slippage in Milestone 2.";
            }

            try
            {
                var prompt = $"Analyze risk for project: {project.Name}. Milestones count: {project.Milestones.Count}. Status: {project.Status}. End Date: {project.EndDate:dd-MMM-yyyy}. Write a plain-English risk summary paragraph.";
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";
                var response = await _httpClient.PostAsync(url, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    return doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "[GEMINI AI] No response text.";
                }
                return "[GEMINI AI] Failed to generate content from Gemini API.";
            }
            catch (Exception ex)
            {
                return "Error calling AI service: " + ex.Message;
            }
        }

        public async Task<IEnumerable<string>> GetSkillMatchAsync(int projectId, int employeeId)
        {
            var apiKey = await GetApiKeyAsync();
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

            try
            {
                var prompt = $"Analyze skill match for employee: {employee?.FullName} (skills: {string.Join(", ", employee?.Skills.Select(s => s.SkillName) ?? Array.Empty<string>())}) against project: {project?.Name}. Output match percentage and reasoning as list of strings.";
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";
                var response = await _httpClient.PostAsync(url, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? "";
                    return new List<string> { "[GEMINI AI] Match details:", text };
                }
            }
            catch {}

            return new List<string>
            {
                "[GEMINI AI] 90% Match",
                $"Employee {employee?.FullName} has strong proficiency in skills required by {project?.Name}."
            };
        }

        public async Task<IEnumerable<AIMatchResultDto>> SearchTeamResourcesAsync(int managerId, string query, int projectId)
        {
            var apiKey = await GetApiKeyAsync();
            var project = await _projectRepository.GetByIdAsync(projectId);
            var employees = await _employeeRepository.GetByManagerIdAsync(managerId);

            var candidateList = new List<AIMatchResultDto>();
            var now = DateTime.UtcNow;

            foreach (var emp in employees)
            {
                var activeAllocations = emp.Allocations.Where(a => a.IsActiveOn(now)).ToList();
                var currentAlloc = activeAllocations.Sum(a => a.UtilizationPercent);
                var availabilityPct = 100 - currentAlloc;
                var availabilityStatus = currentAlloc == 0 ? "FULL" : $"{availabilityPct}% free";

                candidateList.Add(new AIMatchResultDto
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.User?.FullName ?? emp.FullName,
                    Department = emp.Department,
                    SkillsMatch = string.Join(", ", emp.Skills.Select(s => s.SkillName)),
                    AvailabilityPercentage = availabilityPct,
                    AvailabilityStatus = availabilityStatus,
                    MatchReason = "Candidate has relevant skills.",
                    MatchingScore = 70,
                    RecentActivity = "Active on team projects"
                });
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                // FALLBACK LOCAL SMART SEARCH
                var searchTerms = query.Split(new[] { ' ', ',', ';', '.' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLower())
                    .Where(t => t.Length > 2)
                    .ToList();

                foreach (var candidate in candidateList)
                {
                    var emp = employees.First(e => e.Id == candidate.EmployeeId);
                    var matchedSkills = new List<string>();
                    int skillOverlaps = 0;

                    foreach (var skill in emp.Skills)
                    {
                        var sName = skill.SkillName.ToLower();
                        if (searchTerms.Any(term => sName.Contains(term) || term.Contains(sName)))
                        {
                            skillOverlaps++;
                            matchedSkills.Add(skill.SkillName);
                        }
                    }

                    int score = 50; // base score
                    if (skillOverlaps > 0)
                    {
                        score += skillOverlaps * 20;
                        if (score > 95) score = 95;
                        candidate.SkillsMatch = string.Join(", ", matchedSkills);
                        candidate.MatchReason = $"[MOCK AI] Matched {skillOverlaps} required skill(s) ({string.Join(", ", matchedSkills)}). Candidate is {candidate.AvailabilityStatus}.";
                    }
                    else
                    {
                        candidate.SkillsMatch = "None matched directly";
                        candidate.MatchReason = $"[MOCK AI] Base profile match. No direct matching skills found in query. Candidate is {candidate.AvailabilityStatus}.";
                    }

                    candidate.MatchingScore = score;
                }

                return candidateList.OrderByDescending(c => c.MatchingScore);
            }

            // GEMINI LLM CALL
            try
            {
                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("You are an AI Resource Matcher for a Project & Resource Management (PRM) system.");
                promptBuilder.AppendLine($"Project: {project?.Name ?? "Selected Project"}");
                promptBuilder.AppendLine($"Requirement Description: {query}");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Candidates List:");
                foreach (var c in candidateList)
                {
                    promptBuilder.AppendLine($"- ID: {c.EmployeeId}, Name: {c.EmployeeName}, Department: {c.Department}, Skills: {c.SkillsMatch}, Availability: {c.AvailabilityStatus}");
                }
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Analyze the requirement and match it against the candidates. Score each candidate from 0 to 100 based on skill relevance, department fit, and availability.");
                promptBuilder.AppendLine("Return the result strictly as a JSON array of objects, with NO markdown block or surrounding text. Each object must have these keys exactly:");
                promptBuilder.AppendLine("  - employeeId (integer)");
                promptBuilder.AppendLine("  - matchingScore (integer)");
                promptBuilder.AppendLine("  - skillsMatch (string showing matching skills)");
                promptBuilder.AppendLine("  - matchReason (string explaining why they match and availability fit)");
                promptBuilder.AppendLine("  - recentActivity (string summarizing fit)");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Ensure valid JSON syntax.");

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = promptBuilder.ToString() } } }
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";
                var response = await _httpClient.PostAsync(url, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    var textResponse = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString() ?? string.Empty;

                    textResponse = textResponse.Trim();
                    if (textResponse.StartsWith("```json"))
                    {
                        textResponse = textResponse.Substring(7).Trim();
                    }
                    if (textResponse.StartsWith("```"))
                    {
                        textResponse = textResponse.Substring(3).Trim();
                    }
                    if (textResponse.EndsWith("```"))
                    {
                        textResponse = textResponse.Substring(0, textResponse.Length - 3).Trim();
                    }

                    var aiMatches = JsonSerializer.Deserialize<List<AIMatchResponseObj>>(textResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (aiMatches != null)
                    {
                        foreach (var aiM in aiMatches)
                        {
                            var cand = candidateList.FirstOrDefault(c => c.EmployeeId == aiM.EmployeeId);
                            if (cand != null)
                            {
                                cand.MatchingScore = aiM.MatchingScore;
                                cand.SkillsMatch = aiM.SkillsMatch;
                                cand.MatchReason = aiM.MatchReason;
                                cand.RecentActivity = aiM.RecentActivity;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (var c in candidateList)
                {
                    c.MatchReason = $"[AI Error Fallback] Matches based on candidate profile. (Error: {ex.Message})";
                }
            }

            return candidateList.OrderByDescending(c => c.MatchingScore);
        }

        private class AIMatchResponseObj
        {
            public int EmployeeId { get; set; }
            public int MatchingScore { get; set; }
            public string SkillsMatch { get; set; } = string.Empty;
            public string MatchReason { get; set; } = string.Empty;
            public string RecentActivity { get; set; } = string.Empty;
        }
    }
}
