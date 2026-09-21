using System.Security.Cryptography;
using HoaTheoMua.Service.Exceptions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HoaTheoMua.Service.Flower;

public static class FlowerLogoComposer
{
    public record Result(byte[] Png, string LogoSha256, int Width, int Height,
        int LogoX, int LogoY, int LogoWidth, int LogoHeight);

    public static async Task<Result> ComposeAsync(byte[] mockup, byte[] logo, FlowerImageOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!double.IsFinite(options.LogoWidthRatio) || options.LogoWidthRatio is <= 0 or > 0.5
            || !double.IsFinite(options.MarginRatio) || options.MarginRatio is < 0 or > 0.2
            || options.MaxImagePixels <= 0)
            throw new ServerException("Flower logo configuration is invalid");
        try
        {
            var decoder = new DecoderOptions { MaxFrames = 1 };
            var info = Image.Identify(mockup);
            var logoInfo = Image.Identify(logo);
            if ((long)info.Width * info.Height > options.MaxImagePixels
                || (long)logoInfo.Width * logoInfo.Height > options.MaxImagePixels)
                throw new BadGatewayException("Flower image dimensions exceed the supported limit");
            using var image = Image.Load<Rgba32>(decoder, mockup);
            using var mark = Image.Load<Rgba32>(decoder, logo);
            image.Mutate(x => x.AutoOrient());
            mark.Mutate(x => x.AutoOrient());
            var margin = (int)Math.Round(Math.Min(image.Width, image.Height) * options.MarginRatio);
            var availableWidth = image.Width - 2 * margin;
            var availableHeight = image.Height - 2 * margin;
            var width = Math.Max(1, Math.Min(availableWidth, (int)Math.Round(image.Width * options.LogoWidthRatio)));
            var scale = Math.Min((double)width / mark.Width, (double)availableHeight / mark.Height);
            var logoWidth = Math.Clamp((int)Math.Round(mark.Width * scale), 1, availableWidth);
            var logoHeight = Math.Clamp((int)Math.Round(mark.Height * scale), 1, availableHeight);
            mark.Mutate(x => x.Resize(logoWidth, logoHeight));
            var left = image.Width - margin - logoWidth;
            var top = image.Height - margin - logoHeight;
            image.Mutate(x => x.DrawImage(mark, new Point(left, top), 1f));
            // Do not propagate source EXIF/XMP to the published PNG.
            image.Metadata.ExifProfile = null;
            image.Metadata.XmpProfile = null;
            using var output = new MemoryStream();
            await image.SaveAsPngAsync(output, cancellationToken);
            if (output.Length > options.MaxUploadBytes)
                throw new BadGatewayException("Branded flower image exceeds the upload size limit");
            return new(output.ToArray(), Convert.ToHexString(SHA256.HashData(logo)).ToLowerInvariant(),
                image.Width, image.Height, left, top, logoWidth, logoHeight);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
        {
            throw new BadGatewayException("Flower image could not be decoded");
        }
    }
}
