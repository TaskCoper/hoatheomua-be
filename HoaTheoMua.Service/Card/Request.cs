using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Card;

public class Request
{
    public class AdminGeneratedCardListRequest
    {
        public Guid? id { get; set; }
        public string? formType { get; set; }
        public string? size { get; set; }
        public string? cardTemplateName { get; set; }
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }

    public class CustomerGeneratedCardListRequest
    {
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class CreateCardRequest
    {
        public Guid? ProductId { get; set; }
        public Guid? GeneratedFlowerId { get; set; }
        public Guid CardTemplateId { get; set; }
        public required string SizeName { get; set; }
        public required string MessageContent { get; set; }
    }

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class CreateCardAiRequest
    {
        public Guid? OrderDetailId { get; set; }
        public Guid? VirtualId { get; set; }
        public Guid? SourceCardId { get; set; }
        public Guid? CardTemplateId { get; set; }
        public string? MessageContent { get; set; }
    }

    public class UpdateFinalGeneratedCardId
    {
        public Guid OrderDetailId { get; set; }
        public Guid VirtualId { get; set; }
        public Guid GeneratedCardId { get; set; }
    }

    public class GetGeneratedCardsByProductIdsRequest
    {
        public required List<Guid> ProductIds { get; set; } = [];
    }
}
