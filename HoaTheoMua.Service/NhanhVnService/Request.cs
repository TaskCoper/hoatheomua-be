using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.NhanhVnService;

public class OrderLabelRequest
{
    [JsonPropertyName("filters")]
    public LabelFilters? Filters { get; set; }

    [JsonPropertyName("paginator")]
    public Paginator? Paginator { get; set; }
}

public class LabelFilters
{
    [JsonPropertyName("ids")]
    public List<int>? Ids { get; set; } 

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class Paginator
{
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    // Thêm trường sort (có thể là object hoặc dictionary tùy theo API cụ thể yêu cầu)
    [JsonPropertyName("sort")]
    public object? Sort { get; set; }

    // THÊM TRƯỜNG NEXT VÀO ĐÂY ĐỂ PHÂN TRANG
    [JsonPropertyName("next")]
    public RequestNextPage? Next { get; set; }
}

public class RequestNextPage
{
    // Dựa theo raw data, id của trang kế tiếp là kiểu int (Ví dụ: 262)
    [JsonPropertyName("id")]
    public int? Id { get; set; }
}

public class OrderSourceRequest
{
    [JsonPropertyName("filters")]
    public OrderSourceFilters? Filters { get; set; }

    [JsonPropertyName("paginator")]
    public OrderSourcePaginator? Paginator { get; set; }
}

public class OrderSourceFilters
{
    // Mảng ID nguồn đơn hàng, tối đa 100 phần tử
    [JsonPropertyName("ids")]
    public List<int>? Ids { get; set; } 
}

public class OrderSourcePaginator
{
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    // Dùng object? để linh hoạt truyền dạng mảng [] hoặc object {} theo tài liệu Nhanh.vn
    [JsonPropertyName("sort")]
    public object? Sort { get; set; }

    [JsonPropertyName("next")]
    public OrderSourceNextPage? Next { get; set; }
}

public class OrderSourceNextPage
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }
}