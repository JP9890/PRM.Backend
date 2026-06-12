using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PRMTool.Application.DTOs;
using PRMTool.Application.Services;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using Xunit;

namespace PRMTool.Tests
{
    public class TimesheetServiceTests
    {
        private readonly Mock<ITimesheetRepository> _timesheetRepoMock;
        private readonly Mock<IResourceProfileRepository> _profileRepoMock;
        private readonly Mock<IActivityTagRepository> _activityTagRepoMock;
        private readonly TimesheetService _service;

        public TimesheetServiceTests()
        {
            _timesheetRepoMock = new Mock<ITimesheetRepository>();
            _profileRepoMock = new Mock<IResourceProfileRepository>();
            _activityTagRepoMock = new Mock<IActivityTagRepository>();

            _service = new TimesheetService(
                _timesheetRepoMock.Object,
                _profileRepoMock.Object,
                _activityTagRepoMock.Object);
        }

        [Fact]
        public async Task SubmitTimesheetAsync_ShouldThrow_IfResourceNotFound()
        {
            // Arrange
            _profileRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync((ResourceProfile?)null);

            var dto = new SubmitTimesheetDto { WeekStartDate = "2025-01-01", Entries = new List<SubmitTimesheetEntryDto>() };

            // Act
            Func<Task> act = async () => await _service.SubmitTimesheetAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Resource profile not found.");
        }

        [Fact]
        public async Task SubmitTimesheetAsync_ShouldThrow_IfTimesheetAlreadyExistsForWeek()
        {
            // Arrange
            _profileRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(TestHelper.CreateEntity<ResourceProfile>(1));

            var existingTimesheet = TestHelper.CreateEntity<Timesheet>(1)
                .SetPrivate("ResourceId", 1)
                .SetPrivate("WeekStartDate", DateTime.Now)
                .SetPrivate("TotalHours", 40);

            _timesheetRepoMock.Setup(repo => repo.GetByResourceAndWeekAsync(1, "2025-01-01"))
                .ReturnsAsync(existingTimesheet);

            var dto = new SubmitTimesheetDto { WeekStartDate = "2025-01-01", Entries = new List<SubmitTimesheetEntryDto>() };

            // Act
            Func<Task> act = async () => await _service.SubmitTimesheetAsync(1, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Timesheet already submitted for this week.");
        }

        [Fact]
        public async Task SubmitTimesheetAsync_ShouldSucceed_AndCalculateTotalHours()
        {
            // Arrange
            _profileRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(TestHelper.CreateEntity<ResourceProfile>(1));

            _timesheetRepoMock.Setup(repo => repo.GetByResourceAndWeekAsync(1, "2025-01-01"))
                .ReturnsAsync((Timesheet?)null); // No existing timesheet

            var dto = new SubmitTimesheetDto
            {
                WeekStartDate = "2025-01-01",
                Entries = new List<SubmitTimesheetEntryDto>
                {
                    new SubmitTimesheetEntryDto { ProjectId = 1, HoursWorked = 20, ActivityTagIds = new List<int>{1} },
                    new SubmitTimesheetEntryDto { ProjectId = 2, HoursWorked = 15, CustomTag = "Bugfix" }
                }
            };

            _timesheetRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Timesheet>()))
                .Returns(Task.CompletedTask)
                .Callback<Timesheet>(t => t.SetPrivate("Id", 200));

            var savedTimesheet = TestHelper.CreateEntity<Timesheet>(200)
                .SetPrivate("ResourceId", 1)
                .SetPrivate("WeekStartDate", new DateTime(2025, 1, 1))
                .SetPrivate("TotalHours", 35);

            _timesheetRepoMock.Setup(repo => repo.GetByIdAsync(200))
                .ReturnsAsync(savedTimesheet);

            // Act
            var result = await _service.SubmitTimesheetAsync(1, dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(200);
            result.TotalHours.Should().Be(35); // 20 + 15
        }
    }
}
