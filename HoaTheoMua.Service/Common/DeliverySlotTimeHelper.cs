namespace HoaTheoMua.Service.Common;

public static class DeliverySlotTimeHelper
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public static bool TryGetHours(
        string? label,
        out TimeOnly start,
        out TimeOnly end)
    {
        start = default;
        end = default;

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

        start = new TimeOnly(startHour, 0);
        end = new TimeOnly(endHour, 0);
        return true;
    }

    public static bool ContainsTime(string? label, TimeOnly time)
    {
        if (!TryGetHours(label, out var start, out var end))
        {
            return false;
        }

        return start < end
            ? time >= start && time <= end
            : time >= start || time <= end;
    }

    public static bool IsWithinSlot(string? label, DateTimeOffset createdAt)
    {
        var vietnamTime = TimeOnly.FromDateTime(
            createdAt.ToOffset(VietnamOffset).DateTime);

        return ContainsTime(label, vietnamTime);
    }
}
