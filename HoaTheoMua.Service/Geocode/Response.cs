using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Geocode;

public class Response
{
    public class AutocompleteEnvelope
    {
        [JsonPropertyName("results")]
        public List<AutocompleteResult> Results { get; set; } = [];

        [JsonPropertyName("reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Reason { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; } = 200;
    }

    public class PlaceEnvelope
    {
        [JsonPropertyName("result")]
        public PlaceResult? Result { get; set; }

        [JsonPropertyName("reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Reason { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; } = 200;
    }

    public class AutocompleteResult
    {
        public string RefId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    public class PlaceResult
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string MapId { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }

    // Upstream VietMap Autocomplete Result model
    public class VietMapAutocompleteItem
    {
        [JsonPropertyName("ref_id")]
        public string RefId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("display")]
        public string Display { get; set; } = string.Empty;
    }

    // Upstream VietMap Place Result model
    public class VietMapPlaceResponse
    {
        [JsonPropertyName("display")]
        public string Display { get; set; } = string.Empty;

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }
}
