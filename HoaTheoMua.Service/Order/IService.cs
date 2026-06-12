using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Order;

public interface IService
{
    Task<Response.CreateOrderResponse> ProcessOrderTransport(Request.CreateOrderRequest request);
    Task<string> CheckingOrder(Guid orderId);
    Task<BasePaginationResponse> GetOrderPaginationResponse(int pageIndex, int pageSize);
}