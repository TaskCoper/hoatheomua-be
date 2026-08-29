using HoaTheoMua.Service.BaoKim;
using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Order;

public interface IService
{
    // Task<Response.CreateOrderResponse> CreateOrderTransport(Request.CreateOrderRequest request);
    // Task<string> CheckingOrder(Guid orderId);
    // Task<string> UpdateOrder(Request.UpdateOrderRequest request);
    
    //--------------------
    Task<BasePaginationResponse> GetOrderPaginationResponse(int pageIndex, int pageSize);
    Task<CreateOrderResponse> CreateOrder(Request.CreateOrderRequest request);
    Task<Response.EstimateXanhSmShippingFeeResponse> EstimateXanhSmShippingFee(
        Request.EstimateXanhSmShippingFeeRequest request,
        CancellationToken cancellationToken = default);
    // Task<CreateOrderResponse> CreateDefaultOrder();
    Task<BaseResponse> GetOrderById (Guid id);
    Task<string> GetStatusOrder(Guid id);
    Task<OrderListRootResponse> GetOrderNhanhVn(NhanhVnOrderListRequest.OrderListRequest request);
    Task<Response.NhanhVnLocationResponse> GetShippingLocation(Request.LocationRequest filter);
    Task<string> GetInfoTaxCode(string taxCode);
    Task<string> CancelOrder(Guid id);
    Task<string> UpdateAhaMoveService(Request.UpdateAhamoveRequest request);
    Task<BasePaginationResponse> GetOrderByAdmin(string? search, int  pageIndex, int pageSize);
}
