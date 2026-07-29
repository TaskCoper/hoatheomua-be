using HoaTheoMua.Service.Exceptions;

namespace HoaTheoMua.Service.Geocode;

public class VietMapException : AppException
{
    public string ReasonCode => MessageCode;

    public VietMapException(string reasonCode, int statusCode, string message = "")
        : base("VietMap Error", statusCode, reasonCode, message)
    {
    }
}
