namespace HoaTheoMua.Service.FlowerReference;

public class Request
{
    public class GetFlowerReferenceRequest
    {
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
        public bool? isActive { get; set; }
        public string? type { get; set; }
        public string? search { get; set; }
    }

    public class CreateFlowerReferenceRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Type { get; set; }
    }
}
