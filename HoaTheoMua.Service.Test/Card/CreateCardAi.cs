using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Test.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Xunit;

using CardRequest = HoaTheoMua.Service.Card.Request;
using CardService = HoaTheoMua.Service.Card.Service;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;

namespace HoaTheoMua.Service.Test.Card;

public class CreateCardAi
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static CardService CreateService(List<OrderDetails>? details = null, List<GeneratedCard>? cards = null)
    {
        details ??= [];
        cards ??= [];
        var detailSet = BuildSet(details);
        var cardSet = BuildSet(cards);
        var historySet = BuildSet(new List<ClientHistory>());
        var context = new Mock<AppDbContext>(new DbContextOptionsBuilder<AppDbContext>().Options);
        context.Setup(x => x.OrderDetails).Returns(detailSet.Object);
        context.Setup(x => x.GeneratedCards).Returns(cardSet.Object);
        context.Setup(x => x.ClientHistories).Returns(historySet.Object);
        context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var transaction = new Mock<IDbContextTransaction>();
        transaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var database = new Mock<DatabaseFacade>(context.Object);
        database.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        context.Setup(x => x.Database).Returns(database.Object);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.GetRequiredUserId()).Returns(UserId);
        return new CardService(context.Object, currentUser.Object);
    }

    private static Mock<DbSet<T>> BuildSet<T>(List<T> source) where T : class
    {
        var queryable = source.AsQueryable();
        var set = new Mock<DbSet<T>>();
        set.As<IAsyncEnumerable<T>>().Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(source.GetEnumerator()));
        set.As<IQueryable<T>>().Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        set.As<IQueryable<T>>().Setup(x => x.Expression).Returns(queryable.Expression);
        set.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(queryable.ElementType);
        set.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(() => source.GetEnumerator());
        set.Setup(x => x.Add(It.IsAny<T>())).Callback<T>(source.Add).Returns((EntityEntry<T>)null!);
        return set;
    }

    private static OrderDetails Detail(string? snapshot) => new()
    {
        Id = Guid.NewGuid(), SnapshotCard = snapshot, Name = "Flower", OrderId = Guid.NewGuid(),
        Order = new OrderEntity { Id = Guid.NewGuid(), UserId = UserId, Ward = 1, City = 1, Address = "Address", ShortAddress = "Address", FullName = "Name", Phone = "0900000000", Remarks = "", Status = "new", TrackNumber = "track", OrderTime = 0, PhoneReceiver = "0900000000", FullNameReceiver = "Name", PaymentGateway = Repository.Enum.PaymentGateway.BaoKim, ShippingGateway = Repository.Enum.ShippingGateway.Ahamove }
    };

    private static CardRequest.CreateCardAiRequest Request(Guid id) => new() { OrderDetailId = id };

    private static string Snapshot(string? regeneratedFromCardId = null, string message = "Chúc bạn sinh nhật vui vẻ") => $$"""
        [{
          "source": { "type": "product", "id": "11111111-1111-1111-1111-111111111111", "imageUrl": "https://example.com/flower.png" },
          "regeneratedFromCardId": {{(regeneratedFromCardId is null ? "null" : $"\"{regeneratedFromCardId}\"")}},
          "cardTemplate": { "templateId": "22222222-2222-2222-2222-222222222222", "templateName": "AI Template", "templateType": "ai", "templateImageUrl": "https://example.com/template.png" },
          "cardSize": { "name": "M", "width": 15, "height": 25, "base_price": 15000, "max_words": 150 },
          "cardConfig": { "min_words": 0, "max_words": 35, "extra_price": 0 },
          "messageContent": "{{message}}", "formType": "calligraphy", "wordCount": 5,
          "basePrice": 15000, "extraPrice": 0, "totalPrice": 15000,
          "systemPrompt": { "type": "card", "content": "Prompt content" }, "validatedAt": "2026-09-11T08:00:00Z"
        }]
        """;

    private static async Task InvalidSnapshot(string? snapshot)
    {
        var detail = Detail(snapshot);
        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService([detail]).CreateCardAi(Request(detail.Id)));
        Assert.Equal("ORDER_DETAIL_CARD_SNAPSHOT_INVALID", error.MessageCode);
    }

    [Fact]
    public async Task CreateCardAi_BothIdsNull_ThrowsBadRequest() => await Assert.ThrowsAsync<BadRequestException>(() => CreateService().CreateCardAi(new()));

    [Fact]
    public async Task CreateCardAi_BothIdsProvided_ThrowsBadRequest() => await Assert.ThrowsAsync<BadRequestException>(() => CreateService().CreateCardAi(new() { OrderDetailId = Guid.NewGuid(), SourceCardId = Guid.NewGuid() }));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public Task ParseSnapshot_NullOrEmpty_ThrowsConflict(string? snapshot) => InvalidSnapshot(snapshot);

    [Fact]
    public Task ParseSnapshot_InvalidJson_ThrowsConflict() => InvalidSnapshot("not json");

    [Fact]
    public Task ParseSnapshot_NotArray_ThrowsConflict() => InvalidSnapshot("{}");

    [Fact]
    public Task ParseSnapshot_EmptyArray_ThrowsConflict() => InvalidSnapshot("[]");

    [Fact]
    public Task ParseSnapshot_NoAiSnapshot_ThrowsConflict() => InvalidSnapshot(Snapshot().Replace("\"templateType\": \"ai\"", "\"templateType\": \"handmade\""));

    [Fact]
    public async Task ParseSnapshot_PriorityOriginal_OverRegenerated()
    {
        var regenerated = Snapshot(Guid.NewGuid().ToString(), "regenerated");
        var original = Snapshot(message: "original");
        var detail = Detail($"[{regenerated[1..^1]},{original[1..^1]}]");
        var cards = new List<GeneratedCard>();

        var result = await CreateService([detail], cards).CreateCardAi(Request(detail.Id));

        Assert.Equal("original", result.MessageContent);
        Assert.Equal("original", Assert.Single(cards).MessageContent);
    }

    [Fact]
    public async Task ParseSnapshot_FallbackToFirstAi()
    {
        var first = Snapshot(Guid.NewGuid().ToString(), "first");
        var second = Snapshot(Guid.NewGuid().ToString(), "second");
        var detail = Detail($"[{first[1..^1]},{second[1..^1]}]");

        var result = await CreateService([detail], []).CreateCardAi(Request(detail.Id));

        Assert.Equal("first", result.MessageContent);
    }

    [Fact]
    public Task ParseSnapshot_MissingMessageContent_ThrowsConflict() => InvalidSnapshot(Snapshot().Replace("\"messageContent\": \"Chúc bạn sinh nhật vui vẻ\", ", ""));

    [Fact]
    public Task ParseSnapshot_InvalidSourceType_ThrowsConflict() => InvalidSnapshot(Snapshot().Replace("\"type\": \"product\"", "\"type\": \"invalid\""));

    [Fact]
    public Task ParseSnapshot_InvalidFormType_ThrowsConflict() => InvalidSnapshot(Snapshot().Replace("\"formType\": \"calligraphy\"", "\"formType\": \"invalid\""));

    [Fact]
    public async Task CreateCardAi_OrderDetailNotFound_ThrowsNotFound() => await Assert.ThrowsAsync<NotFoundException>(() => CreateService().CreateCardAi(Request(Guid.NewGuid())));

    [Fact]
    public async Task CreateCardAi_CreateMode_WithMessageContent_ThrowsBadRequest()
    {
        var detail = Detail(Snapshot());
        await Assert.ThrowsAsync<BadRequestException>(() => CreateService([detail]).CreateCardAi(new() { OrderDetailId = detail.Id, MessageContent = "abc" }));
    }
}
