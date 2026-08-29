using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Config;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using ConfigServiceImpl = HoaTheoMua.Service.Config.Service;

namespace HoaTheoMua.Service.Test.ConfigTest
{
    public class GetConfigByIdTests
    {
        private static readonly Guid TestUserId = Guid.Parse("cba7c21c-d6f2-4b30-9b4e-8c7c10b4f0b2");

        private static ConfigEntity CreateConfigEntity(
            Guid id, string key, string group, string kind, string value = "{}", bool isPublic = true, bool isDeleted = false)
        {
            return new ConfigEntity
            {
                Id = id,
                Key = key,
                Group = group,
                Kind = kind,
                Value = value,
                IsPublic = isPublic,
                IsDeleted = isDeleted,
                UserId = TestUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static Mock<AppDbContext> CreateMockDbContext(List<ConfigEntity> configs)
        {
            var mockConfigs = configs.BuildMockDbSet();
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);
            mockDbContext.Setup(x => x.Configs).Returns(mockConfigs.Object);
            return mockDbContext;
        }

        private static ConfigServiceImpl CreateService(AppDbContext dbContext, ICurrentUserService currentUser)
        {
            return new ConfigServiceImpl(dbContext, currentUser);
        }

        private static ICurrentUserService CreateMockCurrentUserService(Guid userId)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(x => x.GetRequiredUserId()).Returns(userId);
            mock.Setup(x => x.IsVerified).Returns(true);
            return mock.Object;
        }

        // HAPPY CASE
        [Fact]
        public async Task GetConfigById_ValidId_ReturnsConfigSuccessfully()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetConfigById(configId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("web.title", result.Key);
            Assert.Equal("My Title", result.Value.GetProperty("value").GetString());
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetConfigById_ConfigNotFound_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.GetConfigById(Guid.NewGuid()));
            Assert.Equal("Id Not Found", exception.Message);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetConfigById_UnauthenticatedUser_ThrowsUnauthorizedException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.GetRequiredUserId()).Throws(new UnauthorizedException("User not found"));
            var service = CreateService(dbContext, currentUserMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetConfigById(Guid.NewGuid()));
            Assert.Equal("User not found", exception.Message);
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetConfigById_IsSecretKind_ReturnsMaskedValue()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"my-super-secret-token-long-enough-to-mask\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Act
            var result = await service.GetConfigById(configId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("api.secret", result.Key);
            Assert.Contains("••••", result.Value.GetProperty("value").GetString());
        }
    }
}