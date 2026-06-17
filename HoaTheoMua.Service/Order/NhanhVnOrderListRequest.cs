using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Order;

public class OrderListRequest
{
    [JsonPropertyName("filters")]
    public OrderFilter Filters { get; set; } = new OrderFilter();

    [JsonPropertyName("paginator")]
    public OrderPaginator Paginator { get; set; }

    [JsonPropertyName("dataOptions")]
    public OrderDataOptions DataOptions { get; set; } = new OrderDataOptions();
}

public class OrderFilter
{
    // Bạn có thể thêm các thuộc tính lọc như Id, Status... vào đây nếu sau này cần dùng
}

public class OrderPaginator
{
    [JsonPropertyName("size")]
    public int Size { get; set; }
}

public class OrderDataOptions
{
    // Khai báo các option mở rộng của Nhanh.vn nếu cần ở đây
}