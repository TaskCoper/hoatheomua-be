using HoaTheoMua.Repository;
using HoaTheoMua.Service.Card;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;
using CardService = HoaTheoMua.Service.Card.Service;

namespace HoaTheoMua.Service.Test.CardTest;

public class CreateCardAiRequestTest
{
    [Fact]
    public async Task CreateCardAi_NullSourceCardId_UsesCreateMode()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var user = new Mock<ICurrentUserService>();
        user.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());
        var service = new CardService(db, user.Object, Mock.Of<Ai.IService>());

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => service.CreateCardAi(new()
        {
            OrderDetailId = Guid.NewGuid(),
            VirtualId = Guid.NewGuid(),
            SourceCardId = null,
            CardTemplateId = Guid.NewGuid(),
            MessageContent = "Giáng sinh 123"
        }));

        Assert.Equal("NOT_FOUND", exception.MessageCode);
    }
}
