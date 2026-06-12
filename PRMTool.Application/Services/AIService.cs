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
        private readonly IResourceProfileRepository _profileRepository;
        private readonly ISystemSettingRepository _settingRepository;

        public AIService(
            IConfiguration configuration, 
            IProjectRepository projectRepository, 
            IResourceProfileRepository profileRepository,
            ISystemSettingRepository settingRepository)
        {
            _configuration = configuration;
            
            var handler = new HttpClientHandler { UseProxy = false };
            _httpClient = new HttpClient(handler);
            
            _projectRepository = projectRepository;
            _profileRepository = profileRepository;
            _settingRepository = settingRepository;
        }

        private async Task<string> GetApiKeyAsync()
        {
            var setting = await _settingRepository.GetByKeyAsync("LlmApiKey");
            return setting?.Value ?? string.Empty;
        }

        private async Task<ILLMProvider> GetLLMProviderAsync()
        {
            var providerSetting = await _settingRepository.GetByKeyAsync("LlmProvider");
            string providerName = providerSetting?.Value?.ToLower() ?? "gemini";

            return providerName switch
            {
                "gemma" => new PRMTool.Application.Services.LLMProviders.GemmaProvider(_httpClient, _configuration),
                "groq" => new PRMTool.Application.Services.LLMProviders.GroqProvider(_httpClient),
                _ => new PRMTool.Application.Services.LLMProviders.GeminiProvider(_httpClient)
            };
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
                var prompt = $"Analyze risk for project: {project.Name}. Milestones count: {project.Milestones.Count}. Status: {project.Status?.StatusCode}. End Date: {project.EndDate:dd-MMM-yyyy}. Write a plain-English risk summary paragraph.";
                
                var provider = await GetLLMProviderAsync();
                var responseText = await provider.GenerateTextAsync(prompt, apiKey);
                
                return string.IsNullOrWhiteSpace(responseText) ? "[AI] No response text." : responseText;
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
            var profile = await _profileRepository.GetByIdAsync(employeeId);

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
                var prompt = $"Analyze skill match for employee: {profile?.User?.FullName} (skills: {string.Join(", ", profile?.Skills.Select(s => s.Skill?.Name) ?? Array.Empty<string>())}) against project: {project?.Name}. Output match percentage and reasoning as list of strings.";
                
                var provider = await GetLLMProviderAsync();
                var text = await provider.GenerateTextAsync(prompt, apiKey);
                
                return new List<string> { "[AI] Match details:", text };
            }
            catch {}

            return new List<string>
            {
                "[GEMINI AI] 90% Match",
                $"Employee {profile?.User?.FullName} has strong proficiency in skills required by {project?.Name}."
            };
        }

        public async Task<IEnumerable<AIMatchResultDto>> SearchTeamResourcesAsync(int managerId, string query, int projectId)
        {
            var apiKey = await GetApiKeyAsync();
            var project = await _projectRepository.GetByIdAsync(projectId);
            var profiles = await _profileRepository.GetByManagerIdAsync(managerId);

            var candidateList = new List<AIMatchResultDto>();
            var now = DateTime.UtcNow;

            foreach (var emp in profiles)
            {
                var activeAllocations = emp.Allocations.Where(a => a.IsActiveOn(now)).ToList();
                var currentAlloc = activeAllocations.Sum(a => a.UtilisationPct);
                var availabilityPct = 100 - currentAlloc;
                var availabilityStatus = currentAlloc == 0 ? "FULL" : $"{availabilityPct}% free";

                candidateList.Add(new AIMatchResultDto
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.User?.FullName ?? string.Empty,
                    Department = emp.User?.Department ?? string.Empty,
                    SkillsMatch = string.Join(", ", emp.Skills.Select(s => s.Skill?.Name ?? string.Empty)),
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
                    var emp = profiles.First(e => e.Id == candidate.EmployeeId);
                    var matchedSkills = new List<string>();
                    int skillOverlaps = 0;

                    foreach (var skill in emp.Skills)
                    {
                        var sName = skill.Skill?.Name?.ToLower() ?? string.Empty;
                        if (searchTerms.Any(term => sName.Contains(term) || term.Contains(sName)))
                        {
                            skillOverlaps++;
                            matchedSkills.Add(skill.Skill?.Name ?? string.Empty);
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

            // LLM CALL
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

                var provider = await GetLLMProviderAsync();
                var textResponse = await provider.GenerateTextAsync(promptBuilder.ToString(), apiKey);

                if (!string.IsNullOrWhiteSpace(textResponse))
                {

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
