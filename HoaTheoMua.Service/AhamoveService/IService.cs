namespace HoaTheoMua.Service.AhamoveService;

public interface IService
{
    Task<object> EstimateShippingFee(Request.EstimateShippingFeeRequest request); 
}