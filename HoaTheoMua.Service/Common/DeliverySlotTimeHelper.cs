namespace HoaTheoMua.Service.Common;

public static class DeliverySlotTimeHelper
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public static bool IsWithinSlot(string? label, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return false;
        }

        var hourMatches = System.Text.RegularExpressions.Regex.Matches(
            label,
            @"(?<!\d)(?<hour>[01]?\d|2[0-3])\s*h",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (hourMatches.Count < 2 ||
            !int.TryParse(hourMatches[0].Groups["hour"].Value, out var startHour) ||
            !int.TryParse(hourMatches[1].Groups["hour"].Value, out var endHour) ||
            startHour == endHour)
        {
            return false;
        }

        var vietnamTime = TimeOnly.FromDateTime(
            createdAt.ToOffset(VietnamOffset).DateTime);
        var startTime = new TimeOnly(startHour, 0);
        var endTime = new TimeOnly(endHour, 0);

        return startTime < endTime
            ? vietnamTime >= startTime && vietnamTime < endTime
            : vietnamTime >= startTime || vietnamTime < endTime;
    }
}
