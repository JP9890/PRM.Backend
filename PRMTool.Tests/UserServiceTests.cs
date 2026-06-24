using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PRMTool.Application.DTOs;
using PRMTool.Application.Services;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using Xunit;

namespace PRMTool.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IRoleRepository> _roleRepoMock;
        private readonly Mock<IResourceProfileRepository> _profileRepoMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _roleRepoMock = new Mock<IRoleRepository>();
            _profileRepoMock = new Mock<IResourceProfileRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _service = new UserService(
                _userRepoMock.Object,
                _roleRepoMock.Object,
                _profileRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateUserAsync_GivenExistingUsername_ThrowsException()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = "existingUser",
                Password = "ValidPassword123!"
            };

            _userRepoMock.Setup(repo => repo.GetByUsernameAsync("existingUser"))
                .ReturnsAsync(TestHelper.CreateEntity<User>(1));

            // Act
            Func<Task> act = async () => await _service.CreateUserAsync(dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Username already exists.");
        }

        [Fact]
        public async Task DeactivateUserAsync_GivenSelfUsername_ThrowsException()
        {
            // Arrange
            var user = TestHelper.CreateEntity<User>(1)
                .SetPrivate("Username", "myUsername");

            _userRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _service.DeactivateUserAsync(1, "myUsername");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("You cannot deactivate your own account.");
        }

        [Fact]
        public async Task DeactivateUserAsync_GivenLastActiveAdmin_ThrowsException()
        {
            // Arrange
            var adminRole = TestHelper.CreateEntity<Role>(1).SetPrivate("Name", "Admin");
            var adminUser = TestHelper.CreateEntity<User>(1)
                .SetPrivate("Username", "otherAdmin")
                .SetPrivate("Role", adminRole)
                .SetPrivate("RoleId", 1)
                .SetPrivate("IsActive", true);

            _userRepoMock.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(adminUser);

            // Mock that this is the ONLY active admin
            _userRepoMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<User> { adminUser });

            // Act
            Func<Task> act = async () => await _service.DeactivateUserAsync(1, "someOtherUser");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("You cannot deactivate the last active Admin.");
        }
    }
}
