using HoaTheoMua.Service.User;

namespace HoaTheoMua.Service.NhanhVnService;

public interface IService
{
    Task<OrderLabelResponse> GetOrderLabel(OrderLabelRequest orderLabelRequest);
    Task<OrderSourceResponse>  GetOrderSource(OrderSourceRequest orderSourceRequest);
    Task<string> WebhookNhanhVnInventory(NhanhWebhookInventoryRequest request);
}