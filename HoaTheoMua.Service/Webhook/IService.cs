namespace HoaTheoMua.Service.Webhook;

public interface IService
{
    public Task SepayWebhookHandler(Request.SepayWebhookRequest request);

    public Task BaoKimWebHookHandler(BaoKimPaymentCallback request);
}
