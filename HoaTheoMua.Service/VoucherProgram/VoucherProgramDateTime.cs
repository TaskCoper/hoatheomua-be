using System.Globalization;
using System.Text.RegularExpressions;

namespace HoaTheoMua.Service.VoucherProgram;

internal static class VoucherProgramDateTime
{
    public static bool TryParse(string? value, out DateTimeOffset result)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('T')
            || !Regex.IsMatch(value.Trim(), @"(?:Z|[+-]\d{2}:\d{2})$", RegexOptions.CultureInvariant))
        {
            result = default;
            return false;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind, out result);
    }

    public static DateTimeOffset Parse(string value)
    {
        if (TryParse(value, out var result))
            return result;

        throw new InvalidOperationException("Voucher program date-time was not validated.");
    }
}
