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
    public class GetConfigRevealTests
    {
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
                UserId = Guid.NewGuid(),
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

        private static ConfigServiceImpl CreateService(AppDbContext dbContext)
        {
            var mockCurrentUser = new Mock<ICurrentUserService>();
            return new ConfigServiceImpl(dbContext, mockCurrentUser.Object);
        }

        // HAPPY CASE
        [Fact]
        public async Task GetConfigReveal_ValidSecretId_ReturnsUnmaskedSecret()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"plain-unmasked-token\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigReveal(configId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("api.secret", result.Key);
            Assert.Equal("plain-unmasked-token", result.Value.GetProperty("value").GetString()); // Unmasked!
            Assert.True(result.ExpiresAt > result.RevealedAt);
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetConfigReveal_ConfigNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetConfigReveal(Guid.NewGuid()));
            Assert.Equal("Config not found or not a secret", exception.Message);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetConfigReveal_ConfigIsDeleted_ThrowsNotFoundException()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"plain-unmasked-token\"}", isDeleted: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetConfigReveal(configId));
            Assert.Equal("Config not found or not a secret", exception.Message);
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetConfigReveal_ConfigNotSecretKind_ThrowsNotFoundException()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.GetConfigReveal(configId));
            Assert.Equal("Config not found or not a secret", exception.Message);
        }
    }
}