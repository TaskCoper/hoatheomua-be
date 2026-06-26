using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Product;

public interface IService
{
    Task<ResponseProductList.NhanhVnProductResponse> GetListProducts(Request.ProductListRequest request);
    Task<ResponseProductDetails.NhanhVnProductDetailResponse> GetProductDetails(Request.ProductDetailRequest request);
}