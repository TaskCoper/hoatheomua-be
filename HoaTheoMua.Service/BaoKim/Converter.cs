using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.BaoKim;

public class BaokimDateTimeConverter : JsonConverter<DateTimeOffset>
{
    // Matches the format BaoKim expects on request_time (e.g. 2026-07-16T09:30:00.000Z).
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture));
}