using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.NhanhVnService;

public class OrderLabelResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("paginator")]
    public ResponsePaginator Paginator { get; set; } = null!;

    [JsonPropertyName("data")]
    public List<LabelData> Data { get; set; } = null!;
}

public class ResponsePaginator
{
    [JsonPropertyName("next")]
    public NextPage Next { get; set; } = null!;
}

public class NextPage
{
    // SỬA TỪ string THÀNH int VÌ THỰC TẾ TRẢ VỀ SỐ (Ví dụ: 262)
    [JsonPropertyName("id")]
    public int Id { get; set; } 
}

public class LabelData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("businessId")]
    public int BusinessId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    // SỬA THÀNH string? VÌ DỮ LIỆU THỰC TẾ NHIỀU ITEM KHÔNG CÓ bgColor
    [JsonPropertyName("bgColor")]
    public string? BgColor { get; set; }

    // SỬA THÀNH string? ĐỂ PHÒNG TRƯỜNG HỢP CÓ ITEM THIẾU CẢ textColor
    [JsonPropertyName("textColor")]
    public string? TextColor { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    // Tiện ích: Tự động decode unicode (như \u1ea3) C# xử lý tự động, không lo lắng
    [JsonIgnore]
    public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeSeconds(CreatedAt).LocalDateTime;
}

public class OrderSourceResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("paginator")]
    public SourcePaginator Paginator { get; set; } = null!;
 
    [JsonPropertyName("data")]
    public List<OrderSourceData> Data { get; set; } = null!;
}

public class SourcePaginator
{
    // Dùng dấu ? đề phòng trường hợp trang cuối cùng không có phần tử 'next' (next bị null)
    [JsonPropertyName("next")]
    public SourceNextPage? Next { get; set; }
}

public class SourceNextPage
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class OrderSourceData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    // Trạng thái: 1 = Active, 2 = Inactive
    [JsonPropertyName("status")]
    public int Status { get; set; }

    // Thời gian tạo - định dạng định dạng unix timestamp
    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    // Tiện ích: Tự động quy đổi timestamp thành định dạng DateTime trong C# để tiện xử lý logic
    [JsonIgnore]
    public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeSeconds(CreatedAt).LocalDateTime;
}