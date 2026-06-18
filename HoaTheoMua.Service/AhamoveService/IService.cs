using HoaTheoMua.Service.Webhook;

namespace HoaTheoMua.Service.AhamoveService;

public interface IService
{
    Task<object> EstimateShippingFee(Request.EstimateShippingFeeRequest request);
    Task<AhaMoveResponse.ServiceDetailsResponse> GetServiceDetailsAhaMove();
}