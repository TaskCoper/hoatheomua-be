using HoaTheoMua.Repository.Enum;
namespace HoaTheoMua.Service.PostGen;

public static class AspectRatios
{
    public static (int Width, int Height) Pixels(AspectRatioType ratio) => ratio switch
    {
        AspectRatioType.Square1x1 => (1080, 1080),
        AspectRatioType.Portrait4x5 => (1080, 1350),
        AspectRatioType.Portrait3x4 => (1080, 1440),
        AspectRatioType.Story9x16 => (1080, 1920),
        AspectRatioType.Landscape16x9 => (1920, 1080),
        _ => throw new ArgumentOutOfRangeException(nameof(ratio))
    };
}
