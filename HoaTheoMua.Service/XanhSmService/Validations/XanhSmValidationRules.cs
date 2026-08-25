using System.Text.RegularExpressions;
using FluentValidation;

namespace HoaTheoMua.Service.XanhSmService.Validations;

internal static class XanhSmValidationRules
{
    private static readonly Regex InternationalPhone = new("^\\+\\d{8,15}$", RegexOptions.Compiled);
    internal static readonly string[] PackageSizes =
    [
        "SMALL", "MEDIUM", "LARGE",
        "VAN_SMALL", "VAN_MEDIUM", "VAN_LARGE"
    ];
    internal static readonly int[] AddonIds = [4, 5, 9, 11];

    internal sealed class RoutePointValidator : AbstractValidator<Request.RoutePoint>
    {
        /// <summary>
        /// Kiểm tra các field chung cho mọi route point: loại điểm, tọa độ, người liên hệ và địa chỉ.
        /// Các field package/COD không đặt ở đây vì chúng phụ thuộc PICK UP hay DROP OFF.
        /// </summary>
        public RoutePointValidator()
        {
            RuleFor(x => x.Type)
                .Must(x => x is "PICK UP" or "DROP OFF")
                .WithMessage("POINT_TYPE_INVALID");
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90)
                .WithMessage("LATITUDE_INVALID");
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("LONGITUDE_INVALID");
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(256)
                .WithMessage("NAME_INVALID");
            RuleFor(x => x.PhoneNumber)
                .Must(x => InternationalPhone.IsMatch(x))
                .WithMessage("PHONE_NUMBER_INVALID");
            RuleFor(x => x.ShortAddress)
                .NotEmpty()
                .MaximumLength(512)
                .WithMessage("SHORT_ADDRESS_INVALID");
            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(512)
                .WithMessage("ADDRESS_INVALID");
            RuleFor(x => x.AddressNote)
                .MaximumLength(512)
                .WithMessage("ADDRESS_NOTE_TOO_LONG");
        }
    }

    /// <summary>
    /// Kiểm tra rule liên trường mà FluentValidation rule đơn lẻ không thể hiện rõ:
    /// phải có pickup/dropoff, package chỉ ở pickup, COD khớp remain_cod/collect_cod,
    /// Door-to-door một dropoff, Bulky đúng kích cỡ và điểm thanh toán cash hợp lệ.
    /// </summary>
    /// <param name="points">Toàn bộ route points của estimate hoặc create order.</param>
    /// <param name="addons">Addon lấy từ service_ids khi estimate hoặc root request khi create.</param>
    /// <param name="validatePaymentCheckpoint">Chỉ true ở create order vì estimate không có quy tắc người trả phí.</param>
    /// <param name="addFailure">Callback thêm lỗi vào FluentValidation context đang chạy.</param>
    internal static void ValidateRouteAndAddons(List<Request.RoutePoint> points, IEnumerable<Request.Addon> addons,
        bool validatePaymentCheckpoint, Action<string, string> addFailure)
    {
        // Tách pickup/drop-off trước để kiểm tra các rule chỉ áp dụng cho từng loại điểm.
        var pickups = points.Where(x => x.Type == "PICK UP").ToList();
        var dropoffs = points.Where(x => x.Type == "DROP OFF").ToList();
        if (pickups.Count == 0)
            addFailure("points", "PICKUP_POINT_REQUIRED");
        if (dropoffs.Count == 0)
            addFailure("points", "DROPOFF_POINT_REQUIRED");

        foreach (var (point, index) in pickups.Select((value, index) => (value, index)))
        {
            var path = $"points[{index}]";

            // Thông tin kiện hàng chỉ bắt buộc tại điểm lấy hàng.
            if (point.PackageWeight is null || point.PackageWeight < 0)
                addFailure($"{path}.packageWeight", "PACKAGE_WEIGHT_INVALID");
            if (string.IsNullOrWhiteSpace(point.PackageSize) || !PackageSizes.Contains(point.PackageSize))
                addFailure($"{path}.packageSize", "PACKAGE_SIZE_INVALID");
            if (point.PackageTypes is not { Count: > 0 } || point.PackageTypes.Any(string.IsNullOrWhiteSpace))
                addFailure($"{path}.packageTypes", "PACKAGE_TYPES_REQUIRED");

            // addons tại point chỉ là object COD và chỉ hợp lệ tại DROP OFF.
            if (point.Addons is not null)
                addFailure($"{path}.addons", "POINT_ADDON_ONLY_ALLOWED_AT_DROPOFF");
        }

        var addonList = addons.ToList();
        foreach (var addon in addonList)
        {
            if (!AddonIds.Contains(addon.Id))
                addFailure("addons", "ADDON_ID_INVALID");
            if (!addon.Apply)
                addFailure("addons", "ADDON_APPLY_MUST_BE_TRUE");
        }

        var codAddons = addonList.Where(x => x.Id is 4 or 11).ToList();
        foreach (var cod in codAddons)
        {
            if (cod.RemainCod is null || cod.RemainCod < 0)
                addFailure("addons", "COD_REMAIN_AMOUNT_INVALID");
            if (cod.Value != 1)
                addFailure("addons", "COD_VALUE_MUST_BE_ONE");
            // Số COD trên addon và ở DROP OFF phải hoàn toàn trùng nhau.
            var matchingDropoff = dropoffs.SingleOrDefault(x => x.Addons is { } pointAddon
                && pointAddon.CollectCod == cod.RemainCod && pointAddon.CurrencyCode == "VND");
            if (matchingDropoff is null)
                addFailure("points", "COD_AMOUNT_MUST_MATCH_DROPOFF");
        }

        if (addonList.Any(x => x.Id == 5) && dropoffs.Count != 1)
            addFailure("addons", "DOOR_TO_DOOR_REQUIRES_ONE_DROPOFF");

        if (addonList.Any(x => x.Id == 9)
            && pickups.Any(x => x.PackageSize != "LARGE" || x.PackageWeight is < 31 or > 50))
        {
            addFailure("addons", "BULKY_ITEM_REQUIREMENTS_NOT_MET");
        }

        if (validatePaymentCheckpoint)
        {
            // Với cash: hoặc bỏ trống toàn bộ để dùng mặc định, hoặc khai báo cho mọi point với đúng một true.
            var explicitlySet = points.Where(x => x.IsPaymentCheckPoint.HasValue).ToList();
            var isInvalid = explicitlySet.Count > 0
                && (explicitlySet.Count != points.Count || explicitlySet.Count(x => x.IsPaymentCheckPoint == true) != 1);

            if (isInvalid)
                addFailure("points", "PAYMENT_CHECKPOINT_INVALID");
        }
    }

}
