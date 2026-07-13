namespace HoaTheoMua.Service.Exceptions;

public class ValidationError
{
    public string Field { get; set; } = default!;
    public string Message { get; set; } = default!;
}