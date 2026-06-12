using System;
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
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _projectRepoMock;
        private readonly Mock<IMilestoneRepository> _milestoneRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly ProjectService _service;

        public ProjectServiceTests()
        {
            _projectRepoMock = new Mock<IProjectRepository>();
            _milestoneRepoMock = new Mock<IMilestoneRepository>();
            _userRepoMock = new Mock<IUserRepository>();

            _service = new ProjectService(
                _projectRepoMock.Object,
                _milestoneRepoMock.Object,
                _userRepoMock.Object);
        }

        [Fact]
        public async Task CreateAsync_GivenInvalidDates_ThrowsException()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                Name = "Test Project",
                StartDate = new DateTime(2025, 2, 1),
                EndDate = new DateTime(2025, 1, 1) // Invalid: Before StartDate
            };

            // Act
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Start date must be before end date.");
        }

        [Fact]
        public async Task CreateAsync_GivenNonManagerUser_ThrowsException()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                Name = "Test Project",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 2, 1),
                ManagerId = 1
            };

            var nonManagerRole = TestHelper.CreateEntity<Role>().SetPrivate("Name", "Employee");
            var nonManagerUser = TestHelper.CreateEntity<User>(1).SetPrivate("Role", nonManagerRole);

            _userRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(nonManagerUser);

            // Act
            Func<Task> act = async () => await _service.CreateAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Manager not found or user is not a Manager.");
        }

        [Fact]
        public async Task CreateAsync_GivenValidData_SuccessfullyCreatesProject()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                Name = "Test Project",
                Description = "A clean code project",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 2, 1),
                ProjectStatusId = 1,
                ManagerId = 1,
                TotalStoryPoints = 100
            };

            var managerRole = TestHelper.CreateEntity<Role>().SetPrivate("Name", "Manager");
            var managerUser = TestHelper.CreateEntity<User>(1).SetPrivate("Role", managerRole);

            _userRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(managerUser);

            _projectRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Project>()))
                .Returns(Task.CompletedTask)
                .Callback<Project>(p => p.SetPrivate("Id", 100));

            var savedProject = TestHelper.CreateEntity<Project>(100)
                .SetPrivate("Name", "Test Project")
                .SetPrivate("StartDate", new DateTime(2025, 1, 1))
                .SetPrivate("EndDate", new DateTime(2025, 2, 1));

            _projectRepoMock.Setup(repo => repo.GetByIdAsync(100))
                .ReturnsAsync(savedProject);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(100);
            result.Name.Should().Be("Test Project");
        }
    }
}
