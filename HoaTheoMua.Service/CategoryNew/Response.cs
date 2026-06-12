using HoaTheoMua.Repository.Entity;

namespace HoaTheoMua.Service.CategoryNew;

public class Response
{
    public class CreateCategoryResponse
    {
        public required Guid categoryId { get; set; }
        public required string title { get; set; }
        public Guid? parentId { get; set; }
        public required Guid userId { get; set; }
    }
    
    public class CategoryResponse
    {
        public required Guid categoryId { get; set; }
        public required string title { get; set; }
        public required Guid userId { get; set; }
        public Guid? parentId { get; set; }
        public CategoryResponse? Parent { get; set; }
        public List<CategoryResponse>? children { get; set; } = new List<CategoryResponse>();
        public int? count { get; set; }
    }

}