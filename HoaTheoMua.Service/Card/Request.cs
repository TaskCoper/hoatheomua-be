namespace HoaTheoMua.Service.Card;

public class Request
{
    public class CreateCardRequest
    {
        public Guid? ProductId { get; set; }
        public Guid? GeneratedFlowerId { get; set; }
        public Guid CardTemplateId { get; set; }
        public required string SizeName { get; set; }
        public required string MessageContent { get; set; }
    }

    public class CreateCardAiRequest
    {
        public Guid? OrderDetailId { get; set; }
        public Guid? SourceCardId { get; set; }
        public Guid? CardTemplateId { get; set; }
        public string? MessageContent { get; set; }
    }
}
