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


//
public class NhanhWebhookInventoryRequest
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("businessId")]
        public long BusinessId { get; set; }

        // Map trực tiếp trường data thành List các sản phẩm thay đổi tồn kho
        [JsonPropertyName("data")]
        public List<NhanhInventoryItem> Data { get; set; } = new();
    }

    // Chi tiết từng sản phẩm trong mảng data
    public class NhanhInventoryItem
    {
        [JsonPropertyName("id")]
        public long Id { get; set; } // ID sản phẩm trên Nhanh.vn

        [JsonPropertyName("appProductId")]
        public string AppProductId { get; set; } = string.Empty;

        [JsonPropertyName("typeId")]
        public int TypeId { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty; // Mã sản phẩm (SKU)

        [JsonPropertyName("remain")]
        public double Remain { get; set; } // Tổng tồn kho thực tế

        [JsonPropertyName("shipping")]
        public double Shipping { get; set; }

        [JsonPropertyName("damaged")]
        public double Damaged { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }

        [JsonPropertyName("transfering")]
        public double Transfering { get; set; }

        [JsonPropertyName("available")]
        public double Available { get; set; } // Số lượng có thể bán (Quan trọng nhất)

        [JsonPropertyName("warranty")]
        public NhanhWarranty Warranty { get; set; } = new();

        [JsonPropertyName("depots")]
        public List<NhanhDepot> Depots { get; set; } = new();
    }

    // Thông tin bảo hành
    public class NhanhWarranty
    {
        [JsonPropertyName("remain")]
        public double Remain { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }
    }

    // Chi tiết tồn kho theo từng Kho hàng (Depot)
    public class NhanhDepot
    {
        [JsonPropertyName("id")]
        public long Id { get; set; } // ID của kho hàng

        [JsonPropertyName("remain")]
        public double Remain { get; set; }

        [JsonPropertyName("shipping")]
        public double Shipping { get; set; }

        [JsonPropertyName("damaged")]
        public double Damaged { get; set; }

        [JsonPropertyName("holding")]
        public double Holding { get; set; }

        [JsonPropertyName("transfering")]
        public double Transfering { get; set; }

        [JsonPropertyName("available")]
        public double Available { get; set; }

        [JsonPropertyName("warranty")]
        public NhanhWarranty Warranty { get; set; } = new();
    }