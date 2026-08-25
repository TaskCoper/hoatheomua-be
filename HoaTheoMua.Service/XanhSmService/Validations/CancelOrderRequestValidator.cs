using FluentValidation;

namespace HoaTheoMua.Service.XanhSmService.Validations;

public sealed class CancelOrderRequestValidator : AbstractValidator<Request.CancelOrderRequest>
{
    /// <summary>
    /// Kiểm tra dữ liệu hủy đơn theo contract Xanh SM: order id và lý do là bắt buộc,
    /// cancel code đối tác cấp cho partner hiện tại là 9000.
    /// </summary>
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("ORDER_ID_REQUIRED");
        RuleFor(x => x.CancelCode).Equal(9000).WithMessage("CANCEL_CODE_INVALID");
        RuleFor(x => x.CancelMessage).Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("CANCEL_MESSAGE_REQUIRED");
    }
}
