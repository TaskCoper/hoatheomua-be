using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
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
using CardTemplateEntity = HoaTheoMua.Repository.Entity.CardTemplate;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;

namespace HoaTheoMua.Service.Test.Card;

public class CreateHandMadeCard
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static CardService CreateService(
        List<CardTemplateEntity>? templates = null, List<ProductEntity>? products = null,
        List<GeneratedFlower>? flowers = null, List<ClientHistory>? histories = null,
        List<ConfigEntity>? configs = null, List<GeneratedCard>? cards = null)
    {
        templates ??= [];
        products ??= [];
        flowers ??= [];
        histories ??= [];
        configs ??= [];
        cards ??= [];
        var context = new Mock<AppDbContext>(new DbContextOptionsBuilder<AppDbContext>().Options);
        context.Setup(x => x.CardTemplates).Returns(BuildSet(templates).Object);
        context.Setup(x => x.Products).Returns(BuildSet(products).Object);
        context.Setup(x => x.GeneratedFlowers).Returns(BuildSet(flowers).Object);
        context.Setup(x => x.ClientHistories).Returns(BuildSet(histories).Object);
        context.Setup(x => x.Configs).Returns(BuildSet(configs).Object);
        context.Setup(x => x.GeneratedCards).Returns(BuildSet(cards).Object);
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

    private static CardTemplateEntity Template(bool active = true, bool deleted = false,
        CardTemplateType type = CardTemplateType.Handmade) => new()
    {
        Id = Guid.NewGuid(), Name = "Handmade", ImageUrl = "https://example.com/template.png", IsActive = active,
        IsDeleted = deleted, TemplateType = type
    };

    private static ProductEntity Product() => new() { Id = Guid.NewGuid(), IsActive = true, AvailableForSale = 1 };

    private static ConfigEntity Config(string key, string value, bool isPublic = true, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(), Group = "card_config", Key = key, Kind = "Setting", IsPublic = isPublic,
        IsDeleted = deleted, UserId = UserId,
        Value = JsonSerializer.Serialize(new { value })
    };

    private static string Sizes(string name = "M", int maxWords = 100) =>
        $$"""{ "items": [{ "name": "{{name}}", "width": 15, "height": 25, "base_price": 20000, "max_words": {{maxWords}} }] }""";

    private static string Prices() =>
        """{ "items": [{ "min_words": 0, "max_words": 35, "extra_price": 0 }, { "min_words": 36, "max_words": 70, "extra_price": 39000 }, { "min_words": 71, "max_words": 100, "extra_price": 69000 }] }""";

    private static List<ConfigEntity> ValidConfigs() =>
        [Config("card_config.sizes", Sizes()), Config("card_config.prices", Prices())];

    private static CardRequest.CreateCardRequest Request(Guid templateId, Guid? productId = null, Guid? flowerId = null,
        string message = "one two three four five six seven eight") => new()
    {
        CardTemplateId = templateId, ProductId = productId, GeneratedFlowerId = flowerId, SizeName = "M",
        MessageContent = message
    };

    private static async Task AssertConflict(CardService service, CardRequest.CreateCardRequest request) =>
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateHandMadeCard(request));

    [Fact]
    public async Task CreateHandMade_Success_FromProduct()
    {
        var template = Template();
        var product = Product();
        var cards = new List<GeneratedCard>();
        var histories = new List<ClientHistory>();
        var result =
            await CreateService([template], [product], configs: ValidConfigs(), cards: cards, histories: histories)
                .CreateHandMadeCard(Request(template.Id, product.Id));

        var card = Assert.Single(cards);
        var history = Assert.Single(histories);
        Assert.Equal(8, result.WordCount);
        Assert.Equal(0, result.BasePrice);
        Assert.Equal(0, result.ExtraPrice);
        Assert.Equal(GeneratedCardType.Handmade, card.CardType);
        Assert.Equal(product.Id, history.BaseId);
        Assert.Equal(product.Id, history.RootId);
        Assert.Equal(ClientHistoryType.HandmadeCard, history.Type);
    }

    [Fact]
    public async Task CreateHandMade_Success_FromGeneratedFlower()
    {
        var template = Template();
        var flower = new GeneratedFlower
            { Id = Guid.NewGuid(), UserId = UserId, ImageUrl = "https://example.com/flower.png" };
        var rootId = Guid.NewGuid();
        var histories = new List<ClientHistory>
        {
            new()
            {
                Id = Guid.NewGuid(), UserId = UserId, Type = ClientHistoryType.Flower, OutputId = flower.Id,
                RootId = rootId
            }
        };
        var cards = new List<GeneratedCard>();
        var result =
            await CreateService([template], flowers: [flower], histories: histories, configs: ValidConfigs(),
                cards: cards).CreateHandMadeCard(Request(template.Id, flowerId: flower.Id, message: ""));

        Assert.Equal(0, result.WordCount);
        Assert.Equal(0, result.TotalPrice);
        Assert.Equal(flower.Id, Assert.Single(histories.Where(x => x.Type == ClientHistoryType.HandmadeCard)).BaseId);
        Assert.Equal(rootId, histories.Single(x => x.Type == ClientHistoryType.HandmadeCard).RootId);
    }

    [Fact]
    public async Task CreateHandMade_BothIdsNull_ThrowsBadRequest() =>
        await Assert.ThrowsAsync<BadRequestException>(() =>
            CreateService().CreateHandMadeCard(Request(Guid.NewGuid())));

    [Fact]
    public async Task CreateHandMade_BothIdsProvided_ThrowsBadRequest() =>
        await Assert.ThrowsAsync<BadRequestException>(() =>
            CreateService().CreateHandMadeCard(Request(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())));

    [Fact]
    public async Task CreateHandMade_ProductNotFound_ThrowsNotFound()
    {
        var template = Template();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService([template]).CreateHandMadeCard(Request(template.Id, Guid.NewGuid())));
    }

    [Fact]
    public async Task CreateHandMade_GeneratedFlowerNotFound_ThrowsNotFound()
    {
        var template = Template();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService([template]).CreateHandMadeCard(Request(template.Id, flowerId: Guid.NewGuid())));
    }

    [Fact]
    public async Task CreateHandMade_TemplateNotFound_ThrowsNotFound() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService().CreateHandMadeCard(Request(Guid.NewGuid(), Guid.NewGuid())));

    [Fact]
    public async Task CreateHandMade_TemplateDeleted_ThrowsNotFound()
    {
        var template = Template(deleted: true);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateService([template]).CreateHandMadeCard(Request(template.Id, Guid.NewGuid())));
    }

    [Fact]
    public async Task CreateHandMade_TemplateInactive_ThrowsConflict()
    {
        var template = Template(active: false);
        await AssertConflict(CreateService([template]), Request(template.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateHandMade_TemplateTypeAi_ThrowsConflict()
    {
        var template = Template(type: CardTemplateType.Ai);
        await AssertConflict(CreateService([template]), Request(template.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateHandMade_SizeConfigNotFound_ThrowsConflict()
    {
        var template = Template();
        var product = Product();
        await AssertConflict(CreateService([template], [product], configs: [Config("card_config.prices", Prices())]),
            Request(template.Id, product.Id));
    }

    [Fact]
    public async Task CreateHandMade_SizeConfigDeleted_ThrowsConflict() =>
        await AssertSizeConfigConflict(Config("card_config.sizes", Sizes(), deleted: true));

    [Fact]
    public async Task CreateHandMade_SizeConfigInactive_ThrowsConflict() =>
        await AssertSizeConfigConflict(Config("card_config.sizes", Sizes(), isPublic: false));

    [Fact]
    public async Task CreateHandMade_SizeConfigInvalid_ThrowsConflict() =>
        await AssertSizeConfigConflict(Config("card_config.sizes", "not json"));

    [Fact]
    public async Task CreateHandMade_MessageExceedsSize_ThrowsBadRequest()
    {
        var template = Template();
        var product = Product();
        await Assert.ThrowsAsync<BadRequestException>(() =>
            CreateService([template], [product],
                    configs: [Config("card_config.sizes", Sizes(maxWords: 5)), Config("card_config.prices", Prices())])
                .CreateHandMadeCard(Request(template.Id, product.Id, message: "one two three four five six")));
    }

    [Fact]
    public async Task CreateHandMade_PriceConfigNotFound_ThrowsConflict() => await AssertPriceConfigConflict([]);

    [Fact]
    public async Task CreateHandMade_PriceConfigDeleted_ThrowsConflict() =>
        await AssertPriceConfigConflict([Config("card_config.prices", Prices(), deleted: true)]);

    [Fact]
    public async Task CreateHandMade_PriceConfigInactive_ThrowsConflict() =>
        await AssertPriceConfigConflict([Config("card_config.prices", Prices(), isPublic: false)]);

    [Fact]
    public async Task CreateHandMade_PriceConfigInvalid_ThrowsConflict() =>
        await AssertPriceConfigConflict([Config("card_config.prices", "not json")]);

    [Fact]
    public async Task CreateHandMade_WordCountNotInPriceRule_ThrowsConflict()
    {
        var template = Template();
        var product = Product();
        var prices =
            """{ "items": [{ "min_words": 0, "max_words": 35, "extra_price": 0 }, { "min_words": 51, "max_words": 100, "extra_price": 69000 }] }""";
        await AssertConflict(
            CreateService([template], [product],
                configs: [Config("card_config.sizes", Sizes()), Config("card_config.prices", prices)]),
            Request(template.Id, product.Id, message: string.Join(' ', Enumerable.Repeat("word", 40))));
    }

    private static async Task AssertSizeConfigConflict(ConfigEntity sizeConfig)
    {
        var template = Template();
        var product = Product();
        await AssertConflict(
            CreateService([template], [product], configs: [sizeConfig, Config("card_config.prices", Prices())]),
            Request(template.Id, product.Id));
    }

    private static async Task AssertPriceConfigConflict(List<ConfigEntity> priceConfigs)
    {
        var template = Template();
        var product = Product();
        priceConfigs.Insert(0, Config("card_config.sizes", Sizes()));
        await AssertConflict(CreateService([template], [product], configs: priceConfigs),
            Request(template.Id, product.Id));
    }
}