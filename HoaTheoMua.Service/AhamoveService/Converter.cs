using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Service.AhamoveService;

public class NhanhWarrantyConverter : JsonConverter<NhanhWarranty>
{
    public override NhanhWarranty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // If the token is anything other than the start of an object (e.g., StartArray '[' or String)
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            // Skip the entire token/structure (like an empty array []) so the reader stays on track
            reader.Skip();
            return null;
        }

        // Otherwise, deserialize normally using a temporary options instance without this converter
        // to prevent infinite recursion loop
        var newOptions = new JsonSerializerOptions(options);
        return JsonSerializer.Deserialize<NhanhWarranty>(ref reader, newOptions);
    }

    public override void Write(Utf8JsonWriter writer, NhanhWarranty value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}