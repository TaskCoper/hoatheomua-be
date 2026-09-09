using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class GeneratedCard
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? RawImage { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public GeneratedCardType CardType { get; set; }

    public CardFormType FormType { get; set; }
    public string SizeKey { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string MessageContent { get; set; } = null!;
    public string? AttachedImageUrl { get; set; }

    public string SizeName { get; set; } = null!;
    public decimal SizeWidth { get; set; }
    public decimal SizeHeight { get; set; }
    public decimal SizeBasePrice { get; set; }
    public int SizeMaxWords { get; set; }
    public int WordCount { get; set; }
    public string? WordConfigSnapshot { get; set; }

    public decimal BasePrice { get; set; }
    public decimal ExtraPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
