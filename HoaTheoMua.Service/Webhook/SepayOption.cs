namespace HoaTheoMua.Service.Webhook;

public class SepayOption
{
    // SePay sends "Authorization: Apikey <ApiKey>" on every webhook call. Empty means inbound
    // webhooks cannot be authenticated, and the handler fails closed rather than fulfilling orders
    // on unverified input.
    public string ApiKey { get; set; } = null!;
}
