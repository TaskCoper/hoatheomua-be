using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.CategoryProduct;

public interface IService
{
    Task<Response.CreateCategoryProductResponse> CreateCateProduct(Request.CreateCategoryProductRequest request);
    Task<BasePaginationResponse> GetCateProducts(int pageIndex, int pageSize, string? keyword);
    Task<Response.CategoryProductResponse> GetCateProduct(Guid categoryId);
    Task<Response.CategoryProductResponse> UpdateCateProduct(Guid categoryId, Request.UpdateCategoryProductRequest request);
    Task AssignProductsToCategory(Guid categoryId, Request.AssignProductsToCategoryRequest request);
    Task DeleteCateProduct(Guid categoryId);
    Task<List<Response.CategoryWithChildrenResponse>> GetCategoryWithChild();
}
