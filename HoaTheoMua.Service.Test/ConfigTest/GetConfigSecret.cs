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
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using ConfigServiceImpl = HoaTheoMua.Service.Config.Service;

namespace HoaTheoMua.Service.Test.ConfigTest
{
    public class GetConfigSecretTests
    {
        private static ConfigEntity CreateConfigEntity(
            string key, string group, string kind, string value = "{}", bool isPublic = true, bool isDeleted = false)
        {
            return new ConfigEntity
            {
                Id = Guid.NewGuid(),
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
        public async Task GetConfigSecret_ValidFilters_ReturnsMaskedSecretConfigs()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"my-super-secret-token-long-enough-to-mask\"}"),
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigSecret("api", "secret");

            // Assert
            Assert.Single(result);
            Assert.Equal("api.secret", result[0].Key);
            var valueJson = result[0].Value.GetProperty("value").GetString();
            Assert.Contains("••••", valueJson); // Should be masked
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetConfigSecret_GroupFilterNoMatch_ReturnsEmptyList()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"secret\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigSecret("nonexistent", null);

            // Assert
            Assert.Empty(result);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetConfigSecret_KeyFilterNoMatch_ReturnsEmptyList()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"secret\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigSecret(null, "nonexistent");

            // Assert
            Assert.Empty(result);
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetConfigSecret_ExcludesDeletedSecretConfigs()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"secret\"}", isDeleted: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigSecret(null, null);

            // Assert
            Assert.Empty(result);
        }
    }
}