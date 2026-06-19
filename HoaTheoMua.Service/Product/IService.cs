using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Product;

public interface IService
{
    Task<BasePaginationResponse> GetListProducts(Request.ProductListRequest request, int  pageIndex, int pageSize);
    Task<object> GetProductDetails(Request.ProductDetailRequest request);
}