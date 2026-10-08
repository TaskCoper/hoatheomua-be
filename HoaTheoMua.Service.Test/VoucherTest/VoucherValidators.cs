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
            { Ids = [id, id], IsActive = true }).Errors, x => x.ErrorMessage == "Duplicate voucher id");
        Assert.Contains(validator.Validate(new Request.SetVouchersActiveRequest
            { Ids = Enumerable.Range(0, 1001).Select(_ => Guid.NewGuid()).ToList(), IsActive = false }).Errors,
            x => x.ErrorMessage == "Ids must not exceed 1000");
    }

    [Fact]
    public void EstimateValidator_RequiresCodeAndPositiveFee()
    {
        var validator = new EstimateVoucherRequestValidator();
        Assert.False(validator.Validate(new Request.EstimateVoucherRequest()).IsValid);
        Assert.True(validator.Validate(new Request.EstimateVoucherRequest
            { Code = "SAVE10", ProductFee = 100_000 }).IsValid);
    }

    private static Request.CreateVoucherRequest ValidCreate() => new()
    {
        Code = "SAVE10", Visibility = VoucherVisibilities.Public, DiscountType = VoucherDiscountTypes.Percent,
        DiscountValue = 10, MaxDiscountAmount = 50_000, MaxUsageCount = 5,
        StartAt = DateTimeOffset.UtcNow.AddHours(-1), EndAt = DateTimeOffset.UtcNow.AddDays(1),
        MinOrderAmount = 100_000, MaxOrderAmount = 2_000_000
    };
}
