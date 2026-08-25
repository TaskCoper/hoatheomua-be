using HoaTheoMua.Service.Exceptions;

namespace HoaTheoMua.Service.XanhSmService;

/// <summary>Lỗi Xanh SM đã được chuẩn hóa để không lộ token, secret hoặc lỗi hạ tầng cho FE.</summary>
public sealed class XanhSmException : AppException
{
    /// <summary>
    /// Tạo lỗi tích hợp với HTTP status công khai, mã để FE phân loại và message an toàn để hiển thị/log.
    /// Không truyền raw response của Xanh SM vào đây vì response có thể chứa thông tin nội bộ.
    /// </summary>
    public XanhSmException(int statusCode, string messageCode, string detail)
        : base(GetTitle(statusCode), statusCode, messageCode, detail)
    {
    }

    // Title dùng cho error envelope; MessageCode mới là mã chi tiết để FE xử lý từng trường hợp.
    private static string GetTitle(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        404 => "Not Found",
        409 => "Conflict",
        429 => "Too Many Requests",
        503 => "Service Unavailable",
        504 => "Gateway Timeout",
        _ when statusCode >= 500 => "Bad Gateway",
        _ => "Integration Error"
    };
}
