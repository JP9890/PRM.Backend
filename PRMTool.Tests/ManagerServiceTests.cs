using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PRMTool.Application.Services;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using Xunit;

namespace PRMTool.Tests
{
    public class ManagerServiceTests
    {
        private readonly Mock<IResourceProfileRepository> _profileRepoMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IAllocationRepository> _allocationRepoMock;
        private readonly Mock<ITimesheetRepository> _timesheetRepoMock;
        private readonly ManagerService _service;

        public ManagerServiceTests()
        {
            _profileRepoMock = new Mock<IResourceProfileRepository>();
            _projectRepoMock = new Mock<IProjectRepository>();
            _allocationRepoMock = new Mock<IAllocationRepository>();
            _timesheetRepoMock = new Mock<ITimesheetRepository>();

            _service = new ManagerService(
                _profileRepoMock.Object,
                _projectRepoMock.Object,
                _allocationRepoMock.Object,
                _timesheetRepoMock.Object);
        }

        [Fact]
        public async Task GetDashboardAsync_GivenEmployeesWithAllocations_CalculatesBenchAndActiveCorrectly()
        {
            // Arrange
            var profiles = new List<ResourceProfile>
            {
                CreateMockProfileWithAllocation(1, 100), // Active (100%)
                CreateMockProfileWithAllocation(2, 50),  // Partial (50%)
                CreateMockProfileWithAllocation(3, 0)    // Bench (0%)
            };

            _profileRepoMock.Setup(repo => repo.GetByManagerIdAsync(1))
                .ReturnsAsync(profiles);

            // Act
            var dashboard = await _service.GetDashboardAsync(1);

            // Assert
            dashboard.Should().NotBeNull();
            dashboard.BenchCount.Should().Be(1);
            dashboard.PartialCount.Should().Be(1);
            dashboard.OnBench.Should().ContainSingle(e => e.Id == 3);
            dashboard.Active.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetProjectDetailAsync_GivenOverdueMilestones_CalculatesAtRiskHealth()
        {
            // Arrange
            var managerId = 1;
            var projectId = 100;

            var project = TestHelper.CreateEntity<Project>(projectId)
                .SetPrivate("ManagerUserId", managerId);

            var overdueMilestone = TestHelper.CreateEntity<Milestone>(1)
                .SetPrivate("DueDate", DateTime.UtcNow.AddDays(-5)) // 5 days ago
                .SetPrivate("MilestoneStatusId", 1) // NOT DONE
                .SetPrivate("Status", TestHelper.CreateEntity<MilestoneStatus>(1).SetPrivate("StatusCode", "IN_PROGRESS"));

            // Project.Milestones has private setter and is initialized as empty list. Use reflection to set it.
            var milestonesList = new List<Milestone> { overdueMilestone };
            project.SetPrivate("Milestones", milestonesList);

            _projectRepoMock.Setup(repo => repo.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _allocationRepoMock.Setup(repo => repo.GetActiveByProjectIdAsync(projectId))
                .ReturnsAsync(new List<Allocation>());

            // Act
            var details = await _service.GetProjectDetailAsync(managerId, projectId);

            // Assert
            details.Should().NotBeNull();
            details!.Health.Should().Be("AT RISK");
            details.RiskFlags.Should().Contain(flag => flag.Contains("overdue"));
        }

        [Fact]
        public async Task GetProjectDetailAsync_GivenUpcomingDeadline_CalculatesAttentionHealth()
        {
            // Arrange
            var managerId = 1;
            var projectId = 100;

            var project = TestHelper.CreateEntity<Project>(projectId)
                .SetPrivate("ManagerUserId", managerId);

            var upcomingMilestone = TestHelper.CreateEntity<Milestone>(1)
                .SetPrivate("DueDate", DateTime.UtcNow.AddDays(3)) // Due in 3 days
                .SetPrivate("MilestoneStatusId", 1) // NOT DONE
                .SetPrivate("Status", TestHelper.CreateEntity<MilestoneStatus>(1).SetPrivate("StatusCode", "IN_PROGRESS"));

            var milestonesList = new List<Milestone> { upcomingMilestone };
            project.SetPrivate("Milestones", milestonesList);

            _projectRepoMock.Setup(repo => repo.GetByIdAsync(projectId))
                .ReturnsAsync(project);

            _allocationRepoMock.Setup(repo => repo.GetActiveByProjectIdAsync(projectId))
                .ReturnsAsync(new List<Allocation>());

            // Act
            var details = await _service.GetProjectDetailAsync(managerId, projectId);

            // Assert
            details.Should().NotBeNull();
            details!.Health.Should().Be("ATTENTION");
        }

        // Helper
        private ResourceProfile CreateMockProfileWithAllocation(int profileId, int allocationPercent)
        {
            var profile = TestHelper.CreateEntity<ResourceProfile>(profileId);
            
            var user = TestHelper.CreateEntity<User>(profileId)
                .SetPrivate("FullName", $"User {profileId}");
            
            profile.SetPrivate("User", user);

            var allocations = new List<Allocation>();
            if (allocationPercent > 0)
            {
                var alloc = TestHelper.CreateEntity<Allocation>(profileId)
                    .SetPrivate("UtilisationPct", allocationPercent)
                    .SetPrivate("FromDate", DateTime.UtcNow.AddDays(-10))
                    .SetPrivate("ToDate", DateTime.UtcNow.AddDays(10));
                
                allocations.Add(alloc);
            }

            profile.SetPrivate("Allocations", allocations);
            return profile;
        }
    }
}
