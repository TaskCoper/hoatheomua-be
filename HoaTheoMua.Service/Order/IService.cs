using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Order;

public interface IService
{
    // Task<Response.CreateOrderResponse> CreateOrderTransport(Request.CreateOrderRequest request);
    // Task<string> CheckingOrder(Guid orderId);
    // Task<string> UpdateOrder(Request.UpdateOrderRequest request);
    
    //--------------------
    Task<BasePaginationResponse> GetOrderPaginationResponse(int pageIndex, int pageSize);
    Task<string> CreateOrder(Request.CreateOrderRequest request);
    Task<BaseResponse> GetOrderById (Guid id);
    Task<string> GetStatusOrder(Guid id);
    
    
    Task<object?> GetOrderNhanhVn(int pageIndex, int pageSize);
}