using HoaTheoMua.Service.VoucherProgram;
using HoaTheoMua.Service.VoucherProgram.Validate;
using Xunit;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class GenerateProgramVouchersValidator
{
    [Fact]
    public void Update_ReportsNegativeMinAndMaxAsSeparateFieldErrors()
    {
        var request = new Request.UpdateVoucherProgramRequest { MinOrderAmount = -1, MaxOrderAmount = -2 };

        var result = new UpdateVoucherProgramRequestValidator().Validate(request);

        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, error => error.PropertyName == "MinOrderAmount"
            && error.ErrorMessage == "Min order amount must not be negative");
        Assert.Contains(result.Errors, error => error.PropertyName == "MaxOrderAmount"
            && error.ErrorMessage == "Max order amount must not be negative");
    }

    [Fact]
    public void Generate_ReportsInvalidPrefixAndSuffixAsSeparateFieldErrors()
    {
        var request = GenerateRequest();
        request.CodePrefix = "htm";
        request.CodeSuffix = "-26";

        var result = new GenerateProgramVouchersRequestValidator().Validate(request);

        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, error => error.PropertyName == "CodePrefix"
            && error.ErrorMessage == "Code prefix must contain only uppercase letters or digits");
        Assert.Contains(result.Errors, error => error.PropertyName == "CodeSuffix"
            && error.ErrorMessage == "Code suffix must contain only uppercase letters or digits");
    }

    [Theory]
    [InlineData(5, 1, "cash")]
    [InlineData(10, 1000, "percent")]
    public async Task AcceptsValidBoundariesAndOptionalFields(int length, int quantity, string type)
    {
        var request = GenerateRequest(quantity);
        request.CodeLength = length;
        request.CodePrefix = new string('A', 20 - length);
        request.CodeSuffix = null;
        request.DiscountType = type;
        request.DiscountValue = type == "percent" ? 100 : 50000;
        request.MaxDiscountAmount = null;
        request.Description = null;

        Assert.True((await new GenerateProgramVouchersRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Theory]
    [InlineData("CodeLength", "4", "Code length must be at least 5")]
    [InlineData("CodeLength", "11", "Code length must not exceed 10")]
    [InlineData("CodeLength", "0", "Code length must be at least 5")]
    [InlineData("CodePrefix", "htm", "Code prefix must contain only uppercase letters or digits")]
    [InlineData("CodePrefix", "HTM\n", "Code prefix must contain only uppercase letters or digits")]
    [InlineData("CodeSuffix", "-26", "Code suffix must contain only uppercase letters or digits")]
    [InlineData("CodeSuffix", "AAAAAAAAAAAAAAAAAAAA", "Voucher code must not exceed 20 characters including prefix and suffix", "CodeLength")]
    [InlineData("Quantity", "0", "Quantity must be at least 1")]
    [InlineData("Quantity", "1001", "Quantity must not exceed 1000")]
    [InlineData("MaxUsageCount", "0", "Max usage count must be greater than 0")]
    [InlineData("DiscountType", "PERCENT", "Discount type must be cash or percent")]
    [InlineData("DiscountType", "", "Discount type is required")]
    [InlineData("DiscountValue", "0", "Discount value must be greater than 0")]
    [InlineData("DiscountValue", "101", "Discount percent must not exceed 100")]
    [InlineData("MaxDiscountAmount", "0", "Max discount amount must be greater than 0")]
    [InlineData("MaxDiscountAmount", "-1", "Max discount amount must be greater than 0")]
    [InlineData("DiscountType", "cash", "Max discount amount is only for percent", "MaxDiscountAmount")]
    [InlineData("Description", "long", "Description must not exceed 500 characters")]
    public async Task RejectsInvalidGenerationFields(string field, string value, string message, string? errorField = null)
    {
        var request = GenerateRequest();
        switch (field)
        {
            case "CodeLength": request.CodeLength = int.Parse(value); break;
            case "CodePrefix": request.CodePrefix = value; break;
            case "CodeSuffix": request.CodeSuffix = value; break;
            case "Quantity": request.Quantity = int.Parse(value); break;
            case "MaxUsageCount": request.MaxUsageCount = int.Parse(value); break;
            case "DiscountType": request.DiscountType = value; break;
            case "DiscountValue": request.DiscountValue = decimal.Parse(value); break;
            case "MaxDiscountAmount": request.MaxDiscountAmount = decimal.Parse(value); break;
            case "Description": request.Description = new string('A', 501); break;
        }

        var result = await new GenerateProgramVouchersRequestValidator().ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == (errorField ?? field) && error.ErrorMessage == message);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Update_RequiresAtLeastOneField_AcceptsBothBooleanValues(bool? active, bool valid)
    {
        var result = await new UpdateVoucherProgramRequestValidator()
            .ValidateAsync(new Request.UpdateVoucherProgramRequest { IsActive = active });

        Assert.Equal(valid, result.IsValid);
        if (!valid)
            Assert.Equal("At least one voucher program field must be provided for update", Assert.Single(result.Errors).ErrorMessage);
    }
}
