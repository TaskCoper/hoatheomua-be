using System.Net;
using System.Net.Sockets;

namespace HoaTheoMua.Service.Flower;

// AI output URLs are untrusted. Pin the connection to a checked public IP, including after DNS lookup.
public static class PublicImageHttpHandler
{
    public static SocketsHttpHandler Create() => new()
    {
        AllowAutoRedirect = false,
        UseProxy = false,
        UseCookies = false,
        ConnectCallback = async (context, cancellationToken) =>
        {
            if (context.DnsEndPoint.Port is not (80 or 443))
                throw new HttpRequestException("Unsupported image port");
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
            if (addresses.Length == 0 || addresses.Any(ip => !IsPublicAddress(ip)))
                throw new HttpRequestException("Image host must resolve to public addresses");
            foreach (var address in addresses)
            {
                var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch (SocketException) { socket.Dispose(); }
                catch { socket.Dispose(); throw; }
            }
            throw new HttpRequestException("Could not connect to image host");
        }
    };

    public static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        if (IPAddress.IsLoopback(address)) return false;
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetworkV6)
            return (bytes[0] & 0xe0) == 0x20 // global unicast only, excluding documentation/transition ranges
                && !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0d && bytes[3] == 0xb8)
                && !(bytes[0] == 0x20 && bytes[1] == 0x02)
                && !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0 && bytes[3] == 0);
        return bytes[0] is not (0 or 10 or 127) && bytes[0] < 224
            && !(bytes[0] == 100 && bytes[1] is >= 64 and <= 127)
            && !(bytes[0] == 169 && bytes[1] == 254)
            && !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31)
            && !(bytes[0] == 192 && (bytes[1] == 168 || bytes[1] == 0 || bytes[1] == 2))
            && !(bytes[0] == 198 && bytes[1] is 18 or 19)
            && !(bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100)
            && !(bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113);
    }
}
