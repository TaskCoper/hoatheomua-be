using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Product;

public interface IService
{
    Task<NhanhVnProductListResponse.NhanhVnProductResponse> GetListProducts(Request.ProductListRequest request);
    Task<NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse> GetProductDetails(Request.ProductDetailRequest request);
    Task<BasePaginationResponse> GetLocalProducts(Request.LocalProductQueryRequest request);
    Task<Response.ProductDetailResponse> GetLocalProductDetail(Guid id);
    Task<decimal> GetMaxPrice(Guid? categoryId = null);
    Task<BaseResponse> MoveProduct(Guid id, Request.MoveProductRequest request);
    Task<BaseResponse> PinProduct(Request.PinProductRequest request);
    Task<List<Response.PinnedProductResponse>> GetProductPin();
}