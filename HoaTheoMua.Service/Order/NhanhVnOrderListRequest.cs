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
        // Thêm: Thời gian tạo từ ngày (định dạng timestamp)
        [JsonPropertyName("createdAtFrom")]
        public long? CreatedAtFrom { get; set; } = null;

        // Thêm: Thời gian tạo đến ngày (định dạng timestamp)
        [JsonPropertyName("createdAtTo")]
        public long? CreatedAtTo { get; set; } = null;

        // Thêm: Mảng ID đơn hàng
        [JsonPropertyName("ids")]
        public List<long>? Ids { get; set; } = null;

        // Thêm: Mảng ID kho hàng
        [JsonPropertyName("depotIds")]
        public List<long>? DepotIds { get; set; } = null;
    }

    public class OrderPaginator
    {
        [JsonPropertyName("size")]
        public int? Size { get; set; }
    }

    public class OrderDataOptions
    {
        // Khai báo các option mở rộng của Nhanh.vn nếu cần ở đây
    }
}