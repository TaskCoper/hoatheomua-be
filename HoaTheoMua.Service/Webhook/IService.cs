namespace HoaTheoMua.Service.Webhook;

public interface IService
{
    public void VerifySepayApiKey(string? authorizationHeader);

    public Task SepayWebhookHandler(Request.SepayWebhookRequest request);

    public Task BaoKimWebHookHandler(BaoKimPaymentCallback request);
}
