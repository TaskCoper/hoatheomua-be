namespace HoaTheoMua.Service.Order;

public interface IService
{
    Task<Response.CreateOrderResponse> ProcessOrderTransport(Request.CreateOrderRequest request);
}