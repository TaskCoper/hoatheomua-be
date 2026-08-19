using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Product;

public interface IService
{
    Task<NhanhVnProductListResponse.NhanhVnProductResponse> GetListProducts(Request.ProductListRequest request);
    Task<NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse> GetProductDetails(Request.ProductDetailRequest request);
    Task<BasePaginationResponse> GetLocalProducts(Request.LocalProductQueryRequest request);
    Task<BasePaginationResponse> GetLocalProductsV2(Request.LocalProductQueryRequest request);
    Task<Response.LocalProductDetailResponse> GetLocalProductDetail(Guid id);
    Task<decimal> GetMaxPrice(List<Guid>? categoryIds);
    Task<BaseResponse> MoveProduct(Guid id, Request.MoveProductRequest request);
    Task<BaseResponse> PinProduct(Request.PinProductRequest request);
    Task<List<Response.PinnedProductResponse>> GetProductPin(Request.GetProductPinRequest request);
    Task<string> UpdateProduct(Guid id, Request.UpdateProductRequest request);
    Task<List<Response.ProductResponseWithList>> GetProductWithList(Request.GetProductWithList request);
}