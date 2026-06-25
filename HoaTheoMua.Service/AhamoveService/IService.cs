using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.AhamoveService;

public interface IService
{
    Task<object> EstimateShippingFee(Request.EstimateShippingFeeRequest request); 
    Task<ServiceDetailsResponse> GetServiceDetailsAhaMove();
    Task<DepotResponse> GetDepotDetails(NhanhVnRequest.DepotRequest request);
}