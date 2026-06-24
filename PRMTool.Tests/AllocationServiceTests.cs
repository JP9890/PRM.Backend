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
    public class AllocationServiceTests
    {
        private readonly Mock<IAllocationRepository> _allocationRepoMock;
        private readonly Mock<IResourceProfileRepository> _profileRepoMock;
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly AllocationService _service;

        public AllocationServiceTests()
        {
            _allocationRepoMock = new Mock<IAllocationRepository>();
            _profileRepoMock = new Mock<IResourceProfileRepository>();
            _projectRepoMock = new Mock<IProjectRepository>();

            _service = new AllocationService(
                _allocationRepoMock.Object,
                _profileRepoMock.Object,
                _projectRepoMock.Object);
        }

        [Fact]
        public async Task CreateAllocationAsync_ShouldThrow_IfFromDateIsAfterToDate()
        {
            // Arrange
            var dto = new CreateAllocationDto
            {
                ResourceId = 1,
                ProjectId = 1,
                UtilisationPct = 50,
                FromDate = new DateTime(2025, 2, 1),
                ToDate = new DateTime(2025, 1, 1) // Invalid: Before FromDate
            };

            // Act
            Func<Task> act = async () => await _service.CreateAllocationAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("From Date must be before To Date.");
        }

        [Fact]
        public async Task CreateAllocationAsync_ShouldThrow_IfProjectIsCompleted()
        {
            // Arrange
            var dto = new CreateAllocationDto
            {
                ResourceId = 1,
                ProjectId = 1,
                UtilisationPct = 50,
                FromDate = DateTime.UtcNow.Date,
                ToDate = DateTime.UtcNow.Date.AddDays(30)
            };

            _profileRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(TestHelper.CreateEntity<ResourceProfile>(1));

            var completedStatus = TestHelper.CreateEntity<ProjectStatus>(4).SetPrivate("StatusCode", "COMPLETED");
            var completedProject = TestHelper.CreateEntity<Project>(1)
                .SetPrivate("ProjectStatusId", 4)
                .SetPrivate("Status", completedStatus)
                .SetPrivate("StartDate", DateTime.UtcNow.Date.AddDays(-10))
                .SetPrivate("EndDate", DateTime.UtcNow.Date.AddDays(40));
                
            _projectRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(completedProject);

            // Act
            Func<Task> act = async () => await _service.CreateAllocationAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot allocate to a completed project.");
        }

        [Fact]
        public async Task CreateAllocationAsync_ShouldThrow_IfUtilisationExceeds100Percent()
        {
            // Arrange
            var dto = new CreateAllocationDto
            {
                ResourceId = 1,
                ProjectId = 1,
                UtilisationPct = 60, // Trying to add 60%
                FromDate = DateTime.UtcNow.Date,
                ToDate = DateTime.UtcNow.Date.AddDays(30)
            };

            _profileRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(TestHelper.CreateEntity<ResourceProfile>(1));

            var activeStatus = TestHelper.CreateEntity<ProjectStatus>(2).SetPrivate("StatusCode", "ACTIVE");
            var activeProject = TestHelper.CreateEntity<Project>(1)
                .SetPrivate("Status", activeStatus)
                .SetPrivate("StartDate", DateTime.UtcNow.Date.AddDays(-10))
                .SetPrivate("EndDate", DateTime.UtcNow.Date.AddDays(40));

            _projectRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(activeProject);

            // Mock existing allocation of 50% in the exact same date range
            var existingAllocations = new List<Allocation>
            {
                TestHelper.CreateEntity<Allocation>(50)
                    .SetPrivate("ResourceId", 1)
                    .SetPrivate("ProjectId", 2)
                    .SetPrivate("UtilisationPct", 50)
                    .SetPrivate("FromDate", DateTime.UtcNow.Date)
                    .SetPrivate("ToDate", DateTime.UtcNow.Date.AddDays(30))
            };
            
            _allocationRepoMock.Setup(repo => repo.GetAllActiveAsync())
                .ReturnsAsync(existingAllocations);

            // Act
            Func<Task> act = async () => await _service.CreateAllocationAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Allocation would exceed 100% utilisation. Current: 50%, Adding: 60%.");
        }

        [Fact]
        public async Task CreateAllocationAsync_ShouldSucceed_IfAllRulesPass()
        {
            // Arrange
            var dto = new CreateAllocationDto
            {
                ResourceId = 1,
                ProjectId = 1,
                UtilisationPct = 40,
                FromDate = DateTime.UtcNow.Date,
                ToDate = DateTime.UtcNow.Date.AddDays(30)
            };

            _profileRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(TestHelper.CreateEntity<ResourceProfile>(1));

            var activeStatus = TestHelper.CreateEntity<ProjectStatus>(2).SetPrivate("StatusCode", "ACTIVE");
            var activeProject = TestHelper.CreateEntity<Project>(1)
                .SetPrivate("Status", activeStatus)
                .SetPrivate("StartDate", DateTime.UtcNow.Date.AddDays(-10))
                .SetPrivate("EndDate", DateTime.UtcNow.Date.AddDays(40));

            _projectRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(activeProject);

            _allocationRepoMock.Setup(repo => repo.GetAllActiveAsync())
                .ReturnsAsync(new List<Allocation>()); // No existing allocations

            // We must mock AddAsync to successfully pass the allocation, but it returns Task.
            _allocationRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Allocation>()))
                .Returns(Task.CompletedTask)
                .Callback<Allocation>(a => a.SetPrivate("Id", 100)); // Simulate DB assigning an ID

            // Mock getting the saved entity
            var savedAllocation = TestHelper.CreateEntity<Allocation>(100)
                .SetPrivate("ResourceId", 1)
                .SetPrivate("ProjectId", 1)
                .SetPrivate("UtilisationPct", 40)
                .SetPrivate("FromDate", dto.FromDate)
                .SetPrivate("ToDate", dto.ToDate);
                
            _allocationRepoMock.Setup(repo => repo.GetByIdAsync(100))
                .ReturnsAsync(savedAllocation);

            // Act
            var result = await _service.CreateAllocationAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(100);
            result.UtilizationPercent.Should().Be(40);
        }
    }
}
