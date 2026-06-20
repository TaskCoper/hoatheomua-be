using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.AhamoveService;

public interface IService
{
    Task<object> EstimateShippingFee(Request.EstimateShippingFeeRequest request); 
    Task<BasePaginationResponse> GetServiceDetailsAhaMove(int pageIndex, int pageSize);
}