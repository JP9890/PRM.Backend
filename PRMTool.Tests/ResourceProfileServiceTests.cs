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
    public class ResourceProfileServiceTests
    {
        private readonly Mock<IResourceProfileRepository> _profileRepoMock;
        private readonly Mock<IResourceSkillRepository> _skillRepoMock;
        private readonly Mock<ISkillRepository> _skillLookupRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IAllocationRepository> _allocationRepoMock;
        private readonly ResourceProfileService _service;

        public ResourceProfileServiceTests()
        {
            _profileRepoMock = new Mock<IResourceProfileRepository>();
            _skillRepoMock = new Mock<IResourceSkillRepository>();
            _skillLookupRepoMock = new Mock<ISkillRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _allocationRepoMock = new Mock<IAllocationRepository>();

            _service = new ResourceProfileService(
                _profileRepoMock.Object,
                _skillRepoMock.Object,
                _skillLookupRepoMock.Object,
                _userRepoMock.Object,
                _allocationRepoMock.Object);
        }

        [Fact]
        public async Task AssignManagerAsync_GivenInvalidManagerRole_ThrowsException()
        {
            // Arrange
            var dto = new AssignManagerDto { EmployeeUserId = 10, ManagerUserId = 1 };

            var profile = TestHelper.CreateEntity<ResourceProfile>(10);
            _profileRepoMock.Setup(repo => repo.GetByUserIdAsync(10))
                .ReturnsAsync(profile);

            // Mock non-manager user
            var employeeRole = TestHelper.CreateEntity<Role>().SetPrivate("Name", "Employee");
            var invalidManager = TestHelper.CreateEntity<User>(1).SetPrivate("Role", employeeRole);
            
            _userRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(invalidManager);

            // Act
            Func<Task> act = async () => await _service.AssignManagerAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Manager user not found or user is not a Manager.");
        }

        [Fact]
        public async Task DeactivateAsync_GivenActiveAllocations_EndsAllocationsAndDeactivatesUser()
        {
            // Arrange
            var profileId = 10;
            var userId = 20;

            var profile = TestHelper.CreateEntity<ResourceProfile>(profileId)
                .SetPrivate("UserId", userId)
                .SetPrivate("IsActive", true);

            var user = TestHelper.CreateEntity<User>(userId)
                .SetPrivate("IsActive", true);

            var activeAllocation = TestHelper.CreateEntity<Allocation>(100)
                .SetPrivate("ToDate", DateTime.UtcNow.AddDays(10)); // End date in future
                
            var allocationsList = new List<Allocation> { activeAllocation };
            profile.SetPrivate("Allocations", allocationsList);

            _profileRepoMock.Setup(repo => repo.GetByIdAsync(profileId))
                .ReturnsAsync(profile);

            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Setup maps for return DTO
            _allocationRepoMock.Setup(repo => repo.GetActiveByResourceIdAsync(profileId))
                .ReturnsAsync(new List<Allocation>());
            _skillRepoMock.Setup(repo => repo.GetByResourceProfileIdAsync(profileId))
                .ReturnsAsync(new List<ResourceSkill>());

            // Act
            var result = await _service.DeactivateAsync(profileId);

            // Assert
            result.Should().NotBeNull();
            result!.IsActive.Should().BeFalse();

            _allocationRepoMock.Verify(repo => repo.UpdateAsync(It.Is<Allocation>(a => a.ToDate.Date == DateTime.UtcNow.Date)), Times.Once);
            _profileRepoMock.Verify(repo => repo.UpdateAsync(It.Is<ResourceProfile>(p => !p.IsActive)), Times.Once);
            _userRepoMock.Verify(repo => repo.UpdateAsync(It.Is<User>(u => !u.IsActive)), Times.Once);
        }
    }
}
