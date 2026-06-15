namespace HoaTheoMua.Service.CompanyAddress;

public interface IService
{
    Task<Response.GetCompanyAddressesResponse> GetCompanyAddresses(Request.GetCompanyAddressesRequest request);

    Task<Response.CompanyAddressResponse> GetCompanyAddressById(Guid id);
    
    Task<Response.CompanyAddressResponse> CreateCompanyAddress(Request.CreateCompanyAddressRequest request);

    Task<string> UpdateCompanyAddress(Guid id, Request.UpdateCompanyAddressRequest request);

    Task<string> DeleteCompanyAddress(Guid id);
}
