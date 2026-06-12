namespace HoaTheoMua.Service.CategoryNew;

public class Request
{
    public class CreateCategoriesRequest
    {
        public required string title { get; set; }
        public Guid? parentId { get; set; }
    }

    public class UpdateCategoriesRequest
    {
        public required string title { get; set; }
        public Guid? parentId { get; set; }
    }
}