using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.PostPublishing;

public record PublishingImageInfo(int? Width, int? Height);

// Inspect the existing URL only; never rewrite, convert or store an image.
public sealed class PublishingMediaClient(HttpClient http)
{
    public static bool PublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var b = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
            return (b[0] & 0xe0) == 0x20 && !(b[0] == 0x20 && b[1] == 0x02) // 6to4 can embed private IPv4.
                && !(b[0] == 0x20 && b[1] == 0x01 && (b[2] < 2 || (b[2] == 0x0d && b[3] == 0xb8)));
        return b[0] is not (0 or 10 or 127) && b[0] < 224
            && !(b[0] == 169 && b[1] == 254) && !(b[0] == 172 && b[1] is >= 16 and <= 31)
            && !(b[0] == 192 && b[1] is 168 or 0) && !(b[0] == 100 && b[1] is >= 64 and <= 127)
            && !(b[0] == 198 && b[1] is 18 or 19) && !(b[0] == 192 && b[1] == 0 && b[2] == 2)
            && !(b[0] == 198 && b[1] == 51 && b[2] == 100) && !(b[0] == 203 && b[1] == 0 && b[2] == 113);
    }
    public static bool Url(string? value) => Service.PublicUrl(value) && Uri.TryCreate(value, UriKind.Absolute, out var u)
        && u.IsDefaultPort && u.Fragment.Length == 0 && (!IPAddress.TryParse(u.Host, out var ip) || PublicAddress(ip));
    public static HttpMessageHandler Handler() => new SocketsHttpHandler
    {
        AllowAutoRedirect = false, UseProxy = false, UseCookies = false,
        ConnectCallback = async (context, ct) =>
        {
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, ct);
            if (addresses.Length == 0 || addresses.Any(x => !PublicAddress(x))) throw new HttpRequestException("MEDIA_ADDRESS_BLOCKED");
            // Connect to the validated address, avoiding a second DNS lookup/rebinding. TLS still verifies the original host.
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try { await socket.ConnectAsync(new IPEndPoint(addresses[0], context.DnsEndPoint.Port), ct); return new NetworkStream(socket, ownsSocket: true); }
            catch { socket.Dispose(); throw; }
        }
    };
    public async Task<PublishingImageInfo> Inspect(PublicationImage image, PlatformType platform, CancellationToken ct)
    {
        var suffix = "_" + image.Order;
        if (!Url(image.Url)) throw new MetaPublishingException("MEDIA_URL_INVALID" + suffix, definitive: true);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, image.Url);
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode) throw new MetaPublishingException("MEDIA_FETCH_FAILED" + suffix, true, true);
            var limit = platform == PlatformType.Zalo ? 1_000_000 : (platform == PlatformType.Instagram ? 8 : 10) * 1024 * 1024;
            if (response.Content.Headers.ContentLength > limit) throw new MetaPublishingException("MEDIA_TOO_LARGE" + suffix, definitive: true);
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var bytes = new MemoryStream(); var buffer = new byte[16384]; int read;
            while ((read = await stream.ReadAsync(buffer, ct)) > 0)
            {
                if (bytes.Length + read > limit) throw new MetaPublishingException("MEDIA_TOO_LARGE" + suffix, definitive: true);
                bytes.Write(buffer, 0, read);
            }
            var data = bytes.ToArray();
            var jpeg = JpegDimensions(data);
            var mime = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
            if (platform == PlatformType.Instagram)
            {
                if (mime != "image/jpeg" || jpeg == null) throw new MetaPublishingException("INSTAGRAM_JPEG_REQUIRED" + suffix, definitive: true);
                var (w, h) = jpeg.Value;
                if (w is < 320 or > 1440 || h <= 0 || (double)w / h < 0.8 || (double)w / h > 1.91)
                    throw new MetaPublishingException("INSTAGRAM_IMAGE_DIMENSIONS_INVALID" + suffix, definitive: true);
                return new(w, h);
            }
            var valid = mime switch
            {
                "image/jpeg" => jpeg != null,
                "image/png" => data.Length >= 24 && data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
                "image/gif" => data.Length >= 10 && (data.AsSpan(0, 6).SequenceEqual("GIF89a"u8) || data.AsSpan(0, 6).SequenceEqual("GIF87a"u8)),
                "image/bmp" => data.Length >= 26 && data[0] == 66 && data[1] == 77,
                "image/tiff" => data.Length >= 8 && (data.AsSpan(0, 4).SequenceEqual(new byte[] { 73, 73, 42, 0 }) || data.AsSpan(0, 4).SequenceEqual(new byte[] { 77, 77, 0, 42 })),
                _ => false
            };
            if (platform == PlatformType.Zalo && mime is not ("image/jpeg" or "image/png")) valid = false;
            if (!valid) throw new MetaPublishingException((platform == PlatformType.Zalo ? "ZALO_IMAGE_FORMAT_INVALID" : "FACEBOOK_IMAGE_FORMAT_INVALID") + suffix, definitive: true);
            return new(jpeg?.Width, jpeg?.Height);
        }
        catch (HttpRequestException) { throw new MetaPublishingException("MEDIA_FETCH_FAILED" + suffix, true, true); }
        catch (OperationCanceledException) { throw new MetaPublishingException("MEDIA_FETCH_TIMEOUT" + suffix, true, true); }
    }
    private static (int Width, int Height)? JpegDimensions(byte[] bytes)
    {
        if (bytes.Length < 4 || bytes[0] != 0xff || bytes[1] != 0xd8) return null;
        var i = 2;
        while (i < bytes.Length)
        {
            if (bytes[i++] != 0xff) return null;
            while (i < bytes.Length && bytes[i] == 0xff) i++;
            if (i >= bytes.Length) return null;
            var marker = bytes[i++];
            if (marker is 0xd9 or 0xda) return null;
            if (marker == 1 || marker is >= 0xd0 and <= 0xd7) continue;
            if (i + 2 > bytes.Length) return null;
            var length = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(i, 2));
            if (length < 2 || i + length > bytes.Length) return null;
            if (marker is 0xc0 or 0xc1 or 0xc2 && length >= 8)
                return (BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(i + 5, 2)), BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(i + 3, 2)));
            i += length;
        }
        return null;
    }
}
