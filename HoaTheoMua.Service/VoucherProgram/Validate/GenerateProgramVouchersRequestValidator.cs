using FluentValidation;

namespace HoaTheoMua.Service.VoucherProgram.Validate;

public class GenerateProgramVouchersRequestValidator : AbstractValidator<Request.GenerateProgramVouchersRequest>
{
    public GenerateProgramVouchersRequestValidator()
    {
        RuleFor(x => x.CodeLength).InclusiveBetween(5, 10)
            .WithMessage(x => x.CodeLength < 5 ? "Phần mã ngẫu nhiên phải dài ít nhất 5 ký tự." : "Phần mã ngẫu nhiên không được dài quá 10 ký tự.");
        RuleFor(x => x.CodePrefix).Matches(@"\A[A-Z0-9]*\z")
            .WithMessage("Phần đầu mã chỉ được gồm chữ cái viết hoa hoặc chữ số.");
        RuleFor(x => x.CodeSuffix).Matches(@"\A[A-Z0-9]*\z")
            .WithMessage("Phần cuối mã chỉ được gồm chữ cái viết hoa hoặc chữ số.");
        RuleFor(x => x.CodeLength)
            .Must((request, length) => (long)(request.CodePrefix?.Length ?? 0) + length
                                      + (request.CodeSuffix?.Length ?? 0) <= 20)
            .WithMessage("Mã voucher không được dài quá 20 ký tự, tính cả phần đầu và phần cuối mã.");
        RuleFor(x => x.Quantity).InclusiveBetween(1, 1000)
            .WithMessage(x => x.Quantity < 1 ? "Số lượng voucher cần tạo phải từ 1 trở lên." : "Mỗi lần chỉ được tạo tối đa 1.000 voucher.");
        RuleFor(x => x.MaxUsageCount).GreaterThan(0)
            .WithMessage("Giới hạn lượt sử dụng phải lớn hơn 0.");
        RuleFor(x => x.DiscountType).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Vui lòng chọn loại giảm giá.")
            .Must(type => type is "cash" or "percent")
            .WithMessage("Loại giảm giá chỉ chấp nhận cash (giảm tiền) hoặc percent (giảm theo phần trăm).");
        RuleFor(x => x.DiscountValue).GreaterThan(0)
            .WithMessage("Mức giảm giá phải lớn hơn 0.");
        RuleFor(x => x.DiscountValue).LessThanOrEqualTo(100)
            .When(x => x.DiscountType == "percent")
            .WithMessage("Mức giảm theo phần trăm không được vượt quá 100%.");
        RuleFor(x => x.MaxDiscountAmount).Null()
            .When(x => x.DiscountType != "percent")
            .WithMessage("Chỉ đặt mức giảm tối đa khi giảm theo phần trăm.");
        RuleFor(x => x.MaxDiscountAmount).GreaterThan(0)
            .When(x => x.MaxDiscountAmount.HasValue)
            .WithMessage("Mức giảm tối đa phải lớn hơn 0.");
        RuleFor(x => x.Description).MaximumLength(500)
            .WithMessage("Mô tả không được dài quá 500 ký tự.");
    }
}
