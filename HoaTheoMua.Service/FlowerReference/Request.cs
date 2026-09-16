namespace HoaTheoMua.Service.FlowerReference;

public class Request
{
    public class GetFlowerReferenceRequest
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool? IsActive { get; set; }
        public string? Type { get; set; }
        public string? Search { get; set; }
    }

    public class CreateFlowerReferenceRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Type { get; set; }
    }
}
