using HoaTheoMua.Service.User;

namespace HoaTheoMua.Service.NhanhVnService;

public interface IService
{
    Task<OrderLabelResponse> GetOrderLabel(OrderLabelRequest orderLabelRequest);
    Task<OrderSourceResponse>  GetOrderSource(OrderSourceRequest orderSourceRequest);
    Task<string> WebhookNhanhVnInventory(NhanhInventoryChangeWebhookRequest request);
    Task<string> WebhookNhanhVnProductAdd(NhanhProductAddWebhookRequest request);
    Task<string> WebhookNhanhVnOrderAdd(NhanhOrderAddWebhookRequest request);
    Task<string> WebhookNhanhVnOrderUpdate(NhanhOrderAddWebhookRequest request);
    Task<string> WebhookNhanhVnOrderDelete(NhanhOrderDeleteWebhookRequest request);
}