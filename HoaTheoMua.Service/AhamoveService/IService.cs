using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.AhaMoveService;

public interface IService
{
    Task<object> EstimateShippingFee(AhaMoveRequest.EstimateShippingFeeRequest request); 
    Task<ServiceDetailsResponse> GetServiceDetailsAhaMove();
    Task<DepotResponse> GetDepotDetails(NhanhVnRequest.DepotRequest request);
}