namespace HoaTheoMua.Service.AhamoveService;

public interface IService
{
    Task<decimal> EstimateFee(Request.EstimateFeeRequest request);
    Task<string> CreateOrder(Request.CreateOrderRequestAhamove request);
}