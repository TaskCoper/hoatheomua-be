using System.Text.RegularExpressions;
using HoaTheoMua.Service.Exceptions;

namespace HoaTheoMua.Service.PostInsights;

public sealed class AccountInsightsRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Metrics { get; set; }
    public string Period { get; set; } = "day";
    public string? MetricType { get; set; }

    public (DateTimeOffset Since, DateTimeOffset Until, string[] Metrics) Validate(DateTimeOffset now)
    {
        if (FromDate == null || ToDate == null || FromDate > ToDate
            || ToDate > DateOnly.FromDateTime(now.UtcDateTime) || ToDate.Value.DayNumber - FromDate.Value.DayNumber >= 30)
            throw new BadRequestException("fromDate và toDate bắt buộc, tối đa 30 ngày, không vượt ngày hiện tại (UTC).");
        if (Period is not ("day" or "week" or "days_28" or "month" or "total_over_range"))
            throw new BadRequestException("period không hợp lệ; không hỗ trợ lifetime cho báo cáo theo khoảng.");
        if (MetricType is not (null or "total_value" or "time_series"))
            throw new BadRequestException("metricType phải là total_value hoặc time_series.");
        var metrics = (Metrics ?? "").Split(',').Select(x => x.Trim()).Distinct().ToArray();
        if (metrics.Length is < 1 or > 10 || metrics.Any(x => !Regex.IsMatch(x, @"\A[a-z][a-z0-9_]{0,99}\z")))
            throw new BadRequestException("metrics bắt buộc: từ 1 đến 10 tên metric Meta, phân cách bằng dấu phẩy.");
        return (new(FromDate.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new(ToDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), metrics);
    }
}
