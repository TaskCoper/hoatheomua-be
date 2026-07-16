using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.AhamoveService;

namespace HoaTheoMua.Service.BaoKim;

public interface IService
{
    Task<BaoKimResponse<TokenResponse>> CreateAccessToken();
    Task<BaoKimResponse<CreateOrderResponse>> CreateOrder(
        string accessToken,
        Repository.Entity.Order order,
        IEnumerable<OrderDetails> orderDetails);
}