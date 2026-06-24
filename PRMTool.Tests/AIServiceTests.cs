using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PRMTool.Application.DTOs;
using PRMTool.Application.Services;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using Xunit;

namespace PRMTool.Tests
{
    public class AIServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IResourceProfileRepository> _profileRepoMock;
        private readonly Mock<ISystemSettingRepository> _settingRepoMock;
        private readonly AIService _service;

        public AIServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _profileRepoMock = new Mock<IResourceProfileRepository>();
            _settingRepoMock = new Mock<ISystemSettingRepository>();

            _service = new AIService(
                _configMock.Object,
                _projectRepoMock.Object,
                _profileRepoMock.Object,
                _settingRepoMock.Object);
        }

        [Fact]
        public async Task SearchTeamResourcesAsync_WhenNoApiKey_UsesLocalSmartSearchFallback()
        {
            // Arrange
            _settingRepoMock.Setup(repo => repo.GetByKeyAsync("LlmApiKey"))
                .ReturnsAsync((SystemSetting?)null); // No API Key triggers fallback

            var project = TestHelper.CreateEntity<Project>(1)
                .SetPrivate("Name", "C# Migration");
            
            _projectRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(project);

            var profile1 = CreateProfileWithSkill(1, "Java"); // Doesn't match
            var profile2 = CreateProfileWithSkill(2, "C#");   // Matches exactly

            _profileRepoMock.Setup(repo => repo.GetByManagerIdAsync(10))
                .ReturnsAsync(new List<ResourceProfile> { profile1, profile2 });

            // Act
            var results = (await _service.SearchTeamResourcesAsync(10, "Need Java developer", 1)).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.First().EmployeeId.Should().Be(1); // Profile 1 (Java) should be ranked highest
            results.First().SkillsMatch.Should().Be("Java");
            results.First().MatchingScore.Should().Be(70); // Base 50 + 20 for 1 match
        }

        [Fact]
        public async Task GetRiskSummaryAsync_WhenNoApiKey_ReturnsMockSummary()
        {
            // Arrange
            _settingRepoMock.Setup(repo => repo.GetByKeyAsync("LlmApiKey"))
                .ReturnsAsync((SystemSetting?)null);

            var project = TestHelper.CreateEntity<Project>(1)
                .SetPrivate("Name", "Test Project");
            
            _projectRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(project);

            // Act
            var summary = await _service.GetRiskSummaryAsync(1);

            // Assert
            summary.Should().Contain("[MOCK AI]");
            summary.Should().Contain("ON TRACK");
        }

        // Helper
        private ResourceProfile CreateProfileWithSkill(int id, string skillName)
        {
            var profile = TestHelper.CreateEntity<ResourceProfile>(id);
            var user = TestHelper.CreateEntity<User>(id).SetPrivate("FullName", $"User {id}");
            profile.SetPrivate("User", user);

            var skill = TestHelper.CreateEntity<Skill>(1).SetPrivate("Name", skillName);
            var resourceSkill = TestHelper.CreateEntity<ResourceSkill>(1).SetPrivate("Skill", skill);

            profile.SetPrivate("Skills", new List<ResourceSkill> { resourceSkill });
            profile.SetPrivate("Allocations", new List<Allocation>());

            return profile;
        }
    }
}
