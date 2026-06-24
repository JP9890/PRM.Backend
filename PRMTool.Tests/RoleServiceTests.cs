using System.Collections.Generic;
using System.Linq;
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
    public class RoleServiceTests
    {
        private readonly Mock<IRoleRepository> _roleRepoMock;
        private readonly RoleService _service;

        public RoleServiceTests()
        {
            _roleRepoMock = new Mock<IRoleRepository>();
            _service = new RoleService(_roleRepoMock.Object);
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsMappedRoleDtos()
        {
            // Arrange
            var roles = new List<Role>
            {
                TestHelper.CreateEntity<Role>(1).SetPrivate("Name", "Admin").SetPrivate("Description", "Administrator"),
                TestHelper.CreateEntity<Role>(2).SetPrivate("Name", "Manager").SetPrivate("Description", "Project Manager")
            };

            _roleRepoMock.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(roles);

            // Act
            var result = (await _service.GetAllRolesAsync()).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Admin");
            result.Last().Name.Should().Be("Manager");
        }
    }
}
