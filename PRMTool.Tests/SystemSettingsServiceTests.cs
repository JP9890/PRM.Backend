using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PRMTool.Application.Services;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using Xunit;

namespace PRMTool.Tests
{
    public class SystemSettingsServiceTests
    {
        private readonly Mock<ISystemSettingRepository> _settingRepoMock;
        private readonly SystemSettingsService _service;

        public SystemSettingsServiceTests()
        {
            _settingRepoMock = new Mock<ISystemSettingRepository>();
            _service = new SystemSettingsService(_settingRepoMock.Object);
        }

        [Theory]
        [InlineData("OpenAI")]
        [InlineData("Anthropic")]
        [InlineData("")]
        public async Task UpdateLlmProviderAsync_GivenInvalidProvider_ThrowsException(string invalidProvider)
        {
            // Act
            Func<Task> act = async () => await _service.UpdateLlmProviderAsync(invalidProvider);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Provider must be Gemini, Groq, or gemma.");
        }

        [Theory]
        [InlineData("Gemini")]
        [InlineData("Groq")]
        [InlineData("gemma")]
        public async Task UpdateLlmProviderAsync_GivenValidProvider_UpdatesSetting(string validProvider)
        {
            // Arrange
            _settingRepoMock.Setup(repo => repo.GetByKeyAsync("LlmProvider"))
                .ReturnsAsync((SystemSetting?)null); // Simulate not exists yet

            _settingRepoMock.Setup(repo => repo.AddAsync(It.IsAny<SystemSetting>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.UpdateLlmProviderAsync(validProvider);

            // Assert
            _settingRepoMock.Verify(repo => repo.AddAsync(It.Is<SystemSetting>(s => s.Key == "LlmProvider" && s.Value == validProvider)), Times.Once);
        }

        [Fact]
        public async Task UpdateSchedulerIntervalAsync_GivenInvalidInterval_ThrowsException()
        {
            // Act
            Func<Task> actZero = async () => await _service.UpdateSchedulerIntervalAsync(0);
            Func<Task> actTooLarge = async () => await _service.UpdateSchedulerIntervalAsync(200);

            // Assert
            await actZero.Should().ThrowAsync<InvalidOperationException>();
            await actTooLarge.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task UpdateMaxWeeklyHoursAsync_GivenInvalidHours_ThrowsException()
        {
            // Act
            Func<Task> actNegative = async () => await _service.UpdateMaxWeeklyHoursAsync(-5);
            Func<Task> actTooLarge = async () => await _service.UpdateMaxWeeklyHoursAsync(200);

            // Assert
            await actNegative.Should().ThrowAsync<InvalidOperationException>();
            await actTooLarge.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
