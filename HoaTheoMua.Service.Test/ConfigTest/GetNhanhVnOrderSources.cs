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
    public class GetNhanhVnOrderSourcesTests
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
        public async Task GetNhanhVnOrderSources_ValidConfig_ReturnsOrderSourcesValue()
        {
            // Arrange
            var json = "{\"value\": 10, \"options\": [{\"label\": \"Web\", \"value\": 10}]}";
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("nhanhvn.order_sources", json)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetNhanhVnOrderSources();

            // Assert
            Assert.Equal(10, result);
        }

        // EDGE CASE 1
        [Fact]
        public async Task GetNhanhVnOrderSources_ConfigNotFound_ThrowsBadRequestException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.GetNhanhVnOrderSources());
            Assert.Equal("GetNhanhVnOrderSources Not Found", exception.Message);
        }

        // EDGE CASE 2
        [Fact]
        public async Task GetNhanhVnOrderSources_InvalidJson_ThrowsException()
        {
            // Arrange
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("nhanhvn.order_sources", "invalid-json")
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => service.GetNhanhVnOrderSources());
        }

        // EDGE CASE 3
        [Fact]
        public async Task GetNhanhVnOrderSources_MissingOptions_ThrowsBadRequestException()
        {
            // Arrange: valid "value" but no "options" array. After deserialization
            // Options is null, which the service rejects with BadRequestException.
            // (Property names are case-sensitive via [JsonPropertyName]; they must be lowercase.)
            var json = "{\"value\": 10}";
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("nhanhvn.order_sources", json)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.GetNhanhVnOrderSources());
            Assert.Equal("Cannot convert value", exception.Message);
        }

        // EDGE CASE 4
        [Fact]
        public async Task GetNhanhVnOrderSources_NullValue_ThrowsJsonException()
        {
            // Arrange: JSON null for the non-nullable "value" is rejected by the
            // deserializer itself (the service's `config.Value == null` guard is dead
            // code for a non-nullable int), so the failure surfaces as JsonException.
            var json = "{\"value\": null, \"options\": []}";
            var configs = new List<ConfigEntity>
            {
                CreateConfigEntity("nhanhvn.order_sources", json)
            };
            var dbContext = CreateMockDbContext(configs).Object;
            var service = CreateService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => service.GetNhanhVnOrderSources());
        }
    }
}