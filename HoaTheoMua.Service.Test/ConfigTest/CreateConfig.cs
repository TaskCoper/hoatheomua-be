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
using CreateConfigRequest = HoaTheoMua.Service.Config.Request.CreateConfigRequest;

namespace HoaTheoMua.Service.Test.ConfigTest
{
    public class CreateConfigTests
    {
        private static readonly Guid TestUserId = Guid.Parse("cba7c21c-d6f2-4b30-9b4e-8c7c10b4f0b2");

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
        public async Task CreateConfig_ValidRequest_CreatesConfigSuccessfully()
        {
            // Arrange
            var dbContextMock = CreateMockDbContext(new List<ConfigEntity>());
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContextMock.Object, currentUser);

            var valueDoc = JsonDocument.Parse("{\"group\": \"web\", \"title\": \"My Store\"}");
            var request = new CreateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement
            };

            // Act
            var result = await service.CreateConfig(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("web.title", result.Key);
            Assert.Equal("web", result.Group);
            Assert.Equal(TestUserId, result.UserId);
            dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        // EDGE CASE 1
        [Fact]
        public async Task CreateConfig_KeyAlreadyExists_ThrowsBadRequestException()
        {
            // Arrange
            var existingConfig = CreateConfigEntity("web.title", "web", nameof(KindConfig.Content));
            var dbContext = CreateMockDbContext(new List<ConfigEntity> { existingConfig }).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            var valueDoc = JsonDocument.Parse("{}");
            var request = new CreateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => service.CreateConfig(request));
            Assert.Equal("Key is exists", exception.Message);
        }

        // EDGE CASE 2
        [Fact]
        public async Task CreateConfig_GroupMismatchWithValueGroup_ThrowsConflictException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var currentUser = CreateMockCurrentUserService(TestUserId);
            var service = CreateService(dbContext, currentUser);

            // Value has group "other", but request.Group is "web" (mismatch)
            var valueDoc = JsonDocument.Parse("{\"group\": \"other\"}");
            var request = new CreateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => service.CreateConfig(request));
            Assert.Equal("group doesn't match", exception.Message);
        }

        // EDGE CASE 3
        [Fact]
        public async Task CreateConfig_UnauthenticatedUser_ThrowsUnauthorizedException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ConfigEntity>()).Object;
            var currentUserMock = new Mock<ICurrentUserService>();
            currentUserMock.Setup(x => x.GetRequiredUserId()).Throws(new UnauthorizedException("User not found"));
            var service = CreateService(dbContext, currentUserMock.Object);

            var valueDoc = JsonDocument.Parse("{}");
            var request = new CreateConfigRequest
            {
                Key = "web.title",
                Group = "web",
                Kind = nameof(KindConfig.Content),
                Value = valueDoc.RootElement
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => service.CreateConfig(request));
            Assert.Equal("User not found", exception.Message);
        }
    }
}