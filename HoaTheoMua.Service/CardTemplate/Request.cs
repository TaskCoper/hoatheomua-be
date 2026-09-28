using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.CardTemplate;

public class Request
{
    public class CreateCartTemplateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? TemplateType { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateCardTemplateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }
}
