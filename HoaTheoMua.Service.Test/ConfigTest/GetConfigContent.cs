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
    public class GetConfigContentTests
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
        public async Task GetConfigContent_ValidFilters_ReturnsFilteredPublicContentConfigs()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}", isPublic: true),
                CreateConfigEntity("web.desc", "web", nameof(KindConfig.Content), "{\"value\": \"My Desc\"}", isPublic: false),
                CreateConfigEntity("api.key", "api", nameof(KindConfig.Setting), "{\"value\": \"secret\"}", isPublic: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigContent(isPublic: true, groupName: "web", key: "title");

            // Assert
            Assert.Single(result);
            Assert.Equal("web.title", result[0].Key);
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetConfigContent_GroupFilterNoMatch_ReturnsEmptyList()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}", isPublic: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigContent(isPublic: null, groupName: "nonexistent", key: null);

            // Assert
            Assert.Empty(result);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetConfigContent_KeyFilterNoMatch_ReturnsEmptyList()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}", isPublic: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigContent(isPublic: null, groupName: null, key: "nonexistent");

            // Assert
            Assert.Empty(result);
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetConfigContent_FilterIsNotPublic_ReturnsPrivateContentConfigsOnly()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}", isPublic: true),
                CreateConfigEntity("web.desc", "web", nameof(KindConfig.Content), "{\"value\": \"My Desc\"}", isPublic: false)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigContent(isPublic: false, groupName: null, key: null);

            // Assert
            Assert.Single(result);
            Assert.Equal("web.desc", result[0].Key);
        }
    }
}