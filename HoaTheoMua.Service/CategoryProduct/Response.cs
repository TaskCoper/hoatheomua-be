using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.CategoryProduct;

public class Response
{
    public class CreateCategoryProductResponse
    {
        public required Guid categoryId { get; set; }
        public required string name { get; set; }
        public string? name_En { get; set; }
        public string? description { get; set; }
        public string? description_En { get; set; }
        public double? position { get; set; }
        public string? metaData { get; set; }
        public DateTimeOffset? startTime { get; set; }
        public DateTimeOffset? endTime { get; set; }
        public required CategoryProductType type { get; set; }
        public required bool isActive { get; set; }
        public Guid? parentId { get; set; }
        public required Guid userId { get; set; }
    }

    public class CategoryProductResponse
    {
        public required Guid categoryId { get; set; }
        public required string name { get; set; }
        public string? name_En { get; set; }
        public string? description { get; set; }
        public string? description_En { get; set; }
        public double? position { get; set; }
        public string? metaData { get; set; }
        public DateTimeOffset? startTime { get; set; }
        public DateTimeOffset? endTime { get; set; }
        public required CategoryProductType type { get; set; }
        public required bool isActive { get; set; }
        public required Guid userId { get; set; }
        public Guid? parentId { get; set; }
        public CategoryProductResponse? Parent { get; set; }
        public List<CategoryProductResponse>? children { get; set; } = new List<CategoryProductResponse>();
        public int? productCount { get; set; }
        public List<Guid>? productIds { get; set; }
    }

    public class CategoryWithChildrenResponse
    {
        public Guid CategoryId { get; set; }
        public required string Name { get; set; }
        public string? NameEn { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public double? Position { get; set; }
        public string? MetaData { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public required CategoryProductType Type { get; set; }
        public required Guid UserId { get; set; }
        public Guid? ParentId { get; set; }
        
        public List<CategoryWithChildrenResponse>? Children { get; set; } = new List<CategoryWithChildrenResponse>();
    }
}
