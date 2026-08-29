using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.AhamoveService;

namespace HoaTheoMua.Service.BaoKim;

public interface IService
{
    Task<BaoKimResponse<TokenResponse>> CreateAccessToken();
    Task<BaoKimResponse<CreateOrderResponse>> CreateOrder(
        string accessToken,
        Repository.Entity.Order order);

    // Throws UnauthorizedException unless `signature` is a valid BaoKim signature over rawBody.
    // The callback endpoint has to be anonymous — BaoKim cannot present a JWT — so this is the
    // only thing standing between a POST and a dispatched courier.
    void VerifyCallbackSignature(string rawBody, string? signature);
}