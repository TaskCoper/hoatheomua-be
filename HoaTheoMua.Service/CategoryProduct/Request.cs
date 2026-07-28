using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.CategoryProduct;

public class Request
{
    public class CreateCategoryProductRequest
    {
        public required string name { get; set; }
        public required string name_En { get; set; }
        public string? description { get; set; }
        public string? description_En { get; set; }
        public double? position { get; set; }
        public string? metaData { get; set; }
        public List<string>? page { get; set; }
        public DateTimeOffset? startTime { get; set; }
        public DateTimeOffset? endTime { get; set; }
        public required CategoryProductType type { get; set; }
        public bool isActive { get; set; } = true;
        public Guid? parentId { get; set; }
    }

    public class UpdateCategoryProductRequest
    {
        public required string name { get; set; }
        public required string name_En { get; set; }
        public string? description { get; set; }
        public string? description_En { get; set; }
        public double? position { get; set; }
        public string? metaData { get; set; }
        public List<string>? page { get; set; }
        public DateTimeOffset? startTime { get; set; }
        public DateTimeOffset? endTime { get; set; }
        public required CategoryProductType type { get; set; }
        public bool isActive { get; set; }
        public Guid? parentId { get; set; }
    }

    public class AssignProductsToCategoryRequest
    {
        public required List<Guid> productIds { get; set; }
    }

    public class GetCategoryProductWithChild
    {
        public List<string>? page { get; set; }
    }

    public class CategoryProductDetailRequest
    {
        public Guid ProductId { get; set; }
        public Guid CategoryProductId { get; set; }
        public bool IsPin { get; set; }
        public double Position { get; set; }
    }
}
