using HoaTheoMua.Repository.Abtraction;
using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class History : BaseEntity<Guid>, IAuditableEntity
{
    public required string Input { get; set; }
    public required ConfigType Type { get; set; }
    public required string Metadata { get; set; }
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public Guid PostCoreBaseId { get; set; }
    public PostCoreBase PostCoreBase { get; set; }
    public Guid TemplateCardBaseId { get; set; }
    public TemplateCardBase TemplateCardBase { get; set; }
    public Guid SystemPromptId { get; set; }
    public SystemPrompt SystemPrompt { get; set; } = null!;
    public Guid PostOutId { get; set; }
    public PostOut PostOut { get; set; }
    public Guid FlowerOutId { get; set; }
    public FlowerOut FlowerOut { get; set; }
    public Guid CardOutId { get; set; }
    public CardOut CardOut { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
