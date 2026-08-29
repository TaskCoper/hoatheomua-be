using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Product;

public class NhanhVnProductDataConverter : JsonConverter<List<NhanhVnProductDetailsResponse.ProductDetailData>>
{
    public override List<NhanhVnProductDetailsResponse.ProductDetailData> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<NhanhVnProductDetailsResponse.ProductDetailData>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return list;

                // Deserialize từng phần tử OrderData
                var item = JsonSerializer.Deserialize<NhanhVnProductDetailsResponse.ProductDetailData>(ref reader, options);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        // Trường hợp 2: Nhanh.vn trả về một Object rỗng { } thay vì Array, hoặc null, hoặc dạng lỗi khác
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Đọc hết object lỗi đó để dịch chuyển con trỏ reader qua phân đoạn này
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                // Log cảnh báo ở đây nếu cần thiết
            }
            // Trả về danh sách rỗng thay vì để sập hệ thống
            return new List<NhanhVnProductDetailsResponse.ProductDetailData>();
        }

        // Các trường hợp Token khác (gặp Null, String trống,...)
        return new List<NhanhVnProductDetailsResponse.ProductDetailData>();
    }

    public override void Write(Utf8JsonWriter writer, List<NhanhVnProductDetailsResponse.ProductDetailData> value, JsonSerializerOptions options)
    {
        // Ghi đè phương thức serialize khi cần trả data ngược lại
        JsonSerializer.Serialize(writer, value, options);
    }

}


public class NhanhVnDataListConverter : JsonConverter<List<NhanhVnProductListResponse.ProductDetailData>>
{
    public override List<NhanhVnProductListResponse.ProductDetailData> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Trường hợp 1: Dữ liệu là một Array chuẩn [ { ... }, { ... } ]
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<NhanhVnProductListResponse.ProductDetailData>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return list;

                // Deserialize từng phần tử OrderData
                var item = JsonSerializer.Deserialize<NhanhVnProductListResponse.ProductDetailData>(ref reader, options);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        // Trường hợp 2: Nhanh.vn trả về một Object rỗng { } thay vì Array, hoặc null, hoặc dạng lỗi khác
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Đọc hết object lỗi đó để dịch chuyển con trỏ reader qua phân đoạn này
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                // Log cảnh báo ở đây nếu cần thiết
            }
            // Trả về danh sách rỗng thay vì để sập hệ thống
            return new List<NhanhVnProductListResponse.ProductDetailData>();
        }

        // Các trường hợp Token khác (gặp Null, String trống,...)
        return new List<NhanhVnProductListResponse.ProductDetailData>();
    }

    public override void Write(Utf8JsonWriter writer, List<NhanhVnProductListResponse.ProductDetailData> value, JsonSerializerOptions options)
    {
        // Ghi đè phương thức serialize khi cần trả data ngược lại
        JsonSerializer.Serialize(writer, value, options);
    }
    public class NhanhVnUnitsListConverter : JsonConverter<Dictionary<string, List<NhanhVnProductDetailsResponse.UnitDetail>>>
    {
        public override Dictionary<string, List<NhanhVnProductDetailsResponse.UnitDetail>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Nếu API trả về mảng rỗng []
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Skip(); 
                return new Dictionary<string, List<NhanhVnProductDetailsResponse.UnitDetail>>(); 
            }

            // Nếu API trả về dạng Object chuẩn { "id": [...] }
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return JsonSerializer.Deserialize<Dictionary<string, List<NhanhVnProductDetailsResponse.UnitDetail>>>(ref reader, options);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, List<NhanhVnProductDetailsResponse.UnitDetail>> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}