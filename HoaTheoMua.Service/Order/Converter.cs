
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Order;

public class Converter : JsonConverter<List<OrderData>>
{
    public override List<OrderData> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Trường hợp 1: Dữ liệu là một Array chuẩn [ { ... }, { ... } ]
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<OrderData>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return list;

                // Deserialize từng phần tử OrderData
                var item = JsonSerializer.Deserialize<OrderData>(ref reader, options);
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
            return new List<OrderData>();
        }

        // Các trường hợp Token khác (gặp Null, String trống,...)
        return new List<OrderData>();
    }

    public override void Write(Utf8JsonWriter writer, List<OrderData> value, JsonSerializerOptions options)
    {
        // Ghi đè phương thức serialize khi cần trả data ngược lại
        JsonSerializer.Serialize(writer, value, options);
    }
}