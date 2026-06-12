using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.CategoryNew;

public interface IService
{
    Task<Response.CreateCategoryResponse> CreateCate(Request.CreateCategoriesRequest request);
    Task<BasePaginationResponse> GetCategories(int pageIndex, int pageSize);
    Task<Response.CategoryResponse> GetCategory(Guid categoryId);
    Task<Response.CategoryResponse> UpdateCate(Guid categoryId, Request.UpdateCategoriesRequest request);
    Task DeleteCate(Guid categoryId);
}