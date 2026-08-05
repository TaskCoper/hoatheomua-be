namespace HoaTheoMua.Service.Geocode;

public class VietMapException : Exception
{
    public string ReasonCode { get; }
    public int StatusCode { get; }

    public VietMapException(string reasonCode, int statusCode, string message = "") : base(message)
    {
        ReasonCode = reasonCode;
        StatusCode = statusCode;
    }
}
