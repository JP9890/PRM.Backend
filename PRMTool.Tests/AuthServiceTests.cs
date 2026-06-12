using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PRMTool.Application.Services;
using PRMTool.Domain.Entities;
using PRMTool.Domain.Interfaces;
using Xunit;
using Serilog;

namespace PRMTool.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger>();
            
            // Serilog ForContext mock setup
            _loggerMock.Setup(l => l.ForContext<AuthService>()).Returns(_loggerMock.Object);

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _configurationMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_GivenNonExistentUser_ReturnsNull()
        {
            // Arrange
            string invalidUsername = "unknownUser";
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(invalidUsername))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.AuthenticateAsync(invalidUsername, "anyPassword");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_GivenInactiveUser_ReturnsNull()
        {
            // Arrange
            var inactiveUser = CreateUser(isActive: false);
            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(inactiveUser.Username))
                .ReturnsAsync(inactiveUser);

            // Act
            var result = await _authService.AuthenticateAsync(inactiveUser.Username, "anyPassword");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_GivenInvalidPassword_ReturnsNull()
        {
            // Arrange
            var validUser = CreateUser(isActive: true)
                .SetPrivate("PasswordHash", BCrypt.Net.BCrypt.HashPassword("CorrectPassword"));

            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(validUser.Username))
                .ReturnsAsync(validUser);

            // Act
            var result = await _authService.AuthenticateAsync(validUser.Username, "WrongPassword");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_GivenValidCredentials_ReturnsAuthResponseWithToken()
        {
            // Arrange
            string plainTextPassword = "ValidPassword123!";
            var validUser = CreateUser(isActive: true)
                .SetPrivate("PasswordHash", BCrypt.Net.BCrypt.HashPassword(plainTextPassword));

            SetupJwtConfiguration();

            _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(validUser.Username))
                .ReturnsAsync(validUser);

            // Act
            var result = await _authService.AuthenticateAsync(validUser.Username, plainTextPassword);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be(validUser.Username);
            result.Token.Should().NotBeNullOrEmpty();
        }

        // Helper methods to keep tests clean and readable
        
        private User CreateUser(bool isActive)
        {
            var role = TestHelper.CreateEntity<Role>().SetPrivate("Name", "Employee");

            return TestHelper.CreateEntity<User>(1)
                .SetPrivate("Username", "testUser")
                .SetPrivate("Email", "test@example.com")
                .SetPrivate("IsActive", isActive)
                .SetPrivate("Role", role);
        }

        private void SetupJwtConfiguration()
        {
            _configurationMock.Setup(config => config["Jwt:Key"])
                .Returns("super_secret_test_key_must_be_long_enough");
            
            _configurationMock.Setup(config => config["Jwt:Issuer"])
                .Returns("TestIssuer");
                
            _configurationMock.Setup(config => config["Jwt:Audience"])
                .Returns("TestAudience");
                
            _configurationMock.Setup(config => config["Jwt:DurationInMinutes"])
                .Returns("60");
        }
    }
}
