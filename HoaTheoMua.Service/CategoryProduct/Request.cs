using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.CategoryProduct;

public class Request
{
    public class CreateCategoryProductRequest
    {
        public required string name { get; set; }
        public DateTimeOffset? startTime { get; set; }
        public DateTimeOffset? endTime { get; set; }
        public required CategoryProductType type { get; set; }
        public bool isActive { get; set; } = true;
        public Guid? parentId { get; set; }
    }

    public class UpdateCategoryProductRequest
    {
        public required string name { get; set; }
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
}
