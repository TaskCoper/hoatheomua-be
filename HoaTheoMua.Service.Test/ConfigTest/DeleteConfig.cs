using System;
using System.Collections.Generic;
using System.Linq;
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
    public class DeleteConfigTests
    {
        private static ConfigEntity CreateConfigEntity(
            Guid id, string key, string group, string kind, bool isDeleted = false)
        {
            return new ConfigEntity
            {
                Id = id,
                Key = key,
                Group = group,
                Kind = kind,
                Value = "{}",
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
        public async Task DeleteConfig_ExistingConfig_DeletesConfigSuccessfully()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "web.title", "web", nameof(KindConfig.Content))
            };
            var dbContextMock = CreateMockDbContext(configs);
            var service = CreateService(dbContextMock.Object);

            // Act
            var result = await service.DeleteConfig(configId);

            // Assert
            Assert.Equal("Xóa thành công.", result);
            Assert.True(configs[0].IsDeleted); // soft-delete flag must actually be set
            dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        // EDGE CASE 1
        [Fact]
        public async Task DeleteConfig_ConfigNotFound_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.DeleteConfig(Guid.NewGuid()));
            Assert.Equal("Id Not Found", exception.Message);
        }

        // EDGE CASE 2
        [Fact]
        public async Task DeleteConfig_ConfigAlreadyDeleted_ThrowsBadRequestException()
        {
            // Arrange
            var configId = Guid.NewGuid();
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity(configId, "web.title", "web", nameof(KindConfig.Content), isDeleted: true)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.DeleteConfig(configId));
            Assert.Equal("Id Not Found", exception.Message);
        }

        // EDGE CASE 3
        [Fact]
        public async Task DeleteConfig_EmptyGuid_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.DeleteConfig(Guid.Empty));
            Assert.Equal("Id Not Found", exception.Message);
        }
    }
}