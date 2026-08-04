namespace HoaTheoMua.Service.AhamoveService;

public interface IService
{
    Task<object> EstimateShippingFee(AhaMoveRequest.EstimateShippingFeeRequest request); 
    Task<ServiceDetailsResponse> GetServiceDetailsAhaMove();
    Task<DepotResponse> GetDepotDetails(NhanhVnRequest.DepotRequest request);
    Task<List<AhaMoveGetListResponse>> GetServiceList();
}