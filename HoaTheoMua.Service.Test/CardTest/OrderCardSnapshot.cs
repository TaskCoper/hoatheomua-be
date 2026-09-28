using System.Reflection;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Order = HoaTheoMua.Service.Order;

namespace HoaTheoMua.Service.Test.CardTest;

public class OrderCardSnapshot
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OrderFreezesChosenImageAndCardPromptConfigWithoutChangingProductLineage(bool generated)
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var product = Guid.NewGuid(); var template = Guid.NewGuid(); var line = Guid.NewGuid();
        db.Products.Add(new() { Id = product, ProductName = "Bouquet", CoverImage = "https://example.com/product.png" });
        db.CardTemplates.Add(new() { Id = template, Name = "Template", ImageUrl = "https://example.com/template.png", IsActive = true, TemplateType = CardTemplateType.Ai });
        db.Configs.Add(new() { Id = Guid.NewGuid(), Key = "imagegen.card_prompt", Group = "imagegen", Kind = "Setting",
            Value = JsonSerializer.Serialize(new { value = " card config exact\n" }) });
        db.Configs.Add(new() { Id = Guid.NewGuid(), Key = "card_config.sizes", Group = "card_config", Kind = "Setting",
            Value = JsonSerializer.Serialize(new { value = "{\"items\":[{\"name\":\"S\",\"width\":10,\"height\":15,\"base_price\":10000,\"max_words\":70}]}" }) });
        await db.SaveChangesAsync();
        using var http = new HttpClient();
        var service = new Order.Service(db, Mock.Of<ICurrentUserService>(), new HttpContextAccessor(), http,
            Mock.Of<HoaTheoMua.Service.Product.IService>(), new ConfigurationBuilder().Build(), Mock.Of<HoaTheoMua.Service.BaoKim.IService>());
        var prepared = new Order.Response.PreparedOrder { Bouquets = [new()
        {
            ProductId = product, OrderLineId = line, Number = 1, AiCustomize = generated,
            GeneratedFlowerId = generated ? Guid.NewGuid() : null,
            GeneratedFlowerImageUrl = generated ? "https://example.com/chosen-flower.png" : null,
            Cards = [new() { CardType = GeneratedCardType.Ai, CardTemplateId = template, SizeName = "S",
                FormType = CardFormType.GoMay, MessageContent = "Chúc mừng", AttachedImageUrl = "https://example.com/person.png" }]
        }] };
        var method = typeof(Order.Service).GetMethod("BuildOrderDetailsMetadata", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var output = await (Task<Dictionary<Guid, Order.Request.OrderDetailMetadata>>)method.Invoke(service, [prepared, Guid.NewGuid()])!;
        var card = Assert.Single(output[line].ProductMetadata).SnapShotCard!;
        Assert.Equal(product, card.Source.Id);
        Assert.Equal(generated ? "generated_flower" : "product", card.Source.ImageSourceType);
        Assert.Equal(generated ? "https://example.com/chosen-flower.png" : "https://example.com/product.png", card.Source.ImageUrl);
        Assert.Equal(" card config exact\n", card.SystemPrompt!.Content);
        Assert.Equal("https://example.com/person.png", card.AttachedImageUrl);
        Assert.Equal(10, card.CardSize!.Width); Assert.Equal(15, card.CardSize.Height);
    }
}
