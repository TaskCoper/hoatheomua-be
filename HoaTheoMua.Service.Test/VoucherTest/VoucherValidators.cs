using HoaTheoMua.Service.Voucher;
using HoaTheoMua.Service.Voucher.Validators;
using Xunit;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class VoucherValidatorsTest
{
    [Fact]
    public void CreateValidator_AcceptsValidManualAndGeneratedRequests()
    {
        var validator = new CreateVoucherRequestValidator();
        Assert.True(validator.Validate(ValidCreate()).IsValid);

        var generated = ValidCreate();
        generated.Code = null;
        generated.CodePrefix = "HT";
        generated.CodeLength = 6;
        generated.CodeSuffix = "26";
        Assert.True(validator.Validate(generated).IsValid);
    }

    [Fact]
    public void CreateValidator_RejectsConflictingCodeFieldsAndInvalidBusinessValues()
    {
        var request = ValidCreate();
        request.CodePrefix = "HT";
        request.CodeLength = 6;
        request.DiscountValue = 101;
        request.MaxUsageCount = 0;
        request.EndAt = request.StartAt;

        var result = new CreateVoucherRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.Code));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.DiscountValue));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.MaxUsageCount));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(request.EndAt));
    }

    [Fact]
    public void SetActiveValidator_RejectsEmptyDuplicateOversizedAndMissingState()
    {
        var validator = new SetVouchersActiveRequestValidator();
        Assert.False(validator.Validate(new Request.SetVouchersActiveRequest()).IsValid);
        var id = Guid.NewGuid();
        Assert.Contains(validator.Validate(new Request.SetVouchersActiveRequest
            { Ids = [id, id], IsActive = true }).Errors, x => x.ErrorMessage == "Danh sách đã chọn có voucher bị lặp. Vui lòng kiểm tra lại.");
        Assert.Contains(validator.Validate(new Request.SetVouchersActiveRequest
            { Ids = Enumerable.Range(0, 1001).Select(_ => Guid.NewGuid()).ToList(), IsActive = false }).Errors,
            x => x.ErrorMessage == "Mỗi lần chỉ được đổi trạng thái tối đa 1.000 voucher.");
    }

    [Fact]
    public void EstimateValidator_RequiresCodeAndPositiveFee()
    {
        var validator = new EstimateVoucherRequestValidator();
        Assert.False(validator.Validate(new Request.EstimateVoucherRequest()).IsValid);
        Assert.True(validator.Validate(new Request.EstimateVoucherRequest
            { code = "SAVE10", productFee = 100_000 }).IsValid);
    }

    [Theory]
    [InlineData(0, null, null)]
    [InlineData(null, 0, null)]
    [InlineData(0, 1, null)]
    [InlineData(-1, null, "Giá trị đơn hàng tối thiểu phải từ 0 trở lên.")]
    [InlineData(null, -1, "Giá trị đơn hàng tối đa phải từ 0 trở lên.")]
    [InlineData(0, 0, "Giá trị đơn hàng tối đa phải lớn hơn giá trị tối thiểu.")]
    [InlineData(200_000, 200_000, "Giá trị đơn hàng tối đa phải lớn hơn giá trị tối thiểu.")]
    public void OrderAmounts_AllowZero_RequireMaxAboveMin_InCreateAndUpdate(int? min, int? max, string? error)
    {
        var create = ValidCreate();
        create.MinOrderAmount = min;
        create.MaxOrderAmount = max;
        var update = new Request.UpdateVouchersRequest
        {
            Items = [new() { Id = Guid.NewGuid(), MinOrderAmount = min, MaxOrderAmount = max }]
        };

        var createResult = new CreateVoucherRequestValidator().Validate(create);
        var updateResult = new UpdateVouchersRequestValidator().Validate(update);

        if (error == null)
        {
            Assert.True(createResult.IsValid);
            Assert.True(updateResult.IsValid);
            return;
        }
        Assert.Equal(error, Assert.Single(createResult.Errors).ErrorMessage);
        Assert.Equal(error, Assert.Single(updateResult.Errors).ErrorMessage);
    }

    private static Request.CreateVoucherRequest ValidCreate() => new()
    {
        Code = "SAVE10", Visibility = VoucherVisibilities.Public, DiscountType = VoucherDiscountTypes.Percent,
        DiscountValue = 10, MaxDiscountAmount = 50_000, MaxUsageCount = 5,
        StartAt = DateTimeOffset.UtcNow.AddHours(-1), EndAt = DateTimeOffset.UtcNow.AddDays(1),
        MinOrderAmount = 100_000, MaxOrderAmount = 2_000_000
    };
}
