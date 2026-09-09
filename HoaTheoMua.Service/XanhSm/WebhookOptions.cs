namespace HoaTheoMua.Service.XanhSm;

public class XanhSmWebhookOptions
{
    public string SigningKey { get; set; } = string.Empty;
    public string? SecondarySigningKey { get; set; }
    public string Environment { get; set; } = string.Empty;
}
