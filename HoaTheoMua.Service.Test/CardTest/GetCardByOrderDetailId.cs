using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;
using CardService = HoaTheoMua.Service.Card.Service;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;

namespace HoaTheoMua.Service.Test.CardTest;

public class GetCardByOrderDetailId
{
    [Fact]
    public async Task ReturnsCardPreviewWithFormMessageAndTemplate()
    {
        await using var db = CreateDbContext();
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderDetailId = Guid.NewGuid();
        var virtualId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var templateId = Guid.NewGuid();

        await db.Orders.AddAsync(Order(orderId, userId, orderDetailId, virtualId, cardId));
        await db.GeneratedCards.AddAsync(new GeneratedCard
        {
            Id = cardId,
            UserId = userId,
            ImageUrl = "https://example.test/card.png",
            Content = "Chúc mừng",
            CardType = GeneratedCardType.Ai,
            FormType = CardFormType.Calligraphy,
            MessageContent = "Chúc mừng sinh nhật",
            SizeName = "M",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.ClientHistories.AddAsync(new ClientHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ClientHistoryType.Card,
            OutputId = cardId,
            BaseId = Guid.NewGuid(),
            RootId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = JsonSerializer.Serialize(new
            {
                cardTemplate = new { id = templateId, name = "Birthday" }
            })
        });
        await db.SaveChangesAsync();

        var result = await CreateService(db, userId).GetCardByOrderDetailId(orderDetailId, virtualId);

        var card = Assert.Single(result.Cards);
        Assert.Equal("calligraphy", card.FormType);
        Assert.Equal("Chúc mừng sinh nhật", card.MessageContent);
        Assert.Equal(templateId, card.TemplateId);
    }

    private static AppDbContext CreateDbContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options);

    private static CardService CreateService(AppDbContext db, Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.GetRequiredUserId()).Returns(userId);
        return new CardService(db, currentUser.Object, new HttpContextAccessor(), new Mock<Ai.IService>().Object);
    }

    private static OrderEntity Order(Guid orderId, Guid userId, Guid orderDetailId, Guid virtualId, Guid cardId) => new()
    {
        Id = orderId,
        UserId = userId,
        Ward = 1,
        City = 1,
        Address = "Address",
        ShortAddress = "Address",
        FullName = "Customer",
        Phone = "0900000000",
        Remarks = string.Empty,
        Status = "Paid",
        TrackNumber = string.Empty,
        OrderTime = 0,
        PhoneReceiver = "0900000000",
        FullNameReceiver = "Customer",
        PaymentGateway = PaymentGateway.BaoKim,
        ShippingGateway = ShippingGateway.XanhSm,
        OrderDetails =
        [
            new OrderDetails
            {
                Id = orderDetailId,
                OrderId = orderId,
                Name = "Flower",
                ProductId = Guid.NewGuid(),
                Metadata = JsonSerializer.Serialize(new
                {
                    productMetadata = new[]
                    {
                        new
                        {
                            snapShotCard = new
                            {
                                virtualId,
                                source = new { type = "product", id = Guid.NewGuid() },
                                cardType = "ai",
                                idResult = new[] { cardId }
                            }
                        }
                    }
                })
            }
        ]
    };
}
