using HoaTheoMua.Service.Flower;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.Card;

public class CardImageOptions : FlowerImageOptions
{
    public CardImageOptions() { ObjectPrefix = "general/ai-cards"; LogoWidthRatio = 0.16; }
}

public interface ICardImageBranding : IFlowerImageBranding { }

public sealed class CardImageBranding(IHttpClientFactory clients, IOptions<CardImageOptions> options,
    ITaskcoperStorageSession storage)
    : FlowerImageBranding(clients, Options.Create<FlowerImageOptions>(options.Value), storage), ICardImageBranding
{
    protected override Task<FlowerLogoComposer.Result> ComposeAsync(byte[] image, byte[] logo,
        FlowerImageOptions settings, CancellationToken ct)
        => FlowerLogoComposer.ComposeAsync(image, logo, settings, ct, fitShortEdge: true);
}
