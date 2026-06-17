using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.AhamoveService;

public class NhanhVnRequest
{
    public class NhanhProductListRequest
    {
        public NhanhFilters? Filters { get; set; }
        public NhanhPaginator? Paginator { get; set; }
    }

    public class NhanhFilters
    {
        [JsonPropertyName("ids")] 
        public List<int>? Id { get; set; }
        public List<int>? Status { get; set; }
    }

    public class NhanhPaginator
    {
        public int? Size { get; set; }
        public Dictionary<string, string>? Sort { get; set; }
        public Dictionary<string, int>? Next { get; set; }
    }
}