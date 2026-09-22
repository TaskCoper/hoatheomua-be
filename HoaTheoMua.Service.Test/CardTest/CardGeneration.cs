using System.Text.Json;
using System.Text.Json.Nodes;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.CardJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MassTransit;
using Moq;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;
using Card = HoaTheoMua.Service.Card;
using static HoaTheoMua.Service.Card.MappingCard;

namespace HoaTheoMua.Service.Test.CardTest;

public class CardGeneration
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateUsesChosenBouquetAndFrozenPromptThenPersistsCardHistoryAndSnapshot(bool generated)
    {
        using var f = new Fixture(generated);
        var job = await f.Jobs.Create(f.Request, "create", default);
        Assert.Empty(f.Inputs);
        var replay = await f.Jobs.Create(f.Request, "create", default);
        Assert.Equal(job.JobId, replay.JobId);
        await Assert.ThrowsAsync<ConflictException>(() => f.Jobs.Create(f.Request, "other", default));
        f.Context.HttpContext!.RequestAborted = new CancellationToken(true);
        await f.Worker.Process(job.JobId, default);
        await f.Worker.Process(job.JobId, default);
        var done = await f.Jobs.Get(job.JobId, default);
        Assert.Equal("Succeeded", done.Status);
        var input = Assert.Single(f.Inputs);
        Assert.Equal(generated ? "https://example.com/chosen-flower.png" : "https://example.com/product.png", input.BouquetImageUrl);
        Assert.Equal("snapshot prompt\n", input.SystemPrompt);
        Assert.Equal(10, input.Width); Assert.Equal(15, input.Height);
        var card = await f.Db.GeneratedCards.SingleAsync();
        Assert.Equal(done.Result!.Id, card.Id);
        var history = await f.Db.ClientHistories.SingleAsync(x => x.Type == ClientHistoryType.Card);
        Assert.Null(history.SessionId);
        var metadata = JsonNode.Parse(history.Metadata)!;
        Assert.Equal("create", metadata["operation"]!.GetValue<string>());
        Assert.Null(metadata["regeneratedFromCardId"]);
        var snapshot = await f.Snapshot();
        Assert.Equal("done", snapshot["status"]!.GetValue<string>());
        Assert.Equal(card.Id.ToString(), snapshot["idResult"]![0]!.GetValue<string>());
        Assert.Null(snapshot["finalGeneratedCardId"]);
        await Assert.ThrowsAsync<ConflictException>(() => f.Jobs.Create(f.Request, "repeat", default));
    }

    [Fact]
    public async Task RawImageStillCreatesAndRegenerateUsesHistoryBouquetInsteadOfPreviousCard()
    {
        using var f = new Fixture(true);
        var snapshot = await f.Snapshot(); snapshot["rawImage"] = "https://example.com/background.png";
        await f.SetSnapshot(snapshot);
        var first = await f.Run("first");
        Assert.Equal("https://example.com/background.png", f.Inputs[0].RawImage);
        await f.Service.UpdateFinalGeneratedCardId(new() { OrderDetailId = f.DetailId, VirtualId = f.VirtualId, GeneratedCardId = first });
        // The chosen image is frozen in card history even if the source record changes.
        (await f.Db.GeneratedFlowers.SingleAsync()).ImageUrl = "https://example.com/changed.png";
        await f.Db.SaveChangesAsync();
        var changed = new Card.Request.CreateCardAiRequest { SourceCardId = first, MessageContent = "Chúc tốt nghiệp" };
        var second = await f.Run("second", changed);
        Assert.Equal("https://example.com/chosen-flower.png", f.Inputs[1].BouquetImageUrl);
        Assert.Null(f.Inputs[1].RawImage);
        Assert.Equal("current prompt\n", f.Inputs[1].SystemPrompt);
        f.Db.Configs.RemoveRange(f.Db.Configs); await f.Db.SaveChangesAsync();
        var third = await f.Run("third", new() { SourceCardId = second });
        Assert.Equal("current prompt\n", f.Inputs[2].SystemPrompt);
        var history = await f.Db.ClientHistories.SingleAsync(x => x.OutputId == third);
        Assert.Equal(second, JsonNode.Parse(history.Metadata)!["regeneratedFromCardId"]!.GetValue<Guid>());
        snapshot = await f.Snapshot();
        Assert.Equal(3, snapshot["idResult"]!.AsArray().Count);
        Assert.Equal(first.ToString(), snapshot["finalGeneratedCardId"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("{\"value\":\" \"}")]
    [InlineData("{\"value\":42}")]
    [InlineData("[]")]
    public async Task InvalidCurrentConfigDoesNotSilentlyFallback(string value)
    {
        using var f = new Fixture(); var first = await f.Run("first");
        (await f.Db.Configs.SingleAsync()).Value = value; await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => f.Jobs.Create(new() { SourceCardId = first }, "bad", default));
        Assert.Single(f.Inputs);
    }

    [Fact]
    public async Task ExplicitNullOverridesRejectedBeforeAiAndUnpaidOrForeignOrdersRejected()
    {
        using var f = new Fixture();
        var request = JsonSerializer.Deserialize<Card.Request.CreateCardAiRequest>(
            $$"""{"orderDetailId":"{{f.DetailId}}","virtualId":"{{f.VirtualId}}","messageContent":null}""",
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        await Assert.ThrowsAsync<BadRequestException>(() => f.Jobs.Create(request, "null", default));
        var order = await f.Db.Orders.SingleAsync(); order.Status = "Pending"; await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => f.Jobs.Create(f.Request, "unpaid", default));
        f.Current.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(() => f.Jobs.Create(f.Request, "foreign", default));
        Assert.Empty(f.Inputs);
    }

    [Fact]
    public async Task CancelAndFailureNeverPersistAndRedeliveryDoesNotSpendAgain()
    {
        using var f = new Fixture();
        var job = await f.Jobs.Create(f.Request, "cancel", default);
        var owner = f.Current.Object.GetRequiredUserId(); f.Current.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(() => f.Jobs.Get(job.JobId, default));
        await Assert.ThrowsAsync<NotFoundException>(() => f.Jobs.Cancel(job.JobId, default));
        f.Current.Setup(x => x.GetRequiredUserId()).Returns(owner);
        await f.Jobs.Cancel(job.JobId, default); await f.Worker.Process(job.JobId, default);
        Assert.Empty(f.Inputs);
        job = await f.Jobs.Create(f.Request, "failure", default);
        f.Ai.Setup(x => x.GenerateCardImage(It.IsAny<Ai.Request.CardImageInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadGatewayException("provider failed"));
        await f.Worker.Process(job.JobId, default); await f.Worker.Process(job.JobId, default);
        Assert.Equal("Failed", (await f.Jobs.Get(job.JobId, default)).Status);
        Assert.Empty(f.Db.GeneratedCards);
        Assert.Equal("not yet", (await f.Snapshot())["status"]!.GetValue<string>());
        f.Ai.Verify(x => x.GenerateCardImage(It.IsAny<Ai.Request.CardImageInput>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAfterProviderReturnsKeepsCheckpointButDoesNotCommitCard()
    {
        using var f = new Fixture(); var job = await f.Jobs.Create(f.Request, "race", default);
        f.Ai.Setup(x => x.GenerateCardImage(It.IsAny<Ai.Request.CardImageInput>(), It.IsAny<CancellationToken>()))
            .Returns(async () => { await f.Jobs.Cancel(job.JobId, default); return new Ai.Response.GeneratedImage("https://example.com/out.png", "test", "id", 10); });
        await f.Worker.Process(job.JobId, default);
        Assert.Equal("Canceled", (await f.Jobs.Get(job.JobId, default)).Status);
        Assert.Empty(f.Db.GeneratedCards);
        Assert.NotNull(JobJson.Read<Checkpoint>((await f.Db.CardGenerationJobs.SingleAsync()).Checkpoint).Card);
    }

    [Fact]
    public async Task RegenerateJobsAreNotLockedBySourceAndPreserveEveryResult()
    {
        using var f = new Fixture(); var first = await f.Run("first");
        var request = new Card.Request.CreateCardAiRequest { SourceCardId = first };
        var a = await f.Jobs.Create(request, "a", default); var b = await f.Jobs.Create(request, "b", default);
        await f.Worker.Process(a.JobId, default); await f.Worker.Process(b.JobId, default);
        Assert.Equal(3, (await f.Snapshot())["idResult"]!.AsArray().Count);
        Assert.Equal(3, await f.Db.GeneratedCards.CountAsync());
    }

    [Fact]
    public async Task RecoveryDoesNotReplayUnknownProviderOutcome()
    {
        using var f = new Fixture(); var job = await f.Jobs.Create(f.Request, "stale", default);
        var row = await f.Db.CardGenerationJobs.SingleAsync(); row.Status = "Running";
        row.LeaseId = Guid.NewGuid(); row.LeaseUntil = DateTimeOffset.UtcNow.AddMinutes(-1); await f.Db.SaveChangesAsync();
        await new Recovery(f.Db, Mock.Of<IPublishEndpoint>(), new Store(f.Db, TimeProvider.System), TimeProvider.System).Scan(default);
        await f.Worker.Process(job.JobId, default);
        Assert.Equal("Failed", (await f.Jobs.Get(job.JobId, default)).Status); Assert.Empty(f.Inputs);
    }

    internal sealed class Fixture : IDisposable
    {
        public AppDbContext Db { get; }
        public Mock<ICurrentUserService> Current { get; } = new();
        public HttpContextAccessor Context { get; } = new() { HttpContext = new DefaultHttpContext() };
        public Mock<Ai.IService> Ai { get; } = new();
        public List<Ai.Request.CardImageInput> Inputs { get; } = new();
        public Card.Service Service { get; }
        public HoaTheoMua.Service.CardJobs.Service Jobs { get; }
        public Worker Worker { get; }
        public Guid DetailId { get; } = Guid.NewGuid();
        public Guid VirtualId { get; } = Guid.NewGuid();
        public Card.Request.CreateCardAiRequest Request => new() { OrderDetailId = DetailId, VirtualId = VirtualId };
        public Fixture(bool generated = false, DbContextOptions<AppDbContext>? options = null)
        {
            Db = new(options ?? new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var user = Guid.NewGuid(); var product = Guid.NewGuid(); var flowerId = generated ? Guid.NewGuid() : (Guid?)null;
            Current.Setup(x => x.GetRequiredUserId()).Returns(user);
            if (generated)
            {
                Db.GeneratedFlowers.Add(new() { Id = flowerId!.Value, UserId = user, Name = "Selected bouquet",
                    ImageUrl = "https://example.com/chosen-flower.png", Metadata = "{}" });
                Db.ClientHistories.Add(new() { Id = Guid.NewGuid(), UserId = user, Type = ClientHistoryType.Flower,
                    BaseId = product, RootId = product, OutputId = flowerId.Value, Metadata = "{}" });
            }
            var snapshot = new CardSnapshot { VirtualId = VirtualId, CardType = "ai", Quantity = 1,
                Source = new() { Id = product, Type = "product", ImageUrl = "https://example.com/product.png" },
                CardTemplate = new() { TemplateId = Guid.NewGuid(), TemplateName = "Birthday", TemplateType = "ai", TemplateImageUrl = "https://example.com/template.png" },
                CardSize = new() { Name = "S", Width = 10, Height = 15, MaxWords = 70, BasePrice = 10000 },
                MessageContent = "Chúc mừng sinh nhật", FormType = "go_may", WordCount = 4, BasePrice = 10000, TotalPrice = 10000,
                SystemPrompt = new() { Type = "card", Content = "snapshot prompt\n" }, Status = "not yet" };
            Db.Orders.Add(new HoaTheoMua.Repository.Entity.Order { Id = Guid.NewGuid(), UserId = user, Ward = 1, City = 1,
                Address = "Address", ShortAddress = "Address", FullName = "Customer", Phone = "0900000000", Remarks = "",
                OrderTime = 0, Status = "Paid", TrackNumber = "", PhoneReceiver = "0900000000", FullNameReceiver = "Customer",
                PaymentGateway = PaymentGateway.BaoKim, ShippingGateway = ShippingGateway.XanhSm,
                OrderDetails = [new() { Id = DetailId, Name = "Flower", ProductId = product,
                    Metadata = JsonSerializer.Serialize(new { productMetadata = new[] { new { generatedFlower = flowerId, snapShotCard = snapshot } } }, JobJson.Options) }] });
            Db.Configs.Add(new() { Id = Guid.NewGuid(), Key = "imagegen.card_prompt", Group = "imagegen", Kind = "Setting",
                Value = "{\"value\":\"current prompt\\n\"}" });
            Db.SaveChanges();
            Ai.Setup(x => x.GenerateCardImage(It.IsAny<Ai.Request.CardImageInput>(), It.IsAny<CancellationToken>()))
                .Callback<Ai.Request.CardImageInput, CancellationToken>((input, _) => Inputs.Add(input))
                .ReturnsAsync(() => new("https://example.com/" + Guid.NewGuid() + ".png", "test-model", "request-id", 10));
            Service = new(Db, Current.Object, Context, Ai.Object);
            var store = new Store(Db, TimeProvider.System);
            Jobs = new(Db, Service, Current.Object, Mock.Of<IPublishEndpoint>(), store, TimeProvider.System, Options.Create(new CardJobOptions()));
            Worker = new(Db, store, Ai.Object, Service, Mock.Of<IServiceScopeFactory>(), Options.Create(new CardJobOptions()), TimeProvider.System, NullLogger<Worker>.Instance);
        }
        public async Task<JsonObject> Snapshot() => JsonNode.Parse((await Db.OrderDetails.AsNoTracking().SingleAsync(x => x.Id == DetailId)).Metadata!)!["productMetadata"]![0]!["snapShotCard"]!.AsObject();
        public async Task SetSnapshot(JsonObject snapshot)
        {
            var detail = await Db.OrderDetails.SingleAsync(x => x.Id == DetailId);
            var metadata = JsonNode.Parse(detail.Metadata!)!; metadata["productMetadata"]![0]!["snapShotCard"] = snapshot.DeepClone();
            detail.Metadata = metadata.ToJsonString(); await Db.SaveChangesAsync();
        }
        public async Task<Guid> Run(string key, Card.Request.CreateCardAiRequest? request = null)
        {
            var job = await Jobs.Create(request ?? Request, key, default); await Worker.Process(job.JobId, default);
            var result = await Jobs.Get(job.JobId, default); Assert.Equal("Succeeded", result.Status); return result.Result!.Id;
        }
        public void Dispose() => Db.Dispose();
    }
}
