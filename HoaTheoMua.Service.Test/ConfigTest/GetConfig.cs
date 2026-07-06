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
    public class GetConfigTests
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
        public async Task GetConfig_ReturnsAllActiveConfigsWithAppropriateMasking()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}"),
                CreateConfigEntity("api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"supersecrettoken-long-enough-to-mask\"}")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfig();

            // Assert
            Assert.Equal(2, result.Count);
            var titleConfig = result.First(x => x.Key == "web.title");
            Assert.Equal("My Title", titleConfig.Value.GetProperty("value").GetString());

            var secretConfig = result.First(x => x.Key == "api.secret");
            Assert.Contains("••••", secretConfig.Value.GetProperty("value").GetString());
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetConfig_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfig();

            // Assert
            Assert.Empty(result);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetConfig_ExcludesDeletedConfigs()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}", isDeleted: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfig();

            // Assert
            Assert.Empty(result);
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetConfig_SecretWithEmptyValue_ReturnsEmptyJsonElement()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("api.secret", "api", nameof(KindConfig.Secret), "")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfig();

            // Assert
            Assert.Single(result);
            var val = result[0].Value;
            Assert.Equal(JsonValueKind.Object, val.ValueKind);
        }
    }
}