namespace HoaTheoMua.Service.Exceptions;

public class AhamoveException : AppException
{
    public AhamoveException()
        : base("Khoảng cách không hợp lệ", 406, "INVALID_MAX_DISTANCE",
            "Khoảng cách không hợp lệ") { }
}