namespace HoaTheoMua.Service.Card;

public class Response
{
    public class CreateCardResponse
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public object? RawImage { get; set; }
        public string CardType { get; set; } = null!;
        public string FormType { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public string? AttachedImageUrl { get; set; }
        public string SizeName { get; set; } = null!;
        public int SizeWidth { get; set; }
        public int SizeHeight { get; set; }
        public int SizeBasePrice { get; set; }
        public int SizeMaxWords { get; set; }
        public int WordCount { get; set; }
        public WordConfigSnapshot WordConfigSnapshot { get; set; } = null!;
        public int BasePrice { get; set; }
        public int ExtraPrice { get; set; }
        public int TotalPrice { get; set; }
    }
    
    public class WordConfigSnapshot
    {
        public int MinWords { get; set; }
        public int MaxWords { get; set; }
        public int ExtraPrice { get; set; }
    }
}