using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Service.CardTemplate;

public class Response
{
    public class GetCardTemplateResponse
    {
        public List<CardTemplateResponse> CardTemplates { get; set; } = [];

        public List<ProductGeneratedCardResponse> Products { get; set; } = [];

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }
    }

    public class CardTemplateResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string TemplateType { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class ProductGeneratedCardResponse
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public List<GeneratedCardResponse> GeneratedCards { get; set; } = [];
    }

    public class GeneratedCardResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
    }
}
