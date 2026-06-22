using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Order;

public class NhanhVnOrderListRequest
{
    public class OrderListRequest
    {
        [JsonPropertyName("filters")]
        public OrderFilter Filters { get; set; } = new OrderFilter();

        [JsonPropertyName("paginator")]
        public OrderPaginator? Paginator { get; set; }

        [JsonPropertyName("dataOptions")]
        public OrderDataOptions DataOptions { get; set; } = new OrderDataOptions();
    }

    public class OrderFilter
    {
        [JsonPropertyName("createdAtFrom")]
        public long? CreatedAtFrom { get; set; } = null;

        [JsonPropertyName("createdAtTo")]
        public long? CreatedAtTo { get; set; } = null;

        [JsonPropertyName("ids")]
        public List<long>? Ids { get; set; } = null;

        [JsonPropertyName("depotIds")]
        public List<long>? DepotIds { get; set; } = null;
    }

    public class OrderPaginator
    {
        [JsonPropertyName("size")]
        public int? Size { get; set; }

        // Thêm thuộc tính next trỏ đến class NextPage bên dưới
        [JsonPropertyName("next")]
        public NextPage? Next { get; set; }
    }

    // Tạo thêm class này để hứng object {"id": "xxx"}
    public class NextPage
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }
    }

    public class OrderDataOptions
    {
        // Khai báo các option mở rộng của Nhanh.vn nếu cần ở đây
    }
}