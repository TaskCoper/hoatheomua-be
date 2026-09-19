using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Card;

public class Request
{
    public class AdminGeneratedCardListRequest
    {
        public Guid? Id { get; set; }
        public string? FormType { get; set; }
        public string? Size { get; set; }
        public string? CardTemplateName { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
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
        private Guid? _orderDetailId;
        private Guid? _virtualId;
        private Guid? _sourceCardId;
        private Guid? _cardTemplateId;
        private string? _messageContent;

        public Guid? OrderDetailId
        {
            get => _orderDetailId;
            set { _orderDetailId = value; OrderDetailIdSpecified = true; }
        }

        public Guid? VirtualId
        {
            get => _virtualId;
            set { _virtualId = value; VirtualIdSpecified = true; }
        }

        public Guid? SourceCardId
        {
            get => _sourceCardId;
            set { _sourceCardId = value; SourceCardIdSpecified = true; }
        }

        public Guid? CardTemplateId
        {
            get => _cardTemplateId;
            set { _cardTemplateId = value; CardTemplateIdSpecified = true; }
        }

        public string? MessageContent
        {
            get => _messageContent;
            set { _messageContent = value; MessageContentSpecified = true; }
        }
        [JsonIgnore] public bool OrderDetailIdSpecified { get; private set; }
        [JsonIgnore] public bool VirtualIdSpecified { get; private set; }
        [JsonIgnore] public bool SourceCardIdSpecified { get; private set; }
        [JsonIgnore] public bool CardTemplateIdSpecified { get; private set; }
        [JsonIgnore] public bool MessageContentSpecified { get; private set; }
    }

    public class UpdateFinalGeneratedCardId
    {
        public Guid OrderDetailId { get; set; }
        public Guid VirtualId { get; set; }
        public Guid GeneratedCardId { get; set; }
    }
}
