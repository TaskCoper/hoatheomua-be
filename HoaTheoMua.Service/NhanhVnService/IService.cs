using System.Text.Json;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.User;

namespace HoaTheoMua.Service.NhanhVnService;

public interface IService
{
    Task<ProductLabelResponse> GetProductLabel(ProductLabelRequest request);
    Task<OrderLabelResponse> GetOrderLabel(OrderLabelRequest orderLabelRequest);
    Task<OrderSourceResponse>  GetOrderSource(OrderSourceRequest orderSourceRequest);

    Task<OrderNhanhVnDto> GetOrderNhanhVnById(Guid id);
    Task<BasePaginationResponse> GetOrderNhanhVnPagination(int pageIndex, int pageSize, string? search);

    Task<string> HandleWebhook(JsonElement? body);
    Task<string> WebhookNhanhVnInventory(NhanhInventoryChangeWebhookRequest request);
    Task<string> WebhookNhanhVnProductAdd(NhanhProductAddWebhookRequest request);
    Task<string> WebhookNhanhVnOrderAdd(NhanhOrderAddWebhookRequest request);
    Task<string> WebhookNhanhVnOrderUpdate(NhanhOrderUpdateWebhookRequest request);
    Task<string> WebhookNhanhVnOrderDelete(NhanhOrderDeleteWebhookRequest request);
   Task<string> WebhookNhanhVnProductUpdate(NhanhProductAddWebhookRequest request);
    Task<string> WebhookNhanhVnProductDelete(NhanhProductDeleteWebhookRequest request);
}