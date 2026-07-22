using System.Text.Json.Serialization;

namespace HoaTheoMua.Repository.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CategoryProductType
{
    Fix,
    Campaign
}
