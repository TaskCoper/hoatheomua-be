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
using UpdateConfigRequest = HoaTheoMua.Service.Config.Request.UpdateConfigRequest;

namespace HoaTheoMua.Service.Test.ConfigTest
{
    public class UpdateConfigTests
    {
        private static ConfigEntity CreateConfigEntity(
            Guid id, string key, string group, string kind, string value = "{}", bool isDeleted = false)
        {
            return new ConfigEntity
            {
                Id = id,
                Key = key,
                Group = group,
                Kind = kind,
                Value = value,
                IsPublic = true,
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
        public async Task UpdateConfig_ValidRequest_UpdatesConfigSuccessfully()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "web.title", "web", nameof(KindConfig.Content))
            };
            var dbContextMock = CreateMockDbContext(configs);
            var service = CreateService(dbContextMock.Object);

            var valueDoc = JsonDocument.Parse("{\"group\": \"web\", \"title\": \"New Store Title\"}");
            var request = new UpdateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement,
                IsPublic = false // seed is true, so a successful update must flip this
            };

            // Act
            var result = await service.UpdateConfig(configId, request);

            // Assert
            Assert.Equal("Cập nhật thành công", result);
            // The request fields must actually be written onto the persisted entity.
            Assert.False(configs[0].IsPublic);
            Assert.Contains("New Store Title", configs[0].Value);
            dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        // Sitemap lastmod: sửa config thì UpdatedAt phải được gán
        [Fact]
        public async Task UpdateConfig_ValidRequest_SetsUpdatedAt()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "web.title", "web", nameof(KindConfig.Content))
            };
            var service = CreateService(CreateMockDbContext(configs).Object);
            var request = new UpdateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = JsonDocument.Parse("{\"group\": \"web\"}").RootElement,
                IsPublic = true
            };
            Assert.Null(configs[0].UpdatedAt);
            var before = DateTimeOffset.UtcNow;

            // Act
            await service.UpdateConfig(configId, request);

            // Assert
            Assert.NotNull(configs[0].UpdatedAt);
            Assert.True(configs[0].UpdatedAt >= before);
        }

        // EDGE CASE 1
        [Fact]
        public async Task UpdateConfig_GroupMismatchWithValueGroup_ThrowsConflictException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            var valueDoc = JsonDocument.Parse("{\"group\": \"other\"}");
            var request = new UpdateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement,
                IsPublic = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => service.UpdateConfig(Guid.NewGuid(), request));
            Assert.Equal("group doesn't match", exception.Message);
        }

        // EDGE CASE 2
        [Fact]
        public async Task UpdateConfig_KeyAlreadyExistsOnAnotherConfig_ThrowsBadRequestException()
        {
            // Arrange
            var configId1 = Guid.NewGuid();
            var configId2 = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId1, "web.title", "web", nameof(KindConfig.Content)),
                CreateConfigEntity(configId2, "web.desc", "web", nameof(KindConfig.Content))
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            var valueDoc = JsonDocument.Parse("{\"group\": \"web\"}");
            var request = new UpdateConfigRequest
            {
                Key = "web.desc", // Clashes with configId2
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement,
                IsPublic = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateConfig(configId1, request));
            Assert.Equal("Key is exists", exception.Message);
        }

        // EDGE CASE 3
        [Fact]
        public async Task UpdateConfig_ConfigNotFound_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            var valueDoc = JsonDocument.Parse("{\"group\": \"web\"}");
            var request = new UpdateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement,
                IsPublic = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateConfig(Guid.NewGuid(), request));
            Assert.Equal("Id/Key Not Found", exception.Message);
        }

        // EDGE CASE 4
        [Fact]
        public async Task UpdateConfig_ChangeConfigKind_ThrowsConflictException()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "web.title", "web", nameof(KindConfig.Content))
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            var valueDoc = JsonDocument.Parse("{\"group\": \"web\"}");
            var request = new UpdateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Setting), // Change from Content to Setting
                Value = valueDoc.RootElement,
                IsPublic = true
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => service.UpdateConfig(configId, request));
            Assert.Equal("ConfigKindImmutable", exception.Message);
        }
    }
}