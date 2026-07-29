namespace HoaTheoMua.Service.Geocode;

public class Request
{
    public class AutocompleteQuery
    {
        public string Text { get; set; } = string.Empty;
        public string? Focus { get; set; }
        public string? Ward { get; set; }
        public string? City { get; set; }
    }

    public class PlaceQuery
    {
        public string RefId { get; set; } = string.Empty;
    }
}
