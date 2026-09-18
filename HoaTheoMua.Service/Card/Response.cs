namespace HoaTheoMua.Service.Card;

public class Response
{
    public class CreateCardResponse
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? RawImage { get; set; }
        public string CardType { get; set; } = null!;
        public string FormType { get; set; } = null!;
        public string MessageContent { get; set; } = null!;
        public string? AttachedImageUrl { get; set; }
        public string SizeName { get; set; } = null!;
        public decimal SizeWidth { get; set; }
        public decimal SizeHeight { get; set; }
        public decimal SizeBasePrice { get; set; }
        public int SizeMaxWords { get; set; }
        public int WordCount { get; set; }
        public WordConfigSnapshot? WordConfigSnapshot { get; set; }
        public decimal BasePrice { get; set; }
        public decimal ExtraPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
    
    public class WordConfigSnapshot
    {
        [System.Text.Json.Serialization.JsonPropertyName("min_words")]
        public int MinWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("max_words")]
        public int MaxWords { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("extra_price")]
        public decimal ExtraPrice { get; set; }
    }
}
