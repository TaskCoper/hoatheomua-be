using System.Text.RegularExpressions;
using FluentValidation;

namespace HoaTheoMua.Service.Common;

public static class ValidationRules
{
    public const string VnLocalPhoneRegex = @"^0\d{9,10}$";
    private const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public static IRuleBuilderOptions<T, string?> PhoneNumberVn<T>(this IRuleBuilder<T, string?> rule)
        => rule
            .NotEmpty().WithMessage("PHONE_NUMBER_REQUIRED")
            .Matches(VnLocalPhoneRegex).WithMessage("PHONE_NUMBER_INVALID");
    public static IRuleBuilderOptions<T, string?> OptionalPhoneNumberVn<T>(this IRuleBuilder<T, string?> rule)
        => rule
            .Must(v => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v.Trim(), VnLocalPhoneRegex))
            .WithMessage("PHONE_NUMBER_INVALID");
    public static IRuleBuilderOptions<T, string?> OptionalEmail<T>(this IRuleBuilder<T, string?> rule)
        => rule
            .Must(v => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v.Trim(), EmailRegex))
            .WithMessage("EMAIL_INVALID");
    public static IRuleBuilderOptions<T, string?> HttpUrl<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
}
