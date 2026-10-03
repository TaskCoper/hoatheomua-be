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
    public class GetConfigIsPublicTests
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
        public async Task GetConfigIsPublic_ValidFilters_ReturnsPublicContentConfigs()
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
            var result = await service.GetConfigIsPublic("web", "title");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("web.title", result[0].Key);
        }

        // Sitemap lastmod: response phải có CreatedAt, UpdatedAt
        [Fact]
        public async Task GetConfigIsPublic_ReturnsCreatedAtAndUpdatedAt()
        {
            // Arrange
            var created = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var updated = new DateTimeOffset(2026, 9, 28, 0, 0, 0, TimeSpan.Zero);
            var edited = CreateConfigEntity("web.edited", "web", nameof(KindConfig.Content));
            edited.CreatedAt = created;
            edited.UpdatedAt = updated;
            var neverEdited = CreateConfigEntity("web.never", "web", nameof(KindConfig.Content));
            neverEdited.CreatedAt = created;
            var service = CreateService(CreateMockDbContext(new List<ConfigEntity> { edited, neverEdited }).Object);

            // Act
            var result = await service.GetConfigIsPublic("web", null);

            // Assert
            Assert.NotNull(result);
            var editedResult = result.Single(x => x.Key == "web.edited");
            Assert.Equal(created, editedResult.CreatedAt);
            Assert.Equal(updated, editedResult.UpdatedAt);
            var neverResult = result.Single(x => x.Key == "web.never");
            Assert.Equal(created, neverResult.CreatedAt);
            Assert.Null(neverResult.UpdatedAt);
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetConfigIsPublic_NoMatch_ReturnsNull()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}", isPublic: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigIsPublic("nonexistent", null);

            // Assert
            Assert.Null(result);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetConfigIsPublic_ExcludesSecretAndSettingKinds()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                // CreateConfigEntity("setting.theme", "setting", nameof(KindConfig.), "{\"value\": \"dark\"}", isPublic: true),
                CreateConfigEntity("api.secret", "api", nameof(KindConfig.Secret), "{\"value\": \"secret\"}", isPublic: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigIsPublic(null, null);

            // Assert
            Assert.Null(result);
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetConfigIsPublic_ExcludesPrivateContent()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("web.title", "web", nameof(KindConfig.Content), "{\"value\": \"My Title\"}", isPublic: false)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetConfigIsPublic("web", null);

            // Assert
            Assert.Null(result);
        }
    }
}