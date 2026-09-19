using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Flower;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;
using FlowerService = HoaTheoMua.Service.Flower.Service;
using ProductService = HoaTheoMua.Service.Product.IService;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class GetGeneratedFlowersTest
{
    [Fact]
    public async Task GetAdminGeneratedFlowers_CamelCaseProductFilter_ReturnsMatchingFlower()
    {
        await using var db = CreateDb();
        var matchingProductId = Guid.NewGuid();
        var matchingFlower = Flower("matching", matchingProductId, 2);
        db.GeneratedFlowers.AddRange(matchingFlower, Flower("other", Guid.NewGuid(), 1));
        db.ClientHistories.Add(History(matchingFlower));
        await db.SaveChangesAsync();

        var result = await CreateService(db).GetAdminGeneratedFlowers(new()
        {
            productId = matchingProductId,
            pageIndex = 1,
            pageSize = 5
        });

        var item = Assert.IsType<Response.AdminGeneratedFlowerResponse>(Assert.Single(result.Value.Items));
        Assert.Equal(matchingFlower.Id, item.Id);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task GetCustomerGeneratedFlowers_CamelCaseSessionFilter_ReturnsOnlyRequestedSession()
    {
        await using var db = CreateDb();
        var userId = Guid.NewGuid();
        var requestedSessionId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var matchingFlower = Flower("matching", productId, 2, userId);
        var otherFlower = Flower("other", productId, 1, userId);
        db.GeneratedFlowers.AddRange(matchingFlower, otherFlower);
        db.ClientHistories.AddRange(
            History(matchingFlower, requestedSessionId, productId),
            History(otherFlower, Guid.NewGuid(), productId));
        await db.SaveChangesAsync();

        var result = await CreateService(db, userId).GetCustomerGeneratedFlowers(new()
        {
            sessionId = requestedSessionId,
            pageIndex = 1,
            pageSize = 5
        });

        var session = Assert.IsType<Response.CustomerFlowerSessionResponse>(Assert.Single(result.Value.Items));
        Assert.Equal(requestedSessionId, session.SessionId);
        Assert.Equal(matchingFlower.Id, Assert.Single(session.Flowers).Id);
    }

    [Fact]
    public async Task GetAdminGeneratedFlowers_InvalidCamelCasePageIndex_ThrowsValidationError()
    {
        await using var db = CreateDb();

        await Assert.ThrowsAnyAsync<AppException>(() => CreateService(db).GetAdminGeneratedFlowers(new()
        {
            pageIndex = 0,
            pageSize = 5
        }));
    }

    private static AppDbContext CreateDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static FlowerService CreateService(AppDbContext db, Guid? userId = null)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.GetRequiredUserId()).Returns(userId ?? Guid.NewGuid());
        return new FlowerService(db, currentUser.Object, Mock.Of<Ai.IService>(), Mock.Of<ProductService>());
    }

    private static GeneratedFlower Flower(string name, Guid productId, int sequence, Guid? userId = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        ImageUrl = "https://example.test/flower.jpg",
        Metadata = JsonSerializer.Serialize(new { subId = productId }),
        UserId = userId ?? Guid.NewGuid(),
        CreatedAt = new DateTimeOffset(2026, 9, sequence, 0, 0, 0, TimeSpan.Zero)
    };

    private static ClientHistory History(GeneratedFlower flower, Guid? sessionId = null, Guid? productId = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = flower.UserId,
        Type = ClientHistoryType.Flower,
        SessionId = sessionId ?? Guid.NewGuid(),
        BaseId = productId ?? Guid.NewGuid(),
        RootId = productId ?? Guid.NewGuid(),
        OutputId = flower.Id,
        CreatedAt = flower.CreatedAt,
        Metadata = "{\"operation\":\"create\"}"
    };
}
