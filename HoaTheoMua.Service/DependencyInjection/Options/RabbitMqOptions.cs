namespace HoaTheoMua.Service.DependencyInjection.Options;

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}