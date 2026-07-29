namespace HoaTheoMua.Service.Geocode;

public interface IService
{
    Task<Response.AutocompleteEnvelope> Autocomplete(Request.AutocompleteQuery query);
    Task<Response.PlaceEnvelope> GetPlaceDetail(string refId);
}
