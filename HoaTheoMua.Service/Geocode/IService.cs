namespace HoaTheoMua.Service.Geocode;

public interface IService
{
    Task<List<Response.AutocompleteResult>> Autocomplete(Request.AutocompleteQuery query);
    Task<Response.PlaceResult?> GetPlaceDetail(string refId);
}
