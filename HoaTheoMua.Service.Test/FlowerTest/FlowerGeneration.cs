using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Test.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Ai = HoaTheoMua.Service.AiService;
using Flower = HoaTheoMua.Service.Flower;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class FlowerGeneration
{
    [Fact]
    public async Task CreatePersistsTwoOutputsPromptPairAndCompleteSnapshots()
    {
        using var fixture = new Fixture();
        var result = await fixture.Service.CreateAiFlower(fixture.Request);
        Assert.Equal("https://example.com/flower.png", result.ImageUrl);
        Assert.Equal(result.ImageUrl, result.FlowerImageUrl);
        Assert.Equal("https://example.com/branded.png", result.FlowerWithLogoImageUrl);
        Assert.Equal("https://example.com/composite.png", result.FlowerWithMockupImageUrl);
        var input = Assert.Single(fixture.Inputs);
        Assert.Equal("create exact\n", input.CreateSystemPrompt);
        Assert.Equal("mockup exact\n", input.MockupSystemPrompt);
        Assert.Equal("guard exact\n", input.GuardSystemPrompt);
        Assert.Equal("giấy kraft", input.WrappingPaper.Description);
        Assert.Equal("trắng", input.WrappingPaper.Color);
        Assert.Equal("Nếp gói thấp", input.Bundle.DescriptionPrompt);
        var material = Assert.Single(input.Materials);
        Assert.Equal(10, material.BaseQuantity);
        Assert.Equal(2, material.AdditionalQuantity);
        Assert.Equal(12, material.FinalQuantity);
        Assert.Equal("cành", material.Unit);
        var saved = await fixture.Db.GeneratedFlowers.SingleAsync();
        Assert.Equal(result.FlowerWithMockupImageUrl, saved.FlowerWithMockupImageUrl);
        Assert.Equal(result.FlowerWithLogoImageUrl, saved.FlowerWithLogoImageUrl);
        fixture.Branding.Verify(x => x.ApplyLogoAsync("https://example.com/composite.png", saved.Id, It.IsAny<CancellationToken>()), Times.Once);
        using var metadata = JsonDocument.Parse(saved.Metadata);
        Assert.Equal(result.FlowerWithMockupImageUrl, metadata.RootElement.GetProperty("flowerWithMockupImageUrl").GetString());
        var history = await fixture.Db.ClientHistories.SingleAsync();
        using var prompt = JsonDocument.Parse(history.SystemForm!);
        Assert.Equal(input.CreateSystemPrompt, prompt.RootElement.GetProperty("content").GetString());
        Assert.Equal(input.MockupSystemPrompt, prompt.RootElement.GetProperty("mockupContent").GetString());
        Assert.Equal(input.GuardSystemPrompt, prompt.RootElement.GetProperty("guardContent").GetString());
        Assert.Equal((await fixture.Db.Configs.SingleAsync(x => x.Key.EndsWith("guard_rules"))).Id,
            prompt.RootElement.GetProperty("guardConfigId").GetGuid());
        using var historyData = JsonDocument.Parse(history.Metadata);
        Assert.Equal(5, historyData.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("Nếp gói thấp", historyData.RootElement.GetProperty("bundle").GetProperty("descriptionPrompt").GetString());
    }

    [Fact]
    public async Task RegenerateUsesHistoricalCompositionAndPromptPairWhenConfigsRemoved()
    {
        using var fixture = new Fixture();
        var created = await fixture.Service.CreateAiFlower(fixture.Request);
        fixture.Db.Configs.RemoveRange(fixture.Db.Configs);
        var mat = await fixture.Db.Products.SingleAsync(x => x.Id == fixture.Request.MaterialTotals[0].MatId);
        mat.ProductName = "Changed live material";
        mat.CoverImage = "https://example.com/changed.png";
        await fixture.Db.SaveChangesAsync();
        var regenerated = await fixture.Service.RegenerateAiFlower(created.Id, new());
        Assert.Equal(created.SessionId, regenerated.SessionId);
        Assert.Equal(2, fixture.Inputs.Count);
        Assert.Equal(fixture.Inputs[0], fixture.Inputs[1] with { Materials = fixture.Inputs[0].Materials });
        Assert.Equal(fixture.Inputs[0].Materials[0], fixture.Inputs[1].Materials[0]);
        var history = await fixture.Db.ClientHistories.SingleAsync(x => x.OutputId == regenerated.Id);
        using var metadata = JsonDocument.Parse(history.Metadata);
        Assert.Equal("source_flower_fallback", metadata.RootElement.GetProperty("systemPromptSource").GetString());
        Assert.Equal(created.Id, metadata.RootElement.GetProperty("regeneratedFromFlowerId").GetGuid());
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("wrong-kind")]
    public async Task InvalidPromptPairNeverCallsAi(string scenario)
    {
        using var fixture = new Fixture();
        var config = await fixture.Db.Configs.SingleAsync(x => x.Key.EndsWith("create_flowers"));
        if (scenario == "missing") fixture.Db.Configs.Remove(config);
        if (scenario == "empty") config.Value = "{\"value\":\" \"}";
        if (scenario == "wrong-kind") config.Kind = "content";
        await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => fixture.Service.CreateAiFlower(fixture.Request));
        Assert.Empty(fixture.Inputs);
        Assert.Empty(fixture.Db.GeneratedFlowers);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("empty")]
    [InlineData("malformed")]
    public async Task InvalidGuardStopsCreateAndRegenerateBeforeAi(string scenario)
    {
        using var fixture = new Fixture();
        var created = await fixture.Service.CreateAiFlower(fixture.Request);
        var guard = await fixture.Db.Configs.SingleAsync(x => x.Key.EndsWith("guard_rules"));
        if (scenario == "missing") fixture.Db.Configs.Remove(guard);
        if (scenario == "empty") guard.Value = "{\"value\":\" \"}";
        if (scenario == "malformed") guard.Value = "{\"value\":123}";
        await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => fixture.Service.CreateAiFlower(fixture.Request));
        await Assert.ThrowsAsync<ConflictException>(() => fixture.Service.RegenerateAiFlower(created.Id, new()));
        Assert.Single(fixture.Inputs);
        Assert.Single(fixture.Db.GeneratedFlowers);
    }

    [Fact]
    public async Task HistoricalPromptPairWithoutGuardCannotBypassGuardRequirement()
    {
        using var fixture = new Fixture();
        var created = await fixture.Service.CreateAiFlower(fixture.Request);
        fixture.Db.Configs.RemoveRange(fixture.Db.Configs);
        (await fixture.Db.ClientHistories.SingleAsync()).SystemForm = JsonSerializer.Serialize(new
        { type = "flower", content = "old create", mockupContent = "old mockup" });
        await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => fixture.Service.RegenerateAiFlower(created.Id, new()));
        Assert.Single(fixture.Inputs);
    }

    [Fact]
    public async Task BrandingFailureDoesNotSaveCreateOrRegenerateResults()
    {
        using var fixture = new Fixture();
        var created = await fixture.Service.CreateAiFlower(fixture.Request);
        fixture.Branding.Setup(x => x.ApplyLogoAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadGatewayException("Upload failed"));
        await Assert.ThrowsAsync<BadGatewayException>(() => fixture.Service.CreateAiFlower(fixture.Request));
        await Assert.ThrowsAsync<BadGatewayException>(() => fixture.Service.RegenerateAiFlower(created.Id, new()));
        Assert.Single(fixture.Db.GeneratedFlowers);
        Assert.Single(fixture.Db.ClientHistories);
    }

    [Fact]
    public async Task AiFailureDoesNotSaveFlowerOrHistory()
    {
        using var fixture = new Fixture();
        fixture.Ai.Setup(x => x.GenerateFlowerImage(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadGatewayException("mockup failed"));
        await Assert.ThrowsAsync<BadGatewayException>(() => fixture.Service.CreateAiFlower(fixture.Request));
        Assert.Empty(fixture.Db.GeneratedFlowers);
        Assert.Empty(fixture.Db.ClientHistories);
    }

    [Fact]
    public async Task RegenerateDoesNotTreatLegacySinglePromptAsCompletePair()
    {
        using var fixture = new Fixture();
        var created = await fixture.Service.CreateAiFlower(fixture.Request);
        fixture.Db.Configs.RemoveRange(fixture.Db.Configs);
        (await fixture.Db.ClientHistories.SingleAsync()).SystemForm = "{\"type\":\"flower\",\"content\":\"old combined prompt\"}";
        await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ConflictException>(() => fixture.Service.RegenerateAiFlower(created.Id, new()));
        Assert.Single(fixture.Inputs);
        Assert.Single(fixture.Db.GeneratedFlowers);
    }

    // InMemory proves orchestration and serialized state, not relational transaction semantics.
    internal sealed class Fixture : IDisposable
    {
        public DbContextOptions<AppDbContext> Options { get; }
        public AppDbContext Db { get; }
        public Mock<ICurrentUserService> Current { get; } = new();
        public HttpContextAccessor Context { get; } = new() { HttpContext = new DefaultHttpContext() };
        public Mock<Ai.IService> Ai { get; } = new();
        public Mock<Flower.IFlowerImageBranding> Branding { get; } = new();
        public List<Ai.Request.FlowerImageInput> Inputs { get; } = new();
        public Flower.Service Service { get; }
        public Flower.Request.CreateFlowerRequest Request { get; }
        private readonly HttpClient _http = new();

        public Fixture(DbContextOptions<AppDbContext>? options = null)
        {
            Options = options ?? new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
            Db = new(Options);
            var product = Product("Bó hồng", "Combo");
            var material = Product("Hồng", "Sản phẩm");
            material.Stock = 100; material.Unit = "cành";
            var paper = Product("Giấy", "Sản phẩm");
            paper.ProductLabel = "GIẤY"; paper.Color = "trắng"; paper.Description = "giấy kraft";
            var ribbon = Product("Ruy băng", "Sản phẩm"); ribbon.ProductLabel = "RUY BĂNG";
            var bundle = new HoaTheoMua.Repository.Entity.FlowerReference { Id = Guid.NewGuid(), Name = "Gói tròn", DescriptionPrompt = "Nếp gói thấp",
                ImageUrl = "https://example.com/bundle.png", IsActive = true, Type = FlowerReferenceType.Bundle };
            var mockup = new HoaTheoMua.Repository.Entity.FlowerReference { Id = Guid.NewGuid(), Name = "Cô gái", Description = "Cầm bó hoa",
                ImageUrl = "https://example.com/mockup.png", IsActive = true, Type = FlowerReferenceType.Flower };
            Db.Products.AddRange(product, material, paper, ribbon);
            Db.ComboSpecifications.Add(new ComboSpecification { Id = Guid.NewGuid(), SubId = product.Id,
                MatId = material.Id, Quantity = 10, IsCore = true, IsActive = true });
            Db.FlowerReferences.AddRange(bundle, mockup);
            Db.Configs.AddRange(Config("imagegen.flower_prompt_create_flowers", "create exact\n"),
                Config("imagegen.flower_prompt_flowers_apply_mockup", "mockup exact\n"),
                Config("imagegen.flower_prompt_guard_rules", "guard exact\n"));
            Db.SaveChanges();
            var current = Current;
            current.Setup(x => x.GetRequiredUserId()).Returns(Guid.NewGuid());
            var context = Context;
            var products = new HoaTheoMua.Service.Product.Service(Db, context, _http,
                new ConfigurationBuilder().Build(), MapperFactory.Create());
            Ai.Setup(x => x.GenerateFlowerImage(It.IsAny<Ai.Request.FlowerImageInput>(), It.IsAny<CancellationToken>()))
                .Callback<Ai.Request.FlowerImageInput, CancellationToken>((input, _) => Inputs.Add(input))
                .ReturnsAsync(new Ai.Response.FlowerGeneratedResponse("https://example.com/flower.png",
                    "https://example.com/composite.png", "test", "request-id", 10,
                    new("https://example.com/flower.png", "test", "stage-1", 5),
                    new("https://example.com/composite.png", "test", "stage-2", 5)));
            Branding.Setup(x => x.ApplyLogoAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Flower.BrandedFlowerImage("https://example.com/branded.png", "test/branded.png",
                    "logo-hash", 1024, 1024, 788, 883, 205, 110));
            Service = new Flower.Service(Db, current.Object, context, Ai.Object, products, Branding.Object);
            Request = new() { Name = "Bó sáng tạo", ProductId = product.Id, BundleId = bundle.Id, MockupId = mockup.Id,
                RibbonProductId = ribbon.Id, WrappingPaperProductId = paper.Id,
                MaterialTotals = new() { new() { MatId = material.Id, Quantity = 12 } } };
        }

        private static ProductEntity Product(string name, string type) => new() { Id = Guid.NewGuid(),
            ProductName = name, ProductType = type, CoverImage = $"https://example.com/{Guid.NewGuid()}.png", IsActive = true };
        private static ConfigEntity Config(string key, string prompt) => new() { Id = Guid.NewGuid(),
            Key = key, Group = "imagegen", Kind = "Setting", Value = JsonSerializer.Serialize(new { value = prompt }) };
        public void Dispose() { Db.Dispose(); _http.Dispose(); }
    }
}
