using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
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
    public class GetNhanhVnOrderTagTests
    {
        private static ConfigEntity CreateConfigEntity(
            string key, string value, bool isDeleted = false)
        {
            return new ConfigEntity
            {
                Id = Guid.NewGuid(),
                Key = key,
                Group = "nhanhvn",
                Kind = "setting",
                Value = value,
                IsPublic = false,
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
        public async Task GetNhanhVnOrderTag_ValidConfig_ReturnsFilteredConfigItem()
        {
            // Arrange
            var json = "{\"value\": [1, 2], \"options\": [{\"label\": \"Tag 1\", \"value\": 1}, {\"label\": \"Tag 3\", \"value\": 3}]}";
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("nhanhvn.order_tags", json)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetNhanhVnOrderTag();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Options);
            Assert.Equal(1, result.Options[0].Value);
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetNhanhVnOrderTag_ConfigNotFound_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.GetNhanhVnOrderTag());
            Assert.Equal("NhanhVnOrderTag Not Found", exception.Message);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetNhanhVnOrderTag_InvalidJson_ThrowsException()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("nhanhvn.order_tags", "invalid-json")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => service.GetNhanhVnOrderTag());
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetNhanhVnOrderTag_NullPropertiesInConfig_ThrowsBadRequestException()
        {
            // Arrange
            var json = "{\"value\": null, \"options\": null}";
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("nhanhvn.order_tags", json)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.GetNhanhVnOrderTag());
            Assert.Equal("Cannot convert value", exception.Message);
        }
    }
}
