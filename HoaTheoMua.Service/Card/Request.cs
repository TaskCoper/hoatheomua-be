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
        private Guid? _orderDetailId, _virtualId, _cardTemplateId;
        private string? _messageContent;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? OrderDetailId { get => _orderDetailId; set { _orderDetailId = value; HasOrderDetailId = true; } }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? VirtualId { get => _virtualId; set { _virtualId = value; HasVirtualId = true; } }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? SourceCardId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guid? CardTemplateId { get => _cardTemplateId; set { _cardTemplateId = value; HasCardTemplateId = true; } }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MessageContent { get => _messageContent; set { _messageContent = value; HasMessageContent = true; } }
        [JsonIgnore] public bool HasOrderDetailId { get; private set; }
        [JsonIgnore] public bool HasVirtualId { get; private set; }
        [JsonIgnore] public bool HasCardTemplateId { get; private set; }
        [JsonIgnore] public bool HasMessageContent { get; private set; }
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
